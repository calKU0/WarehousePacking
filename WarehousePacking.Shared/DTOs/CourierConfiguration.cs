using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs
{
    public class CourierConfiguration
    {
        public decimal MaxPackageWeight { get; set; }
        public TimeSpan CloseRouteTime { get; set; }
    }
}