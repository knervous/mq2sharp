using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
public class LockfileMutex : IDisposable
{
    private readonly string _fileName;

    private FileStream? _stream;

    public LockfileMutex(string name)
    {
        var assemblyDir = Path.GetDirectoryName(typeof(LockfileMutex).Assembly.Location) ?? throw new FileNotFoundException("cannot determine assembly location");
        var file = Path.GetFullPath(Path.Combine(assemblyDir, name));
        _fileName = file;
    }

    public bool Acquire()
    {
        try
        {
            _stream = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch (IOException ex) when (ex.Message.Contains(_fileName))
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_stream != null)
        {
            _stream.Dispose();

            try
            {
                File.Delete(_fileName);
            }
            catch
            {
                // ignored
            }
        }

        GC.SuppressFinalize(this);
    }
}

public static class MQInterface
{

    [StructLayout(LayoutKind.Sequential)]
    public struct MQEvent
    {
        public EventSubtype Subtype;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct GameStateEvent
    {
        public EventSubtype Subtype;
        public int EventType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ChatColorEvent
    {
        public EventSubtype Subtype;
        public IntPtr Line;  // Use IntPtr for pointers to strings
        public int Color;
        public int Filter;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IncomingChatEvent
    {
        public EventSubtype Subtype;
        public IntPtr Line;
        public ulong Color;  // Use ulong for `unsigned long`
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpawnEvent
    {
        public EventSubtype Subtype;
        public IntPtr Spawn;  // Pointer to eqlib::PlayerClient
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GroundItemEvent
    {
        public EventSubtype Subtype;
        public IntPtr Item;  // Pointer to eqlib::EQGroundItem
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StringEvent
    {
        public EventSubtype Subtype;
        public IntPtr Msg;
    }

    public delegate void InitializeDelegate();
    public delegate void MQEventDelegate(MQEvent args);
    public delegate void GameStateEventDelegate(GameStateEvent args);
    public delegate void ChatColorEventDelegate(ChatColorEvent args);
    public delegate bool IncomingChatEventDelegate(IncomingChatEvent args);
    public delegate void SpawnEventDelegate(SpawnEvent args);
    public delegate void GroundItemEventDelegate(GroundItemEvent args);
    public delegate void StringEventDelegate(StringEvent args);
    public static void MQEventMethod(MQEvent args)
    {
        if (reload)
        {
            return;
        }
        try
        {
            switch (args.Subtype)
            {
                case EventSubtype.Event_CleanUI:
                    foreach (var handler in handlers)
                    {
                        handler.CleanUI();
                    }
                    break;
                case EventSubtype.Event_ReloadUI:
                    foreach (var handler in handlers)
                    {
                        handler.ReloadUI();
                    }
                    break;
                case EventSubtype.Event_DrawHUD:
                    foreach (var handler in handlers)
                    {
                        handler.DrawHUD();
                    }
                    break;
                case EventSubtype.Event_Pulse:
                    foreach (var handler in handlers)
                    {
                        handler.Pulse();
                    }
                    break;
                case EventSubtype.Event_BeginZone:
                    foreach (var handler in handlers)
                    {
                        handler.BeginZone();
                    }
                    break;
                case EventSubtype.Event_EndZone:
                    foreach (var handler in handlers)
                    {
                        handler.EndZone();
                    }
                    break;
                case EventSubtype.Event_Zoned:
                    foreach (var handler in handlers)
                    {
                        handler.Zoned();
                    }
                    break;
                case EventSubtype.Event_UpdateImGui:
                    foreach (var handler in handlers)
                    {
                        handler.UpdateImGui();
                    }
                    // if (MQ2Sharp.GetGameState() == MQ2Sharp.GAMESTATE_INGAME)
                    //     {
                    //         var pBool = MQ2Sharp.new_boolp();
                    //         MQ2Sharp.boolp_assign(pBool, true);

                    //         if (MQ2Sharp.Begin("MQ2Sharp", pBool, (int)ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
                    //         {
                    //             if (MQ2Sharp.BeginMenuBar())
                    //             {
                    //                 MQ2Sharp.Text("Dotnet is loaded and rendering IMGUI!");
                    //                 MQ2Sharp.EndMenuBar();
                    //             }
                    //         }
                    //         MQ2Sharp.End();
                    //     }
                    break;
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Logger.Log($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Logger.Log($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }

    public static void GameStateEventMethod(GameStateEvent args)
    {
        // Handle the GameStateEvent here
        if (reload)
        {
            return;
        }
        try
        {
            foreach (var handler in handlers)
            {
                handler.SetGameState(args.EventType);
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Logger.Log($"Error running sharp subtype :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Logger.Log($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }

    }


    public static void ChatColorEventMethod(ChatColorEvent args)
    {
        if (reload)
        {
            return;
        }
        try
        {
            // Convert IntPtr to string
            string message = Marshal.PtrToStringAnsi(args.Line);

            foreach (var handler in handlers)
            {
                handler.WriteChatColor(message ?? "", args.Color, args.Filter);
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Logger.Log($"Error running sharp subtype :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Logger.Log($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }


    public static bool IncomingChatEventMethod(IncomingChatEvent args)
    {
        if (reload)
        {
            return true;
        }
        bool ret = true;
        try
        {
            string message = Marshal.PtrToStringAnsi(args.Line);
            foreach (var handler in handlers)
            {
                ret = ret || handler.IncomingChat(message ?? "", args.Color);
            }
            Logger.Log($"Incoming Chat Line: {message}, Color: {args.Color}");
        }
        catch (Exception e)
        {
            if (reload)
            {
                return ret;
            }
            Logger.Log($"Error running sharp subtype :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Logger.Log($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
        return ret;
    }


    public static void SpawnEventMethod(SpawnEvent args)
    {
        if (reload)
        {
            return;
        }
        try
        {
            var spawn = EqFactory.CreatePlayerClient(args.Spawn, false);
            
            switch (args.Subtype)
            {
                case EventSubtype.Event_AddSpawn:
                    foreach (var handler in handlers)
                    {
                        handler.AddSpawn(spawn);
                    }
                    break;
                case EventSubtype.Event_RemoveSpawn:
                    foreach (var handler in handlers)
                    {
                        handler.RemoveSpawn(spawn);
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Logger.Log($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Logger.Log($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }

    public static void GroundItemEventMethod(GroundItemEvent args)
    {
        if (reload)
        {
            return;
        }
        try
        {
            // Don't have wrapper hooked up yet for ground item
            switch (args.Subtype)
            {
                case EventSubtype.Event_AddGroundItem:
                    foreach (var handler in handlers)
                    {
                        handler.AddGroundItem();
                    }
                    break;
                case EventSubtype.Event_RemoveGroundItem:
                    foreach (var handler in handlers)
                    {
                        handler.AddGroundItem();
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Logger.Log($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Logger.Log($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }

    public static void StringEventMethod(StringEvent args)
    {
        // Convert IntPtr to string
        string message = Marshal.PtrToStringAnsi(args.Msg) ?? "";

        if (reload)
        {
            return;
        }
        try
        {
            switch (args.Subtype)
            {
                case EventSubtype.Event_MacroStart:
                    foreach (var handler in handlers)
                    {
                        handler.MacroStart(message);
                    }
                    break;
                case EventSubtype.Event_MacroStop:
                    foreach (var handler in handlers)
                    {
                        handler.MacroStop(message);
                    }
                    break;
                case EventSubtype.Event_LoadPlugin:
                    foreach (var handler in handlers)
                    {
                        handler.LoadPlugin(message);
                    }
                    break;
                case EventSubtype.Event_UnloadPlugin:
                    foreach (var handler in handlers)
                    {
                        handler.UnloadPlugin(message);
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Logger.Log($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Logger.Log($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }

    private static ProgramSettings? _programPaths;
    private static bool reload = false;
    private static List<MQEventHandler> handlers = [];
    private static List<System.Timers.Timer> timers = new List<System.Timers.Timer>();
    private static CollectibleAssemblyLoadContext? assemblyContext_ = null;
    private static Assembly? sharpAssembly;
    private static Lazy<string> AssemblyDirectory = new Lazy<string>(() =>
    {
        string assemblyPath = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(assemblyPath);
    });


    private static System.Timers.Timer PollForChanges(ProgramSettings programSettings, Action callback)
    {
        var timer = new System.Timers.Timer(500);
        string path = programSettings.CompiledAssemblyPath;
        DateTime lastCheck = Directory.Exists(path) ? programSettings.GetFileNames().Max(File.GetLastWriteTimeUtc) : DateTime.MinValue;
        timer.Elapsed += (sender, args) =>
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var lastWriteTime = programSettings.GetFileNames()
                .Max(file => File.GetLastWriteTimeUtc(file));

            if (lastWriteTime > lastCheck)
            {
                Logger.Log($"Detected change in file at {path}- Reloading {programSettings.ProjectFileName}");
                callback();
                lastCheck = lastWriteTime;
            }
        };
        timer.Start();
        return timer;
    }

    public static void Initialize()
    {
        try {
        _programPaths =  new ProgramSettings(AssemblyDirectory.Value, "sharp", "sharp.csproj", "Sharp");
        Logger.Log("Initialize from dotnet");
        Logger.Log($"Watching for *.cs file changes in {_programPaths.AbsolutePath}");

        // Example of adding a /command and handling it in C#
        EQCommands.AddCommand("/sharp", (player, message) =>
        {
            Logger.Log($"Got sharp command with args: {message} - My player name {player.Name}");
        }, false, false, true);

        foreach (var timer in timers)
        {
            timer.Stop();
        }
        timers.Clear();

        if (Directory.Exists(_programPaths.AbsolutePath))
        {
            ReloadSharpWithLock();
        }

        timers.Add(PollForChanges(_programPaths, ReloadSharpAsync));
        } catch (Exception e) { 
        Logger.Log($"Got Exception {e.Message} {e.StackTrace}");
        }
       
    }

    public static void ReloadSharpAsync()
    {
        Task.Run(ReloadSharpWithLock);
    }

    private static void ReloadSharpWithLock()
    {
        var programAssemblyPath = _programPaths.CompiledAssemblyPath;
        if (File.Exists(programAssemblyPath))
        {
            ReloadSharp(false);
            return;
        }
        using (var mutex = new LockfileMutex($"lock{_programPaths.ProjectDirectory}AssemblyReload.lock"))
        {
            var firstAcquire = mutex.Acquire();
            Logger.Log($"Acquired first lock: {firstAcquire}");
            var counter = 0;
            if (!firstAcquire)
            {
                while (!mutex.Acquire())
                {
                    if (counter++ % 10 == 0)
                    {
                        Logger.Log($"Waiting to acquire {_programPaths.ProjectDirectory} lock...");
                    }
                    Thread.Sleep(100);
                    if (File.Exists(programAssemblyPath))
                    {
                        break;
                    }
                }
            }

            ReloadSharp(firstAcquire);
        }
    }

    public static void ReloadSharp(bool firstAcquire)
    {
        reload = true;
        foreach (var handler in handlers)
        {
            handler.Dispose();
        }

        handlers.Clear();

        // If we were not the first acquire we expect this to be built by someone else eventually
        if (!firstAcquire)
        {
            var counter = 0;
            var continueBuild = false;
            while (!File.Exists(_programPaths.CompiledAssemblyPath))
            {
                Logger.Log($"Waiting for another process to build {_programPaths.CompiledAssemblyPath} before continuing {counter} / 20");
                Thread.Sleep(1000);
                if (counter++ > 20)
                {
                    continueBuild = true;
                    break;
                }
            }
            if (!continueBuild)
            {
                if (assemblyContext_ == null)
                {
                    try
                    {

                        LoadDotNetProgram(_programPaths);
                        reload = false;
                        return;

                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Error loading existing {_programPaths.ProjectFileName} lib, continuing to recompile. {e.Message}");
                    }
                }
            }
        }

        if (File.Exists(_programPaths.CompiledAssemblyPath))
        {
            Logger.Log($"{_programPaths.ProjectFileName} dotnet lib up to date");
            if (assemblyContext_ == null)
            {
                try
                {
                    LoadDotNetProgram(_programPaths);
                    reload = false;
                    return;

                }
                catch (Exception e)
                {
                    Logger.Log($"Error loading existing {_programPaths.ProjectFileName} lib, continuing to recompile. {e.Message}");

                }

            }
        }

        if (assemblyContext_ != null)
        {
            assemblyContext_.Unload();
            assemblyContext_ = null;
            sharpAssembly = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        if (!File.Exists(_programPaths.AbsoluteProjectFilePath))
        {
            Logger.Log($"Project path does not exist at {_programPaths.AbsoluteProjectFilePath}");
            return;
        }

        if (Directory.Exists(_programPaths.OutDirectory))
        {
            // Clean up existing dll and pdb
            string[] filesToDelete = Directory.GetFiles(_programPaths.OutDirectory, "*", SearchOption.TopDirectoryOnly)
                                .Where(f => Path.GetFileName(f).StartsWith(_programPaths.ProjectDirectory, StringComparison.OrdinalIgnoreCase))
                                .ToArray();

            // Delete each file
            foreach (string file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Logger.Log($"An error occurred while deleting file {file}: {ex.Message}");
                }
            }
        }

        assemblyContext_ = new CollectibleAssemblyLoadContext(_programPaths.OutDirectory);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(AssemblyDirectory.Value, "DotNetCompiler.exe"),
            Arguments = $"{_programPaths.AssemblyGuid} {_programPaths.OutDirectory} {_programPaths.AbsolutePath} {_programPaths.ProjectFileName}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = AssemblyDirectory.Value,
        };


        //Logger.Log($"RootPath \at{_programPaths.RootPath}\ax");
        //Logger.Log($"ProjectDirectory \at{_programPaths.ProjectDirectory}\ax");
        //Logger.Log($"ProjectFileName \at{_programPaths.ProjectFileName}\ax");
        //Logger.Log($"Directory \at{_programPaths.Directory}\ax");
        //Logger.Log($"OutDirectory \at{_programPaths.OutDirectory}\ax");
        //Logger.Log($"ProjectFileDirectory \at{_programPaths.ProjectFileDirectory}\ax");
        //Logger.Log($"AssemblyGuid() \at{_programPaths.AssemblyGuid()}\ax");
        //Logger.Log($"CompiledAssemblyPath() \at{_programPaths.CompiledAssemblyPath()}\ax");
        //Logger.Log($"Arguments \at{startInfo.Arguments}\ax");

        using (var process = Process.Start(startInfo))
        {
            if (process == null)
            {
                Logger.Log($"Process was null when loading macro");
                return;
            }
            try
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                string errorOutput = process.StandardError.ReadToEnd();
                if (errorOutput.Length > 0 || output.Contains("FAILED"))
                {
                    Logger.Log($"Error compiling macro {_programPaths.ProjectFileName}:");
                    Logger.Log(errorOutput);
                    Logger.Log(output);
                }
                else
                {
                    Logger.Log(output);
                    Logger.Log($"Loading macro assembly from: {_programPaths.CompiledAssemblyPath}");
                    if (!File.Exists(_programPaths.CompiledAssemblyPath))
                    {
                        ReloadSharp(true);
                        return;
                    }

                    LoadDotNetProgram(_programPaths);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Exception in loading {_programPaths.ProjectDirectory} macro {e.Message}");
            }
        }
        reload = false;
    }

    private static void LoadDotNetProgram(ProgramSettings programSettings)
    {
        Logger.Log($"Loading sharp assembly from: {programSettings.CompiledAssemblyPath}");
        assemblyContext_ = new CollectibleAssemblyLoadContext(programSettings.OutDirectory);
        sharpAssembly = assemblyContext_.LoadFromAssemblyPath(programSettings.CompiledAssemblyPath);
        if (sharpAssembly.GetType(programSettings.StartupType) != null)
        {
            var Sharp = Activator.CreateInstance(sharpAssembly.GetType(programSettings.StartupType)) as MQEventHandler;
            if (Sharp != null)
            {
                handlers.Add(Sharp);
                Sharp.Main();
                Logger.Log($"Successfully loaded .NET sharp quests with {sharpAssembly.GetTypes().Count()} exported types.");
            }
        }
        else
        {
            Logger.Log($"Unable to load type '{programSettings.StartupType}'. Loaded assembly types: {string.Join(", ", sharpAssembly.GetTypes().Select(x => x.Name))}");
        }
    }

    public static bool MethodExistsAndIsConcrete(MethodInfo? methodInfo, Type parentType)
    {
        return methodInfo != null && methodInfo.DeclaringType != parentType;
    }

}

public class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    private readonly string _dependencyPath;

    public CollectibleAssemblyLoadContext(string dependencyPath) : base(isCollectible: true)
    {
        _dependencyPath = dependencyPath;
    }


    protected override Assembly Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == "DotNetTypes")
        {
            return null;
        }

        // Attempt to load the assembly from the specified dependency path
        string assemblyPath = Path.Combine(_dependencyPath, $"{assemblyName.Name}.dll");
        if (File.Exists(assemblyPath))
        {
            return LoadFromAssemblyPath(assemblyPath);
        }
        // Fallback to default context
        return null;
    }

}

internal record ProgramSettings(string RootPath, string ProjectDirectory, string ProjectFileName, string StartupType)
{
    public string AbsolutePath { get; } = Path.Combine(RootPath, ProjectDirectory);
    public string OutDirectory => Path.Combine(AbsolutePath, "out");
    public string AbsoluteProjectFilePath => Path.Combine(AbsolutePath, ProjectFileName);
    public string AssemblyGuid => $"{ProjectDirectory}-{GetDirHash(AbsolutePath)}";
    public string CompiledAssemblyPath => Path.Combine(OutDirectory, $"{AssemblyGuid}.dll");

    public string[] GetFileNames()
    {
        var path = Path.Combine(AbsolutePath);
        if (path.EndsWith(".csproj"))
        {
            return System.IO.Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly);
        }
        else if (path.EndsWith(".fsproj"))
        {
            return System.IO.Directory.GetFiles(path, "*.fs", SearchOption.TopDirectoryOnly);
        }

        return Array.Empty<string>();
    }

    private static string GetDirHash(string path)
    {
        if (!System.IO.Directory.Exists(path))
        {
            Logger.Log($"DirHash path not found: {path}");
            return "";
        }

        using (MD5 md5 = MD5.Create())
        {
            foreach (var file in GetFileAndProjectNames(path))
            {
                // Hash based on file contents, we don't care about arbitrary resaves
                byte[] contentBytes = File.ReadAllBytes(file);
                // Update MD5 with file content
                md5.TransformBlock(contentBytes, 0, contentBytes.Length, null, 0);
            }

            // Finalize the hash calculation
            md5.TransformFinalBlock(new byte[0], 0, 0); // Necessary to finalize the hash calculation
            return BitConverter.ToString(md5.Hash).Replace("-", string.Empty);
        }
    }

    private static IEnumerable<string> GetFileAndProjectNames(string path)
    {
        if (path.EndsWith(".csproj")) {
            return System.IO.Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly)
                    .Concat(System.IO.Directory.GetFiles(path, "*.csproj", SearchOption.TopDirectoryOnly));
        }
        else if (path.EndsWith(".fsproj"))
        {
            return System.IO.Directory.GetFiles(path, "*.fs", SearchOption.TopDirectoryOnly)
                    .Concat(System.IO.Directory.GetFiles(path, "*.fsproj", SearchOption.TopDirectoryOnly));
        }

        return Array.Empty<string>();
    }
}
