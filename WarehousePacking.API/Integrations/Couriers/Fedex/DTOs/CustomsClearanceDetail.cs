namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class CustomsClearanceDetail
    {
        public List<string>? RegulatoryControls { get; set; }
        public string? FreightOnValue { get; set; }
        public DutiesPayment? DutiesPayment { get; set; }
        public List<Commodity>? Commodities { get; set; }
        public bool? IsDocumentOnly { get; set; }
        public string? GeneratedDocumentLocale { get; set; }
        public TotalCustomsValue? TotalCustomsValue { get; set; }
        public bool? PartiesToTransactionAreRelated { get; set; }
    }
}