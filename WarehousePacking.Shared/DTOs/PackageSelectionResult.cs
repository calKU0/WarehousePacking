using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs
{
    public class PackageSelectionResult
    {
        public int? PackageId { get; set; }
        public string? InternalBarcode { get; set; }
    }
}