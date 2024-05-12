namespace Serilog.Sinks.MQConsole.Themes
{
    public abstract class ConsoleTheme
    {
        public static ConsoleTheme None { get; } = new EmptyConsoleTheme();

        public abstract int Set(TextWriter output, ConsoleThemeStyle style);

        public abstract void Reset(TextWriter output);

        protected abstract int ResetCharCount { get; }

        internal StyleReset Apply(TextWriter output, ConsoleThemeStyle style, ref int invisibleCharacterCount)
        {
            invisibleCharacterCount += Set(output, style);
            invisibleCharacterCount += ResetCharCount;

            return new StyleReset(this, output);
        }
    }
}