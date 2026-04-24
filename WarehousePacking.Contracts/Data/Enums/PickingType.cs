using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum PickingType
    {
        [Description("Picking")]
        Picking = 1,
        [Description("Multipicking")]
        Multipicking = 2,
        [Description("Multipicking One")]
        Multipicking_One = 3
    }
}
