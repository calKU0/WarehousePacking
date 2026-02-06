using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.Enums
{
    public enum PackageType
    {
        [Description("Paczka")]
        PC,
        [Description("Paleta")]
        PL,
        [Description("Koperta")]
        KP
    }
}
