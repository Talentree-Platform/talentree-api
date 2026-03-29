namespace Talentree.Service.DTOs.AccountSettings
{
    public class PreferencesDto
    {
        public string Timezone { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
        public string CurrencyDisplay { get; set; } = string.Empty;
        public string DashboardLayout { get; set; } = string.Empty;
    }
}