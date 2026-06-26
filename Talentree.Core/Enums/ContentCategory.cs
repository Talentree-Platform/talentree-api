namespace Talentree.Core.Enums
{
    /// <summary>
    /// Allowed category values for Knowledge Base articles.
    /// Merges the FR-AD-40 spec categories with existing BO recommendation categories.
    /// Values are stored as strings in the database for readability.
    /// </summary>
    public static class ContentCategory
    {
        // FR-AD-40 categories
        public const string Business = "Business";
        public const string Craft = "Craft";
        public const string Marketing = "Marketing";
        public const string PlatformGuide = "PlatformGuide";

        // Existing BO recommendation categories (kept for backward compat)
        public const string GettingStarted = "GettingStarted";
        public const string Inventory = "Inventory";

        public static readonly IReadOnlyList<string> AllValues = new[]
        {
            Business, Craft, Marketing, PlatformGuide, GettingStarted, Inventory
        };

        public static bool IsValid(string? value) =>
            !string.IsNullOrEmpty(value) && AllValues.Contains(value);
    }
}
