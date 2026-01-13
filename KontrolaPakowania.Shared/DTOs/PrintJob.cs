using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class PrintJob
    {
        public string PrinterName { get; set; }
        public string DataType { get; set; } // "ZPL", "EPL", "PDF", "CRYSTAL"
        public string Content { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
