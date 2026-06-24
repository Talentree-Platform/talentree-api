namespace Talentree.Service.DTOs.Knowledge
{
    /// <summary>
    /// FR-AD-43: Body sent by the BO frontend to record how long a user watched/read an article.
    /// Called from POST api/knowledge-base/{id}/view-duration
    /// </summary>
    public class RecordViewDurationDto
    {
        /// <summary>Number of seconds the user spent on the article.</summary>
        public int DurationSeconds { get; set; }
    }
}
