namespace Talentree.Service.Contracts
{
    public interface IAIService
    {
        // Called when new review is submitted
        Task PredictSentimentAsync(int reviewId);

        // Called when new support ticket is created
        Task PredictTriageAsync(int ticketId);

        // Called when product is created or updated
        Task ComputeProductAsync(int productId);

        // Called when BO profile is updated
        Task ComputeProfileAsync(string boUserId);

        // Called when BO logs in
        Task PredictChurnAsync(string userId);
    }
}