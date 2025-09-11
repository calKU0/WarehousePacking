using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.Settings
{
    public class ApiSettings
    {
        public ApiClientSettings Database { get; set; } = new();
        public ApiClientSettings FedEx { get; set; } = new();
        public ApiClientSettings GLS { get; set; } = new();
    }

    public class ApiClientSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}