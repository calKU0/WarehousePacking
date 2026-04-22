namespace WarehousePacking.Contracts.DTOs.Couriers.Fedex
{
    public class DutiesPayment
    {
        public Payor? Payor { get; set; }
        public string? PaymentType { get; set; }
    }
}