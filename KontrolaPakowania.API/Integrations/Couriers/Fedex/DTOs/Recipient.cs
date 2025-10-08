using System.Collections.Generic;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class Recipient
    {
        public Address? Address { get; set; }
        public Contact? Contact { get; set; }
        public string? DeliveryInstructions { get; set; }
        public string? EmailAddress { get; set; }
    }
}