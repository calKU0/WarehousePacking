namespace WarehousePacking.API.Integrations.Couriers.DPD.DTOs
{
    public class DpdCreatePackageResponse
    {
        public string TraceId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long? SessionId { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ErrorsXml { get; set; }
        public List<DpdPackageResponse> Packages { get; set; } = new();
    }

    public class DpdPackageResponse
    {
        public string Status { get; set; } = string.Empty;      // maps to JSON "status"
        public string Reference { get; set; } = string.Empty;   // maps to JSON "reference"
        public List<DpdParcelResponse> Parcels { get; set; } = new();
        public List<DpdValidationInfo> ValidationInfo { get; set; } = new();
    }

    public class DpdParcelResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string? Waybill { get; set; }
        public List<DpdValidationInfo> ValidationInfo { get; set; } = new();
    }

    public class DpdValidationInfo
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string Info { get; set; } = string.Empty;
    }
}