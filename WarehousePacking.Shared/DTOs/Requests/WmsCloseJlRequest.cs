using WarehousePacking.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class WmsCloseJlRequest
    {
        public string PackageNumber { get; set; } = string.Empty;
        public string? PackageDestination { get; set; }
        public Courier Courier { get; set; }
        public PackingLevel PackingLevel { get; set; }
        public PackingWarehouse PackingWarehouse { get; set; }
    }
}