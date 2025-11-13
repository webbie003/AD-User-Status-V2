using System.DirectoryServices.Protocols;
using System.Net;

namespace AdUserStatus.Services
{
    public sealed class LdapService : IDisposable
    {
        private readonly LdapConnection _conn;
        private readonly string _baseDn = ""; // set after bind

        public LdapService(string server, bool useLdaps = true, NetworkCredential? cred = null)
        {
            var port = useLdaps ? 636 : 389;
            var id = new LdapDirectoryIdentifier(server, port, false, false);

            _conn = new LdapConnection(id)
            {
                AuthType = cred == null ? AuthType.Negotiate : AuthType.Basic,
                Timeout = TimeSpan.FromSeconds(5)
            };
            if (cred != null) _conn.Credential = cred;

            var sess = _conn.SessionOptions;
            sess.ProtocolVersion = 3;
            sess.SecureSocketLayer = useLdaps;
            sess.ReferralChasing = ReferralChasingOptions.Subordinate;

            if (!useLdaps)
            {
                sess.Signing = true;
                sess.Sealing = true;
            }

            _conn.Bind();

            // ---- Resolve defaultNamingContext ----
            var rootReq = new SearchRequest("", "(objectClass=*)", SearchScope.Base,
                                             "defaultNamingContext", "configurationNamingContext");
            var rootResp = (SearchResponse)_conn.SendRequest(rootReq);
            if (rootResp.Entries.Count == 0)
                throw new InvalidOperationException("RootDSE query returned no entries.");

            var rootEntry = rootResp.Entries[0];
            _baseDn = rootEntry.Attributes["defaultNamingContext"]?[0]?.ToString()
                      ?? throw new InvalidOperationException("Cannot determine defaultNamingContext from RootDSE.");
        }

        // --- helpers ----------------------------------------------------------

        private static string Esc(string s) => s
            .Replace("\\", "\\5c").Replace("*", "\\2a")
            .Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");

        private static string NormalizeEmail(string email) =>
            email.Trim().Replace("\u00A0", "").Replace(" ", "").ToLowerInvariant();

        private static string? GetAttr(SearchResultEntry e, string name) =>
            e.Attributes.Contains(name) && e.Attributes[name].Count > 0
                ? e.Attributes[name][0]?.ToString()
                : null;

        private static (bool found, bool? enabled, string? display, string? sam, string? upn)
            ProjectUser(SearchResultEntry? entry)
        {
            if (entry == null) return (false, null, null, null, null);

            string? display = GetAttr(entry, "displayName");
            string? sam = GetAttr(entry, "sAMAccountName");
            string? upn = GetAttr(entry, "userPrincipalName");

            bool? enabled = null;
            var uacStr = GetAttr(entry, "userAccountControl");
            if (int.TryParse(uacStr, out var uac))
                enabled = (uac & 0x2) == 0; // UF_ACCOUNTDISABLE

            return (true, enabled, display, sam, upn);
        }

        // --- lookups ----------------------------------------------------------

        /// <summary>
        /// Look up a user by email address, UPN, or any alias in proxyAddresses.
        /// Returns: found, enabled, displayName, sAMAccountName, userPrincipalName
        /// </summary>
        public (bool found, bool? enabled, string? displayName, string? sam, string? upn)
            FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return (false, null, null, null, null);

            var e = NormalizeEmail(email);
            var eEsc = Esc(e);

            // proxyAddresses values are like SMTP:primary@x or smtp:alias@x
            var filter =
                $"(&(objectCategory=person)(objectClass=user)" +
                $"(|(mail={eEsc})(userPrincipalName={eEsc})(proxyAddresses=SMTP:{eEsc})(proxyAddresses=smtp:{eEsc})))";

            var attrs = new[] { "displayName", "sAMAccountName", "userPrincipalName", "userAccountControl" };
            var req = new SearchRequest(_baseDn, filter, SearchScope.Subtree, attrs);
            var resp = (SearchResponse)_conn.SendRequest(req);

            var entry = resp.Entries.Cast<SearchResultEntry>().FirstOrDefault();
            return ProjectUser(entry);
        }

        /// <summary>
        /// Look up a user by sAMAccountName.
        /// Returns: found, enabled, displayName, sAMAccountName, userPrincipalName
        /// </summary>
        public (bool found, bool? enabled, string? displayName, string? sam, string? upn)
            FindBySamAccountName(string samAccountName)
        {
            if (string.IsNullOrWhiteSpace(samAccountName)) return (false, null, null, null, null);

            var samEsc = Esc(samAccountName.Trim());

            var filter =
                $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={samEsc}))";

            var attrs = new[] { "displayName", "sAMAccountName", "userPrincipalName", "userAccountControl" };
            var req = new SearchRequest(_baseDn, filter, SearchScope.Subtree, attrs);
            var resp = (SearchResponse)_conn.SendRequest(req);

            var entry = resp.Entries.Cast<SearchResultEntry>().FirstOrDefault();
            return ProjectUser(entry);
        }

        private static string DnToDns(string dn)
        {
            var labels = new List<string>();
            foreach (var rdn in dn.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = rdn.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 2 && kv[0].Trim().Equals("DC", StringComparison.OrdinalIgnoreCase))
                    labels.Add(kv[1].Trim());
            }
            return string.Join('.', labels);
        }

        /// <summary>
        /// Discover internal email/UPN domains in the forest.
        /// </summary>
        public HashSet<string> GetInternalDomains()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Current domain
            var currentDomain = DnToDns(_baseDn);
            if (!string.IsNullOrWhiteSpace(currentDomain)) set.Add(currentDomain);

            // Get configurationNamingContext
            var rootReq = new SearchRequest("", "(objectClass=*)", SearchScope.Base, "configurationNamingContext");
            var rootResp = (SearchResponse)_conn.SendRequest(rootReq);
            if (rootResp.Entries.Count == 0) return set;

            var configNc = rootResp.Entries[0].Attributes["configurationNamingContext"]?[0]?.ToString();
            if (string.IsNullOrWhiteSpace(configNc)) return set;

            var partitionsDn = $"CN=Partitions,{configNc}";

            // crossRef dnsRoot for domain naming contexts
            var crossReq = new SearchRequest(partitionsDn, "(objectClass=crossRef)", SearchScope.OneLevel, "dnsRoot", "nCName");
            var crossResp = (SearchResponse)_conn.SendRequest(crossReq);
            foreach (SearchResultEntry e in crossResp.Entries)
            {
                var nCName = e.Attributes["nCName"]?.Count > 0 ? e.Attributes["nCName"][0]?.ToString() : null;
                var dnsRoot = e.Attributes["dnsRoot"]?.Count > 0 ? e.Attributes["dnsRoot"][0]?.ToString() : null;

                if (!string.IsNullOrWhiteSpace(nCName) &&
                    nCName.StartsWith("DC=", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(dnsRoot))
                {
                    set.Add(dnsRoot!.ToLowerInvariant());
                }
            }

            // Forest UPN suffixes
            var upnReq = new SearchRequest(partitionsDn, "(objectClass=*)", SearchScope.Base, "uPNSuffixes");
            var upnResp = (SearchResponse)_conn.SendRequest(upnReq);
            var upnAttr = upnResp.Entries.Count > 0 ? upnResp.Entries[0].Attributes["uPNSuffixes"] : null;
            if (upnAttr != null)
            {
                foreach (var v in upnAttr) set.Add(v.ToString()!.ToLowerInvariant());
            }

            return set;
        }

        public void Dispose() => _conn.Dispose();
    }
}
