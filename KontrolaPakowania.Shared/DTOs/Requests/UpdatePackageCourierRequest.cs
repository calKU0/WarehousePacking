using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class UpdatePackageCourierRequest
    {
        public int PackageId { get; set; }
        public Courier Courier { get; set; }
    }
}