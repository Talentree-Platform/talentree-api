namespace Talentree.Core.Settings
{
    /// <summary>
    /// Configuration options for the AI Help Center chatbot integration.
    /// </summary>
    public class AiHelpCenterOptions
    {
        /// <summary>
        /// Base URL for the external HuggingFace AI Help Center API.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
    }
}
