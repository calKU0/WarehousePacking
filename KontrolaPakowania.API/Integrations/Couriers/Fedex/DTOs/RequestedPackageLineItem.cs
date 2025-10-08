using System.Collections.Generic;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class RequestedPackageLineItem
    {
        public string? SequenceNumber { get; set; }
        public string? SubPackagingType { get; set; }
        public List<CustomerReference>? CustomerReferences { get; set; }
        public DeclaredValue? DeclaredValue { get; set; }
        public Weight? Weight { get; set; }
        public Dimensions? Dimensions { get; set; }
        public int? GroupPackageCount { get; set; }
        public string? ItemDescriptionForClearance { get; set; }
        public string? ItemDescription { get; set; }
    }
}