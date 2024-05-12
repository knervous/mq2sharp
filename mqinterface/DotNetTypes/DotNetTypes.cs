// using System.Reflection;
// using System.Diagnostics;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;


using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;


public class EqFactory
{
    public static PlayerClient CreatePlayerClient(nint Cptr, bool own)
    {
        return new PlayerClient(Cptr, own);
    }
}

public abstract class MQEventHandler
{
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    Task? runningTask;
    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        runningTask?.Wait();
    }
    public void LoopMain()
    {
        CancellationToken token = cancellationTokenSource.Token;
        runningTask = Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                Main(_logger);
                Thread.Sleep(10);
            }
        }, token);
    }
    public virtual void Main(ILogger<MQ2Sharp>? logger) { }
    public virtual void CleanUI() { }
    public virtual void ReloadUI() { }
    public virtual void DrawHUD() { }
    public virtual void Pulse() { }
    public virtual void BeginZone() { }
    public virtual void EndZone() { }
    public virtual void Zoned() { }
    public virtual void UpdateImGui() { }

    public virtual void SetGameState(int eventType) { }

    public virtual void WriteChatColor(string msg, int color, int filter) {}
    public virtual bool IncomingChat(string msg, ulong color) {  return false;  }
    public virtual void AddSpawn(PlayerClient spawn) { }
    public virtual void RemoveSpawn(PlayerClient spawn) { }
    public virtual void AddGroundItem() { }
    public virtual void RemoveGroundItem() { }
    public virtual void MacroStart(string macro) { }
    public virtual void MacroStop(string macro) { }
    public virtual void LoadPlugin(string plugin) { }
    public virtual void UnloadPlugin(string plugin) { }

    protected ILogger<MQ2Sharp>? _logger;
}

public delegate void CmdFunc(PlayerClient client, string msg);

public class EQCommands
{

    public static List<string> Commands = [];
    public static void AddCommand(string name, CmdFunc fn, bool eq = false, bool parse = true, bool inGame = false, ILogger? logger = null)
    {
        logger?.LogInformation("Adding command: {NAME}", name);
        Commands.Add(name);
        var eqCommandFunc = MQ2Sharp.make_eqcmd_func((clientPtr, messagePtr) =>
        {
            var client = EqFactory.CreatePlayerClient(clientPtr, false);
            string message = Marshal.PtrToStringAnsi(messagePtr) ?? "Unable to convert pointer to string.";
            fn.Invoke(client, message ?? "");
        });
        MQ2Sharp.AddCommand(name, eqCommandFunc, eq, parse, inGame);
    }

    public static void FlushCommands(ILogger? logger = null)
    {
        foreach (var c in Commands)
        {
            logger?.LogInformation("Removing command: {COMMAND}", c);
            MQ2Sharp.RemoveCommand(c);
        }
        Commands.Clear();
    }
}