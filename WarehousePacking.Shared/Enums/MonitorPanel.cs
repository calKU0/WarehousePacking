using System.ComponentModel;

namespace WarehousePacking.Shared.Enums
{
    public enum MonitorPanel
    {
        [Description("Picking")]
        Picking = 0,
        [Description("Sortowanie")]
        Sorting = 1,
        [Description("Pakowanie")]
        Packing = 2,
        [Description("Przyjęcia")]
        Receiving = 3,
        [Description("Zatowarowanie")]
        Stocking = 4,
    }
}
