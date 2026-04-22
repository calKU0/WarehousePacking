using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum PickingType
    {
        [Description("Picking")]
        Picking,
        [Description("Multipicking One")]
        Multipicking_One,
        [Description("Multipicking")]
        Multipicking
    }
}
