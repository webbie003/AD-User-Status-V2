using System.DirectoryServices.ActiveDirectory;

namespace AdUserStatus.Services
{
    public static class DcDiscovery
    {
        public static string GetPreferredDomainController()
        {
            var domain = Domain.GetCurrentDomain();
            return domain.PdcRoleOwner?.Name?.ToUpperInvariant() ?? domain.Name;
        }
    }
}
