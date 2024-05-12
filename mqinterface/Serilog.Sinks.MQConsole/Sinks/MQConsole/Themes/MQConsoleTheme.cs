namespace Serilog.Sinks.MQConsole.Themes
{
    public class MQConsoleTheme : ConsoleTheme
    {
        readonly IReadOnlyDictionary<ConsoleThemeStyle, string> _styles;
        const string MQStyleReset = "\ax";


        /// <summary>
        /// A 256-color theme along the lines of Visual Studio Code.
        /// </summary>
        public static MQConsoleTheme Colored { get; } = MQConsoleThemes.Colored;

        /// <summary>
        /// A theme using only gray, black and white.
        /// </summary>
        public static MQConsoleTheme Grayscale { get; } = MQConsoleThemes.Grayscale;

        public MQConsoleTheme(IReadOnlyDictionary<ConsoleThemeStyle, string> styles)
        {
            if (styles is null) throw new ArgumentNullException(nameof(styles));
            _styles = styles.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        protected override int ResetCharCount { get; }

        public override void Reset(TextWriter output)
        {
            output.Write(MQStyleReset);
        }

        public override int Set(TextWriter output, ConsoleThemeStyle style)
        {
            if (_styles.TryGetValue(style, out var mqStyle))
            {
                output.Write(mqStyle);
                return mqStyle.Length;
            }
            return 0;
        }
    }
}
