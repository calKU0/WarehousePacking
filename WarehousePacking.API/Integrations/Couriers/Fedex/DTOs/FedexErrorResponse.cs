namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class FedexErrorResponse
    {
        public string? TransactionId { get; set; }
        public string? CustomerTransactionId { get; set; }
        public List<FedexError>? Errors { get; set; }
    }

    public class FedexError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public List<FedexErrorParameter> ParameterList { get; set; }
    }

    public class FedexErrorParameter
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}