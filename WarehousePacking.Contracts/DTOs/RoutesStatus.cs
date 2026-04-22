namespace WarehousePacking.Contracts.DTOs
{
    public class RoutesStatus
    {
        public bool DPDClosed { get; set; }
        public bool GLSClosed { get; set; }
        public bool FedexClosed { get; set; }
    }
}