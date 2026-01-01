using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class WmsPackStockRequest
    {
        public PackingLevel PackingLevel { get; set; }
        public PackingWarehouse PackingWarehouse { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string PackageCode { get; set; } = string.Empty;
        public string Courier { get; set; } = string.Empty;
        public string JlCode { get; set; } = string.Empty;
        public string StationNumber { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public DocumentStatus Status { get; set; }
        public List<WMSPackStockItemsRequest> Items { get; set; } = new();
    }

    public class WMSPackStockItemsRequest
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}