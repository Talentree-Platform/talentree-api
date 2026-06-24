namespace Talentree.Core.Enums
{
    /// <summary>
    /// Allowed content types for Knowledge Base articles (FR-AD-41).
    /// Values are stored as strings in the database for readability.
    /// </summary>
    public static class ContentType
    {
        public const string Video = "Video";
        public const string PDF = "PDF";
        public const string Article = "Article";

        public static readonly IReadOnlyList<string> AllValues = new[]
        {
            Video, PDF, Article
        };

        public static bool IsValid(string? value) =>
            !string.IsNullOrEmpty(value) && AllValues.Contains(value);
    }
}
