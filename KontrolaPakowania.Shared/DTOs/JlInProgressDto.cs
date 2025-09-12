using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlInProgressDto
    {
        public string Name { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string StationNumber { get; set; } = string.Empty;
        public string Courier { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}