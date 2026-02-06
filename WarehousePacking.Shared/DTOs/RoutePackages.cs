using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs
{
    public class RoutePackages
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool Dropshipping { get; set; }
    }
}