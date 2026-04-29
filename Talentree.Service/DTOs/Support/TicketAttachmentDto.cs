namespace Talentree.Service.DTOs.Support
{
    public class TicketAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileSizeMB { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }
}