using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KontrolaPakowania.PrintAgent
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(PrintJob))]
    internal partial class PrintAgentJsonContext : JsonSerializerContext
    {
    }
}
