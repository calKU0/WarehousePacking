namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class DutiesPayment
    {
        public Payor? Payor { get; set; }
        public string? PaymentType { get; set; }
    }
}