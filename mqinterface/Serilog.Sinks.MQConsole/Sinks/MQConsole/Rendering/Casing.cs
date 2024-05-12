namespace Serilog.Sinks.MQConsole.Rendering
{
    static class Casing
    {
        public static string Format(string value, string? format = null)
        {
            switch (format)
            {
                case "u":
                    return value.ToUpperInvariant();
                case "w":
                    return value.ToLowerInvariant();
                default:
                    return value;
            }
        }
    }
}