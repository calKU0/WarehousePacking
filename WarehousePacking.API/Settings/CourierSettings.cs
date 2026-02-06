namespace WarehousePacking.API.Settings
{
    public class CourierSettings
    {
        public DpdSettings DPD { get; set; } = new DpdSettings();
        public DpdRomaniaSettings DPDRomania { get; set; } = new DpdRomaniaSettings();
        public GlsSettings GLS { get; set; } = new GlsSettings();
        public FedexSettings Fedex { get; set; } = new FedexSettings();
        public SenderSettings Sender { get; set; } = new SenderSettings();
    }
}