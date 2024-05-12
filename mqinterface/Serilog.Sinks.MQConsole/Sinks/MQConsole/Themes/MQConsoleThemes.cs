namespace Serilog.Sinks.MQConsole.Themes
{
    internal class MQConsoleThemes
    {
        public static MQConsoleTheme Colored { get; } = new MQConsoleTheme(
            new Dictionary<ConsoleThemeStyle, string>
            {
                [ConsoleThemeStyle.Text] = "\a-w",
                [ConsoleThemeStyle.SecondaryText] = "\ao",
                [ConsoleThemeStyle.TertiaryText] = "\ag",
                [ConsoleThemeStyle.Invalid] = "\a-o",
                [ConsoleThemeStyle.Null] = "\a-u",
                [ConsoleThemeStyle.Name] = "\am",
                [ConsoleThemeStyle.String] = "\at",
                [ConsoleThemeStyle.Number] = "\at",
                [ConsoleThemeStyle.Boolean] = "\a-m",
                [ConsoleThemeStyle.Scalar] = "\a-p",
                [ConsoleThemeStyle.LevelVerbose] = "\a-w",
                [ConsoleThemeStyle.LevelDebug] = "\aw",
                [ConsoleThemeStyle.LevelInformation] = "\au",
                [ConsoleThemeStyle.LevelWarning] = "\ay",
                [ConsoleThemeStyle.LevelError] = "\ar",
                [ConsoleThemeStyle.LevelFatal] = "\a-r",
            });

        public static MQConsoleTheme Grayscale { get; } = new MQConsoleTheme(
            new Dictionary<ConsoleThemeStyle, string>
            {
                [ConsoleThemeStyle.Text] = "\aw",
                [ConsoleThemeStyle.SecondaryText] = "\aw",
                [ConsoleThemeStyle.TertiaryText] = "\aw",
                [ConsoleThemeStyle.Invalid] = "\aw",
                [ConsoleThemeStyle.Null] = "\aw",
                [ConsoleThemeStyle.Name] = "\aw",
                [ConsoleThemeStyle.String] = "\aw",
                [ConsoleThemeStyle.Number] = "\aw",
                [ConsoleThemeStyle.Boolean] = "\aw",
                [ConsoleThemeStyle.Scalar] = "\aw",
                [ConsoleThemeStyle.LevelVerbose] = "\aw",
                [ConsoleThemeStyle.LevelDebug] = "\aw",
                [ConsoleThemeStyle.LevelInformation] = "\aw",
                [ConsoleThemeStyle.LevelWarning] = "\aw",
                [ConsoleThemeStyle.LevelError] = "\aw",
                [ConsoleThemeStyle.LevelFatal] = "\aw",
            });
    }
}
