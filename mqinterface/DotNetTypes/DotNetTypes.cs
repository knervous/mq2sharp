// using System.Reflection;
// using System.Diagnostics;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;


public class Logger {
    public static void Log(string message) {
        MQ2Sharp.WriteChatColor(message, (int)ConsoleColor.Yellow, 0);
    }
}

public class EqFactory
{
    public static PlayerClient CreatePlayerClient(nint Cptr, bool own)
    {
        return new PlayerClient(Cptr, own);
    }
}

// public delegate void CmdFunc(Client client, string msg);

// public class EQCommands {

//     public static List<string> Commands = [];
//     public static int AddCommand(string name, string description, AccountStatus admin_level, CmdFunc fn) {
//         questinterface.LogSys.QuestDebug($"Adding command: {name}");
//         Commands.Add(name);
//         return questinterface.command_put(name, description, (byte)admin_level, (clientPtr, seperatorPtr) => {
//             var sep = EqFactory.CreateSeperator(seperatorPtr, false);
//             fn.Invoke(EqFactory.CreateClient(clientPtr, false), sep.msg);
//         });
//     }

//     public static void FlushCommands() {
//         foreach (var c in Commands) {
//             questinterface.LogSys.QuestDebug($"Removing command: {c}");
//             questinterface.command_delete(c);
//         }
//         Commands.Clear();
//     }
// }