namespace Talentree.Core.Enums
{
    public enum CustomerOrderStatus
    {
        Pending = 0,        // Awaiting payment confirmation
        Processing = 1,     // Payment confirmed, seller preparing order
        Shipped = 2,        // Handed to courier, tracking number assigned
        OutForDelivery = 3, // Last-mile delivery
        Delivered = 4,      // Customer received
        Cancelled = 5,      // Cancelled before shipping
        Refunded = 6        // Refund processed
    }
}
