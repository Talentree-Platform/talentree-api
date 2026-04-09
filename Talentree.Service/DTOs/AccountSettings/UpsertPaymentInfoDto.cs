namespace Talentree.Service.DTOs.AccountSettings
{
    public class UpsertPaymentInfoDto
    {
        public string CurrentPassword { get; set; } = string.Empty; // required to save
        public string BankName { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string RoutingSwiftCode { get; set; } = string.Empty;
    }
}