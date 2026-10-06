using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class ShipmentResponse
    {
        public bool Success { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// The courier refused the package's own data (address, postal code,
        /// phone...), as opposed to a technical failure such as a timeout.
        /// Only this kind is reported to the representative.
        /// </summary>
        public bool IsInvalidPackageData { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string TrackingLink { get; set; } = string.Empty;
        public string LabelBase64 { get; set; } = string.Empty;
        public PrintDataType LabelType { get; set; }
        public int PackageId { get; set; }
        public int ErpShipmentId { get; set; }
        public Courier Courier { get; set; }
        public PackageData PackageInfo { get; set; } = new();

        public static ShipmentResponse CreateFailure(string error, bool isInvalidPackageData = false) => new()
        {
            Success = false,
            ErrorMessage = error,
            IsInvalidPackageData = isInvalidPackageData
        };

        public static ShipmentResponse CreateSuccess(
            Courier courier,
            int packageId,
            string trackingNumber,
            string trackingLink,
            string labelBase64,
            PackageData packageInfo,
            PrintDataType labelType,
            string externalId) => new()
            {
                Success = true,
                Courier = courier,
                PackageId = packageId,
                TrackingNumber = trackingNumber,
                TrackingLink = trackingLink,
                LabelBase64 = labelBase64,
                LabelType = labelType,
                PackageInfo = packageInfo,
                ExternalId = externalId
            };
    }
}