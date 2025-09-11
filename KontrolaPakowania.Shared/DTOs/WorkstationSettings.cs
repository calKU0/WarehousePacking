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
        public PackingLocation PackingLocation { get; set; } = PackingLocation.Góra;
        public string StationNumber { get; set; } = "";
    }
}