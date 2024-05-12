namespace Serilog.Sinks.MQConsole.Formatting
{
    struct ThemedValueFormatterState
    {
        public TextWriter Output;
        public string? Format;
        public bool IsTopLevel;

        public ThemedValueFormatterState Nest() => new ThemedValueFormatterState { Output = Output };
    }
}
