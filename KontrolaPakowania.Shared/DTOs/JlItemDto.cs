using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using System.Text.Json.Serialization;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlItemDto
    {
        public string JlCode { get; set; } = string.Empty;
        public int ItemErpId { get; set; }
        public int ItemWmsId { get; set; }
        public int ErpPositionNumber { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public List<string> ItemEan { get; set; } = new List<string>();
        public string ItemUnit { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string ItemImage { get; set; } = string.Empty;
        public decimal ItemWeight { get; set; }
        public decimal ItemVolume { get; set; }
        public bool PackedWMS { get; set; }
        public List<string> SupplierCode { get; set; } = new List<string>();
        public string ClientErpId { get; set; } = string.Empty;
        public string AddressName { get; set; } = string.Empty;
        public string AddressCity { get; set; } = string.Empty;
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressPostalCode { get; set; } = string.Empty;
        public string AddressCountry { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;
        public string ClientSymbol { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public string PackingRequirements { get; set; } = string.Empty;
        public DateTime ScanDate { get; set; }

        [JsonPropertyName("courier")]
        public string CourierName { get; set; } = string.Empty;

        private Courier courier;

        [JsonPropertyName("courierenum")]
        public Courier Courier
        {
            get => courier;
            set
            {
                if (courier != value)
                {
                    courier = value;
                    InitCourierLogo();
                }
            }
        }

        public string LogoCourier { get; set; } = string.Empty;

        public ShipmentServices ShipmentServices { get; set; } = new();

        public int BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string TermValidity { get; set; } = string.Empty;

        [JsonPropertyName("erpDocumentId")]
        public string ErpDocumentId { get; set; } = string.Empty;

        private int _documentId;
        private int _documentType;

        [JsonIgnore]
        public int DocumentId
        {
            get => _documentId != 0 ? _documentId : ParseErpDocumentId().documentId;
            set => _documentId = value;
        }

        [JsonIgnore]
        public int DocumentType
        {
            get => _documentType != 0 ? _documentType : ParseErpDocumentId().documentType;
            set => _documentType = value;
        }

        public decimal DocumentQuantity { get; set; }
        public decimal JlQuantity { get; set; }

        private (int documentId, int documentType) ParseErpDocumentId()
        {
            if (string.IsNullOrWhiteSpace(ErpDocumentId))
                return (0, 0);

            var parts = ErpDocumentId.Split('|', 2);
            var documentId = parts.Length > 0 ? Convert.ToInt32(parts[0]) : 0;
            var documentType = parts.Length > 1 ? Convert.ToInt32(parts[1]) : 0;
            return (documentId, documentType);
        }

        private void InitCourierLogo()
        {
            var suffixes = new List<string>();

            foreach (var prop in typeof(ShipmentServices).GetProperties())
            {
                if (prop.PropertyType == typeof(bool) && (bool)prop.GetValue(ShipmentServices))
                {
                    suffixes.Add(prop.Name); // Or map to user-friendly names
                }
            }

            LogoCourier = suffixes.Any()
                ? $"{Courier.GetDescription()}-{string.Join(", ", suffixes)}"
                : Courier.GetDescription();
        }
    }
}