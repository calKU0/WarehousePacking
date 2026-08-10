using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Dashboards
{
    public class PersonalCollection
    {
        public string ClientCode { get; set; } = string.Empty;
        public string DocName { get; set; } = string.Empty;
        public int DocId { get; set; }
        public int DocErpId { get; set; }
        public int DocErpType { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRealized { get; set; }
        public decimal Weight { get; set; }
        public decimal WeightRealized { get; set; }
        public WarehouseTaskStatus Status { get; set; }
        public DateTime Date { get; set; }
        public string? JlCodes { get; set; }
    }
}
