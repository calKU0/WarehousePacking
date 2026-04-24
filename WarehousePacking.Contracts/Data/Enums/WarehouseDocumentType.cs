using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum WarehouseDocumentType
    {
        [Description("Przyjęcie")]
        Receipt = 0,

        [Description("Zlecenie wydania")]
        IssueOrder = 1,

        [Description("Przesunięcie")]
        Transfer = 2,

        [Description("Remanent")]
        Inventory = 3,

        [Description("Remanent+")]
        InventoryPlus = 4,

        [Description("Remanent-")]
        InventoryMinus = 5,

        [Description("Wydanie zewnętrzne")]
        ExternalIssue = 6,

        [Description("Przyjęcie zewnętrzne")]
        ExternalReceipt = 7,

        [Description("Przeniesienie")]
        Relocation = 8,

        [Description("Pobranie surowca")]
        RawMaterialPick = 9,

        [Description("Zadanie")]
        Task = 14,

        [Description("Przyjęcie zewn.")]
        ExternalReceiptAlt = 15,

        [Description("Przesunięcie Ad-Hoc")]
        AdHocTransfer = 16,

        [Description("Sortowanie")]
        Sorting = 18,

        [Description("Pakowanie")]
        Packing = 19,

        [Description("Załadunek")]
        Loading = 20,

        [Description("Zgłoszenie braku")]
        ShortageReport = 22,

        [Description("Odnalezienie braku")]
        ShortageFound = 23,

        [Description("Potwierdzenie braku")]
        ShortageConfirmed = 24
    }
}
