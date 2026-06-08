using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Dashboards
{
    public class JlToPack
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        public PackingWarehouse Warehouse { get; set; }
        public PackingLevel Level { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public string CourierName { get; set; }
        public Courier Courier { get; set; }
    }
}
