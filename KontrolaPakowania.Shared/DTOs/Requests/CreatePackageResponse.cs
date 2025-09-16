using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class CreatePackageResponse
    {
        public int DocumentRef { get; set; }
        public int DocumentId { get; set; }
    }
}