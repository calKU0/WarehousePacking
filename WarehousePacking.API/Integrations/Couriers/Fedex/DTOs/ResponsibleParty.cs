using System.Collections.Generic;

namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class ResponsibleParty
    {
        public Address? Address { get; set; }
        public Contact? Contact { get; set; }
        public AccountNumber? AccountNumber { get; set; }
    }
}