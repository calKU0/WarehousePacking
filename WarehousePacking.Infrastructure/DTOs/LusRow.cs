using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Infrastructure.DTOs
{
    internal class LusRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public int? ZoneId { get; set; }
        public string? Zone { get; set; }
        public PackingWarehouse? Warehouse { get; set; }
        public string Couriers { get; set; } = "";
        public DateTime? LastOperationDate { get; set; }
        public string? LastOperationOperator { get; set; }
        public int ProductsCount { get; set; }
        public decimal ProductsSum { get; set; }
        public decimal Weight { get; set; }
    }
}
