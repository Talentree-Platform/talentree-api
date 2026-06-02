namespace Talentree.Core.Enums
{
    public enum NotificationType
    {
        System = 0,                 // System announcements, maintenance
        Account = 1,                // Account actions (suspend, ban, approve, etc)
        Order = 2,                  // Order placement, confirmation, delivery
        Product = 3,                // Product approval, rejection, review
        Support = 4,                // Support ticket updates
        Payment = 5,                // Payment success/failure
        Review = 6,                 // Customer review responses
        ProductionRequest = 7,      // Production workflow
        Payout = 8,                 // Payout updates
        Complaint = 9,              // Complaint resolution
        AutoBlock = 10   ,           // Automatic blocking notifications
            MaterialOrder = 11        // Material order updates
    }
}