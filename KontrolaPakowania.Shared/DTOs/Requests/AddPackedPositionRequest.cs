using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class AddPackedPositionRequest
    {
        public int DocumentRef { get; set; }
        public int DocumentId { get; set; }
        public int DocumentType { get; set; }
        public int PositionNumber { get; set; }
        public string Quantity { get; set; } = "0";
    }
}