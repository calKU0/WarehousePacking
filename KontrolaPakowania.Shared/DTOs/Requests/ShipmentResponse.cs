using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class ShipmentResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string LabelBase64 { get; set; } = string.Empty;
        public PrintDataType LabelType { get; set; }
        public int PackageId { get; set; }
        public int ErpShipmentId { get; set; }
        public Courier Courier { get; set; }

        public static ShipmentResponse CreateFailure(string error) => new()
        {
            Success = false,
            ErrorMessage = error
        };

        public static ShipmentResponse CreateSuccess(
            Courier courier,
            int packageId,
            int erpShipmentId,
            string trackingNumber,
            string labelBase64,
            PrintDataType labelType) => new()
            {
                Success = true,
                Courier = courier,
                PackageId = packageId,
                ErpShipmentId = erpShipmentId,
                TrackingNumber = trackingNumber,
                LabelBase64 = labelBase64,
                LabelType = labelType
            };
    }
}