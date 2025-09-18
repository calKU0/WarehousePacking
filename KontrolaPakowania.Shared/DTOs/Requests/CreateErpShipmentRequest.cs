using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class CreateErpShipmentRequest
    {
        public int PackageId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string TrackingLink { get; set; } = string.Empty;
        public float CODAmout { get; set; }
        public float Insurance { get; set; }
        public int PackageCount { get; set; }
    }
}