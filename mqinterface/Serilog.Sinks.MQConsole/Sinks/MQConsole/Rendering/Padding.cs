using Serilog.Parsing;

namespace Serilog.Sinks.MQConsole.Rendering
{
    static class Padding
    {
        static readonly char[] PaddingChars = new string(' ', 80).ToCharArray();

        public static void Apply(TextWriter output, string value, Alignment? alignment)
        {
            if (alignment is null || value.Length >= alignment.Value.Width)
            {
                output.Write(value);
                return;
            }

            var pad = alignment.Value.Width - value.Length;

            if (alignment.Value.Direction == AlignmentDirection.Left)
                output.Write(value);

            if (pad <= PaddingChars.Length)
            {
                output.Write(PaddingChars, 0, pad);
            }
            else
            {
                output.Write(new string(' ', pad));
            }

            if (alignment.Value.Direction == AlignmentDirection.Right)
                output.Write(value);
        }
    }
}