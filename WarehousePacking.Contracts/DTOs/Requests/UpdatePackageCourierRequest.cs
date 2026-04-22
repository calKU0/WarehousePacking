using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class UpdatePackageCourierRequest
    {
        public int PackageId { get; set; }
        public int? DocumentId { get; set; }
        public Courier Courier { get; set; }
    }
}