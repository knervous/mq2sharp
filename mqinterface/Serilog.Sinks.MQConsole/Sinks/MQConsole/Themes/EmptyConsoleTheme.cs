namespace Serilog.Sinks.MQConsole.Themes
{
    class EmptyConsoleTheme : ConsoleTheme
    {
        protected override int ResetCharCount { get; }

        public override int Set(TextWriter output, ConsoleThemeStyle style) => 0;

        public override void Reset(TextWriter output)
        {
        }
    }
}