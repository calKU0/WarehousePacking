using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class PackWMSResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
    }
}