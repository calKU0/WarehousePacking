using WarehousePacking.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class CourierProtocolResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Courier Courier { get; set; }
        public PrintDataType DataType { get; set; }
        public List<string> DataBase64 { get; set; } = new List<string>();
    }
}
