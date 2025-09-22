using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class WorkstationSettings
    {
        public string PrinterLabel { get; set; } = "";
        public string PrinterInvoice { get; set; } = "";
        public PackingWarehouse PackingWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }
        public string StationNumber { get; set; } = "";
    }
}