namespace Talentree.Service.DTOs.UserManagement
{
    public class UserActionLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime ActionDate { get; set; }
        public string? AdminName { get; set; }
        public string? AdminEmail { get; set; }
        public string? UserId { get; set; }
        public string? AdminId { get; set; }
        public string? IpAddress { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? BeforeValues { get; set; }
        public string? AfterValues { get; set; }
    }
}