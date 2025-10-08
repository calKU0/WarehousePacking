using System.Collections.Generic;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class TransactionShipment
    {
        public string? MasterTrackingNumber { get; set; }
        public string? ServiceType { get; set; }
        public string? ShipDatestamp { get; set; }
        public string? ServiceName { get; set; }

        public List<PieceResponse>? PieceResponses { get; set; }
        public ShipmentAdvisoryDetails? ShipmentAdvisoryDetails { get; set; }
        public CompletedShipmentDetail? CompletedShipmentDetail { get; set; }
        public string? ServiceCategory { get; set; }
    }

    public class PieceResponse
    {
        public string? MasterTrackingNumber { get; set; }
        public string? TrackingNumber { get; set; }
        public double? AdditionalChargesDiscount { get; set; }
        public double? NetRateAmount { get; set; }
        public double? NetChargeAmount { get; set; }
        public double? NetDiscountAmount { get; set; }
        public List<PackageDocument>? PackageDocuments { get; set; }  // This was trimmed from your JSON
        public string? Currency { get; set; }
        public List<object>? CustomerReferences { get; set; }
        public double? CodcollectionAmount { get; set; }
        public double? BaseRateAmount { get; set; }
    }

    public class PackageDocument
    {
        public string? ContentType { get; set; }
        public int? CopiesToPrint { get; set; }
        public string? EncodedLabel { get; set; }
        public string? DocType { get; set; }
    }

    public class ShipmentAdvisoryDetails
    {
    }

    public class CompletedShipmentDetail
    {
        public bool? UsDomestic { get; set; }
        public string? CarrierCode { get; set; }
        public MasterTrackingId? MasterTrackingId { get; set; }
        public ServiceDescription? ServiceDescription { get; set; }
        public string? PackagingDescription { get; set; }
        public OperationalDetail? OperationalDetail { get; set; }
        public ShipmentRating? ShipmentRating { get; set; }
        public List<CompletedPackageDetail>? CompletedPackageDetails { get; set; }
        public DocumentRequirements? DocumentRequirements { get; set; }
    }

    public class MasterTrackingId
    {
        public string? TrackingIdType { get; set; }
        public string? FormId { get; set; }
        public string? TrackingNumber { get; set; }
    }

    public class ServiceDescription
    {
        public string? ServiceId { get; set; }
        public string? ServiceType { get; set; }
        public string? Code { get; set; }
        public List<ServiceName>? Names { get; set; }
        public List<string>? OperatingOrgCodes { get; set; }
        public string? ServiceCategory { get; set; }
        public string? Description { get; set; }
        public string? AstraDescription { get; set; }
    }

    public class ServiceName
    {
        public string? Type { get; set; }
        public string? Encoding { get; set; }
        public string? Value { get; set; }
    }

    public class OperationalDetail
    {
        public string? UrsaPrefixCode { get; set; }
        public string? UrsaSuffixCode { get; set; }
        public string? OriginLocationId { get; set; }
        public int? OriginLocationNumber { get; set; }
        public string? OriginServiceArea { get; set; }
        public string? DestinationLocationId { get; set; }
        public int? DestinationLocationNumber { get; set; }
        public string? DestinationServiceArea { get; set; }
        public string? DestinationLocationStateOrProvinceCode { get; set; }
        public string? DeliveryDate { get; set; }
        public string? DeliveryDay { get; set; }
        public string? CommitDate { get; set; }
        public string? CommitDay { get; set; }
        public bool? IneligibleForMoneyBackGuarantee { get; set; }
        public string? AstraPlannedServiceLevel { get; set; }
        public string? AstraDescription { get; set; }
        public string? PostalCode { get; set; }
        public string? StateOrProvinceCode { get; set; }
        public string? CountryCode { get; set; }
        public string? AirportId { get; set; }
        public string? ServiceCode { get; set; }
        public string? PackagingCode { get; set; }
        public string? PublishedDeliveryTime { get; set; }
        public string? Scac { get; set; }
    }

    public class ShipmentRating
    {
        public string? ActualRateType { get; set; }
        public List<ShipmentRateDetail>? ShipmentRateDetails { get; set; }
    }

    public class ShipmentRateDetail
    {
        public string? RateType { get; set; }
        public string? RateScale { get; set; }
        public string? RateZone { get; set; }
        public string? PricingCode { get; set; }
        public string? RatedWeightMethod { get; set; }
        public CurrencyExchangeRate? CurrencyExchangeRate { get; set; }
        public int? DimDivisor { get; set; }
        public double? FuelSurchargePercent { get; set; }
        public TotalBillingWeight? TotalBillingWeight { get; set; }
        public double TotalBaseCharge { get; set; }
        public double TotalFreightDiscounts { get; set; }
        public double TotalNetFreight { get; set; }
        public double TotalSurcharges { get; set; }
        public double TotalNetFedExCharge { get; set; }
        public double TotalTaxes { get; set; }
        public double TotalNetCharge { get; set; }
        public double TotalRebates { get; set; }
        public double TotalDutiesAndTaxes { get; set; }
        public double TotalAncillaryFeesAndTaxes { get; set; }
        public double TotalDutiesTaxesAndFees { get; set; }
        public double TotalNetChargeWithDutiesAndTaxes { get; set; }
        public List<Surcharge>? Surcharges { get; set; }
        public List<object>? FreightDiscounts { get; set; }
        public List<Tax>? Taxes { get; set; }
        public string? Currency { get; set; }
    }

    public class CurrencyExchangeRate
    {
        public string? FromCurrency { get; set; }
        public string? IntoCurrency { get; set; }
        public double Rate { get; set; }
    }

    public class TotalBillingWeight
    {
        public string? Units { get; set; }
        public double Value { get; set; }
    }

    public class Surcharge
    {
        public string? SurchargeType { get; set; }
        public string? Level { get; set; }
        public string? Description { get; set; }
        public double Amount { get; set; }
    }

    public class Tax
    {
        public string? Type { get; set; }
        public string? Description { get; set; }
        public double Amount { get; set; }
    }

    public class CompletedPackageDetail
    {
        public int SequenceNumber { get; set; }
        public List<TrackingId>? TrackingIds { get; set; }
        public int GroupNumber { get; set; }
        public string? SignatureOption { get; set; }
        public OperationalDetail? OperationalDetail { get; set; }
    }

    public class TrackingId
    {
        public string? TrackingIdType { get; set; }
        public string? FormId { get; set; }
        public string? TrackingNumber { get; set; }
    }

    public class DocumentRequirements
    {
        public List<string>? RequiredDocuments { get; set; }
        public List<GenerationDetail>? GenerationDetails { get; set; }
        public List<string>? ProhibitedDocuments { get; set; }
    }

    public class GenerationDetail
    {
        public string? Type { get; set; }
        public int MinimumCopiesRequired { get; set; }
    }
}