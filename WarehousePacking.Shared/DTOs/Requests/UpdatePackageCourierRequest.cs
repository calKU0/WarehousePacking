using WarehousePacking.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class UpdatePackageCourierRequest
    {
        public int PackageId { get; set; }
        public int? DocumentId { get; set; }
        public Courier Courier { get; set; }
    }
}