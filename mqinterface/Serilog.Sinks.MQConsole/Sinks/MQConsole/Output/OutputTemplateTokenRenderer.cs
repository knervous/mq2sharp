using Serilog.Events;

namespace Serilog.Sinks.MQConsole.Output
{
    abstract class OutputTemplateTokenRenderer
    {
        public abstract void Render(LogEvent logEvent, TextWriter output);
    }
}
