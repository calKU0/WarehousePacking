using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.Enums
{
    public enum PackingLevel
    {
        [Description("Góra")]
        Góra,

        [Description("Dół")]
        Dół
    }
}