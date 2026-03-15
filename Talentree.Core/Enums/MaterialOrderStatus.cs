namespace Talentree.Core.Enums
{
    /// <summary>
    /// Fulfilment status of a Business Owner's raw-material purchase order.
    /// Set by Talentree operations staff after the order is placed.
    /// </summary>
    public enum MaterialOrderStatus
    {
        /// <summary>Order placed and awaiting Talentree's confirmation.</summary>
        Pending = 0,

        /// <summary>Talentree has confirmed the order and is preparing it for dispatch.</summary>
        Processing = 1,

        /// <summary>Order has been dispatched and is in transit to the BO.</summary>
        Shipped = 2,

        /// <summary>Order successfully delivered to the Business Owner.</summary>
        Delivered = 3,

        /// <summary>Order was cancelled before dispatch.</summary>
        Cancelled = 4,

        /// <summary>A refund has been issued for this order.</summary>
        Refunded = 5
    }
}