namespace Talentree.Service.DTOs.AccountSettings
{
    public class PaymentInfoDto
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string MaskedAccountNumber { get; set; } = string.Empty; // "****1234"
        public string RoutingSwiftCode { get; set; } = string.Empty;
        public DateTime? LastChangedAt { get; set; }
    }
}