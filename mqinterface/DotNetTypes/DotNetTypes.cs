// using System.Reflection;
// using System.Diagnostics;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;


using System.Runtime.InteropServices;

public class Logger {
    public static void Log(string message) {
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

public delegate void CmdFunc(PlayerClient client, string msg);

public class EQCommands {

    public static List<string> Commands = [];
    public static void AddCommand(string name, CmdFunc fn, bool eq = false, bool parse = true, bool inGame = false) {
        Logger.Log($"Adding command: {name}");  
        Commands.Add(name);
        var eqCommandFunc = MQ2Sharp.make_eqcmd_func((clientPtr, messagePtr) => {
            var client = EqFactory.CreatePlayerClient(clientPtr, false);
            string message = Marshal.PtrToStringAnsi(messagePtr);
            fn.Invoke(client, message ?? "");
        });
        MQ2Sharp.AddCommand(name, eqCommandFunc, eq, parse, inGame);
    }

    public static void FlushCommands() {
        foreach (var c in Commands) {
            Logger.Log($"Removing command: {c}");
            MQ2Sharp.RemoveCommand(c);
        }
        Commands.Clear();
    }
}