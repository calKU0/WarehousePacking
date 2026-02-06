namespace WarehousePacking.Server.Settings
{
    public class AppSettings
    {
        public IntervalsSettings Intervals { get; set; }
    }

    public class IntervalsSettings
    {
        public int RefreshPackingList { get; set; }
        public int RefreshRoutesStatus { get; set; }
        public int Logout { get; set; }
    }
}
