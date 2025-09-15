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
        public Courier Courier { get; set; }
        public int PackageId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public byte[] LabelBytes { get; set; } = Array.Empty<byte>();

        public PrintDataType LabelType { get; set; } = PrintDataType.PDF;
    }
}