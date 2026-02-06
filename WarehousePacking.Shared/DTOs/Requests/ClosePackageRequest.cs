using WarehousePacking.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class ClosePackageRequest
    {
        public int DocumentId { get; set; }
        public string InternalBarcode { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
    }
}