namespace Talentree.Service.DTOs.Customer
{
    public class OrderPaymentDto
    {
        public string PaymentMethod { get; set; } = null!;
        public string? StripeClientSecret { get; set; }
        public int OrderId { get; set; }
    }
}
