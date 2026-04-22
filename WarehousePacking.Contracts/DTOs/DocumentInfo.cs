using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class DocumentInfo
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string DocumentName { get; set; }
        public int AddressId { get; set; }
        public int AddressType { get; set; }
        public int ClientId { get; set; }
        public string CourierName { get; set; }
        public Courier Courier { get; set; }
        public List<DocumentElement>? Elements { get; set; }
    }
}