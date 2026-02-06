using WarehousePacking.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class ShipmentResponse
    {
        public bool Success { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string TrackingLink { get; set; } = string.Empty;
        public string LabelBase64 { get; set; } = string.Empty;
        public PrintDataType LabelType { get; set; }
        public int PackageId { get; set; }
        public int ErpShipmentId { get; set; }
        public Courier Courier { get; set; }
        public PackageData PackageInfo { get; set; } = new();

        public static ShipmentResponse CreateFailure(string error) => new()
        {
            Success = false,
            ErrorMessage = error
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