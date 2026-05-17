namespace Talentree.Service.DTOs.UserManagement
{
    public class UserActionLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime ActionDate { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
    }
}