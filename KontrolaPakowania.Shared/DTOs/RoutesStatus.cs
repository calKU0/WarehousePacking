using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class RoutesStatus
    {
        public bool DPDClosed { get; set; }
        public bool GLSClosed { get; set; }
        public bool FedexClosed { get; set; }
    }
}