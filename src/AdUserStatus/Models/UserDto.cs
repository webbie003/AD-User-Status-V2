namespace AdUserStatus.Models
{
    public class UserDto
    {
        public string SamAccountName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public bool? Enabled { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
