namespace KontrolaPakowania.Server.Shared
{
    public class ComboBoxDefinition
    {
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public List<string> Items { get; set; } = new();
        public string SelectedValue { get; set; }
    }
}