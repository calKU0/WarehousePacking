namespace KontrolaPakowania.API.Settings
{
    public class CourierSettings
    {
        public DpdSettings DPD { get; set; } = new DpdSettings();
        public GlsSettings GLS { get; set; } = new GlsSettings();
        public FedexSettings Fedex { get; set; } = new FedexSettings();
    }
}