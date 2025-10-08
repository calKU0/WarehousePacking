using System.Collections.Generic;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class Shipper
    {
        public Address? Address { get; set; }
        public Contact? Contact { get; set; }
    }
}