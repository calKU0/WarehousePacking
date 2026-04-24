using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class WarehouseDocument
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int ErpId { get; set; }
        public int ErpType { get; set; }
        public Courier Courier { get; set; }
        public WarehouseTaskStatus Status { get; set; }
        public WarehouseDocumentType Type { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRealized { get; set; }
        public decimal Weight { get; set; }
        public decimal WeightRealized { get; set; }
        public decimal Volume { get; set; }
        public decimal VolumeRealized { get; set; }
    }
}
