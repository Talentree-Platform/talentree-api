namespace Talentree.Service.DTOs.Admin
{
    /// <summary>
    /// DTO for approving business owner application
    /// </summary>
    public class ApproveBusinessOwnerDto
    {
        /// <summary>
        /// Business owner profile ID
        /// </summary>
        public int ProfileId { get; set; }

        /// <summary>
        /// Optional approval notes/comments
        /// </summary>
        public string? Notes { get; set; }
    }
}