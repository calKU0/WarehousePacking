namespace WarehousePacking.Shared.DTOs.Requests
{
    public class UpdatePackageDimensionsRequest
    {
        public int PackageId { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
    }
}
