
namespace Talentree.Service.DTOs.Admin
{
   
    public class RejectBusinessOwnerDto
    {
        public int ProfileId { get; set; }

        public string RejectionReason { get; set; } = string.Empty;
    }
}