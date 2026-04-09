namespace Talentree.Service.DTOs.AccountSettings
{
    public class UpdatePreferencesDto
    {
        public string Timezone { get; set; } = "Africa/Cairo";
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public string CurrencyDisplay { get; set; } = "EGP";
        public string DashboardLayout { get; set; } = "default";
    }
}