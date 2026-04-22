using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class CourierProtocolResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Courier Courier { get; set; }
        public PrintDataType DataType { get; set; }
        public List<string> DataBase64 { get; set; } = new List<string>();
    }
}