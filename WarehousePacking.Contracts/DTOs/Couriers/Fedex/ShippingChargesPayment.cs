namespace WarehousePacking.Contracts.DTOs.Couriers.Fedex
{
    public class ShippingChargesPayment
    {
        public string? PaymentType { get; set; }
        public Payor? Payor { get; set; }
    }
}