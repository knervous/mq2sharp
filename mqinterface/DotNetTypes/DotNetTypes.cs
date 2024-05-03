// using System.Reflection;
// using System.Diagnostics;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;


using System.Runtime.InteropServices;

public class Logger
{
    public static void Log(string message)
    {
        MQ2Sharp.MQLog(message, MQ2Sharp.CONCOLOR_YELLOW, 0);
    }
}

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
                Main();
                Thread.Sleep(10);
            }
        }, token);
    }
    public virtual void Main() { }
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
}

public delegate void CmdFunc(PlayerClient client, string msg);

public class EQCommands
{

    public static List<string> Commands = [];
    public static void AddCommand(string name, CmdFunc fn, bool eq = false, bool parse = true, bool inGame = false)
    {
        Logger.Log($"Adding command: {name}");
        Commands.Add(name);
        var eqCommandFunc = MQ2Sharp.make_eqcmd_func((clientPtr, messagePtr) =>
        {
            var client = EqFactory.CreatePlayerClient(clientPtr, false);
            string message = Marshal.PtrToStringAnsi(messagePtr);
            fn.Invoke(client, message ?? "");
        });
        MQ2Sharp.AddCommand(name, eqCommandFunc, eq, parse, inGame);
    }

    public static void FlushCommands()
    {
        foreach (var c in Commands)
        {
            Logger.Log($"Removing command: {c}");
            MQ2Sharp.RemoveCommand(c);
        }
        Commands.Clear();
    }
}