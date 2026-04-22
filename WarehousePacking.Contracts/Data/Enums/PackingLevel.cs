using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum PackingLevel
    {
        [Description("Góra")]
        Up,

        [Description("Dół")]
        Bottom
    }
}