using System.Collections.Generic;

namespace WarehousePacking.PrintService.Models
{
    public class PrintJob
    {
        public string PrinterName { get; set; }
        public string DataType { get; set; } // "ZPL", "EPL", "PDF", "CRYSTAL"
        public string Content { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}