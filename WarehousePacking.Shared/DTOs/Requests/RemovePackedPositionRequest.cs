using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class RemovePackedPositionRequest
    {
        public int PackingDocumentId { get; set; }
        public int SourceDocumentId { get; set; }
        public int SourceDocumentType { get; set; }
        public int PositionNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
    }
}