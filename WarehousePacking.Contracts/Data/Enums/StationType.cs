using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum StationType
    {
        [Description("Pakowanie")]
        Packing,

        [Description("Wysyłka Paczek")]
        Shipping,

        [Description("Dashboard")]
        Dashboard
    }
}