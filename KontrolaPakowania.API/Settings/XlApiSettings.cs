namespace KontrolaPakowania.API.Settings
{
    public class XlApiSettings
    {
        public int ApiVersion { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}