using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.MQConsole.Themes;
using System.Runtime.InteropServices;
using System.Text;

namespace Serilog.Sinks
{
    class MQConsoleSink : ILogEventSink
    {
        readonly LogEventLevel? _standardErrorFromLevel;
        readonly ConsoleTheme _theme;
        readonly ITextFormatter _formatter;
        readonly object _syncRoot;

        const int DefaultWriteBufferCapacity = 256;

        static MQConsoleSink()
        {
        }

        public MQConsoleSink(
            ConsoleTheme theme,
            ITextFormatter formatter,
            LogEventLevel? standardErrorFromLevel,
            object syncRoot)
        {
            _standardErrorFromLevel = standardErrorFromLevel;
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _formatter = formatter;
            _syncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
        }

        public void Emit(LogEvent logEvent)
        {
            var buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));
            _formatter.Format(logEvent, buffer);
            var formattedLogEventText = buffer.ToString();
            lock (_syncRoot)
            {
                MQ2WriteChatf(formattedLogEventText);
            }
        }

        [DllImport("MQ2Main.dll", EntryPoint = "WriteChatf", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MQ2WriteChatf([MarshalAs(UnmanagedType.LPStr)] string buffer);
    }
}
