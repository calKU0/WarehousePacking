namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class ShippingChargesPayment
    {
        public string? PaymentType { get; set; }
        public Payor? Payor { get; set; }
    }
}