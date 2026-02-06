namespace WarehousePacking.API.Integrations.Couriers.DPD.DTOs
{
    public class DpdCreatePackageRequest
    {
        public string GenerationPolicy { get; set; } = string.Empty;
        public List<Package> Packages { get; set; } = new List<Package>();

        public class Attribute
        {
            public string Code { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public class Package
        {
            public string? Reference { get; set; }
            public Receiver Receiver { get; set; } = new Receiver();
            public PudoReceiver? PudoReceiver { get; set; }
            public Sender Sender { get; set; } = new Sender();
            public int PayerFID { get; set; }
            public string? Ref1 { get; set; }
            public string? Ref2 { get; set; }
            public string? Ref3 { get; set; }
            public List<Service>? Services { get; set; }
            public List<Parcel> Parcels { get; set; } = new List<Parcel>();
        }

        public class Parcel
        {
            public string? Reference { get; set; }
            public decimal Weight { get; set; }
            public decimal WeightAdr { get; set; }
            public decimal SizeX { get; set; }
            public decimal SizeY { get; set; }
            public decimal SizeZ { get; set; }
            public string? Content { get; set; }
            public string? CustomerData1 { get; set; }
            public string? CustomerData2 { get; set; }
            public string? CustomerData3 { get; set; }
        }

        public class PudoReceiver
        {
            public string? Company { get; set; }
            public string? Name { get; set; }
            public string? CountryCode { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
        }

        public class Receiver
        {
            public string Company { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string CountryCode { get; set; } = string.Empty;
            public string PostalCode { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public string? Email { get; set; }
        }

        public class Sender
        {
            public string Company { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string CountryCode { get; set; } = string.Empty;
            public string PostalCode { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class Service
        {
            public string Code { get; set; } = string.Empty;
            public List<Attribute>? Attributes { get; set; }
        }
    }
}