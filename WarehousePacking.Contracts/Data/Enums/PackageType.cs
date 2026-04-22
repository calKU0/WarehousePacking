using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
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