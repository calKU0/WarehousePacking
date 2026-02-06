using System.Collections.Generic;

namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class Shipper
    {
        public Address? Address { get; set; }
        public Contact? Contact { get; set; }
    }
}