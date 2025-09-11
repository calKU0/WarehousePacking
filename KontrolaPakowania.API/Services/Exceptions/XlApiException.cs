namespace KontrolaPakowania.API.Services.Exceptions
{
    public class XlApiException : Exception
    {
        public int ErrorCode { get; }

        public XlApiException(int errorCode) : base($"ERP XL API Error. Code: {errorCode}")
        {
            ErrorCode = errorCode;
        }

        public XlApiException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public XlApiException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}