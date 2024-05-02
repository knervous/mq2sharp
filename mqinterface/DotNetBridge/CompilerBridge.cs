using System.Diagnostics;
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
                    break;
                case EventSubtype.Event_ReloadUI:
                    break;
                case EventSubtype.Event_DrawHUD:
                    break;
                case EventSubtype.Event_Pulse:
                    break;
                case EventSubtype.Event_BeginZone:
                    break;
                case EventSubtype.Event_EndZone:
                    break;
                case EventSubtype.Event_Zoned:
                    break;
                case EventSubtype.Event_UpdateImGui:
                	if (MQ2Sharp.GetGameState() == MQ2Sharp.GAMESTATE_INGAME)
                        {
                            var pBool = MQ2Sharp.new_boolp();
                            MQ2Sharp.boolp_assign(pBool, true);
                            
                            if (MQ2Sharp.Begin("MQ2Sharp", pBool, (int)ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
                            {
                                if (MQ2Sharp.BeginMenuBar())
                                {
                                    MQ2Sharp.Text("Dotnet is loaded and rendering IMGUI!");
                                    MQ2Sharp.EndMenuBar();
                                }
                            }
                            MQ2Sharp.End();
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
            Console.WriteLine($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"Error running sharp. Inner Exception: {inner.Message}");
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

        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Console.WriteLine($"Error running sharp subtype :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"Error running sharp. Inner Exception: {inner.Message}");
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
            Console.WriteLine($"Chat Line: {message}, Color: {args.Color}, Filter: {args.Filter}");
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Console.WriteLine($"Error running sharp subtype :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }


    public static void IncomingChatEventMethod(IncomingChatEvent args)
    {
        if (reload)
        {
            return;
        }
        try
        {
            string message = Marshal.PtrToStringAnsi(args.Line);
            Console.WriteLine($"Incoming Chat Line: {message}, Color: {args.Color}");
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Console.WriteLine($"Error running sharp subtype :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
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
                    Logger.Log($"Spawn: {spawn.Name}");
                    break;
                case EventSubtype.Event_RemoveSpawn:
                    Logger.Log($"Despawn: {spawn.Name}");
                    break;
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Console.WriteLine($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"Error running sharp. Inner Exception: {inner.Message}");
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
            switch (args.Subtype)
            {
                case EventSubtype.Event_AddGroundItem:
                    break;
                case EventSubtype.Event_RemoveGroundItem:
                    break;
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Console.WriteLine($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }

    public static void StringEventMethod(StringEvent args)
    {
        // Convert IntPtr to string
        string message = Marshal.PtrToStringAnsi(args.Msg);
        Logger.Log($"String Event Message: {message}");

        if (reload)
        {
            return;
        }
        try
        {
            switch (args.Subtype)
            {
                case EventSubtype.Event_MacroStart:
                    break;
                case EventSubtype.Event_MacroStop:
                    break;
                case EventSubtype.Event_LoadPlugin:
                    break;
                case EventSubtype.Event_UnloadPlugin:
                    break;
            }
        }
        catch (Exception e)
        {
            if (reload)
            {
                return;
            }
            Console.WriteLine($"Error running sharp subtype {args.Subtype} :: {e.Message}");
            var inner = e.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"Error running sharp. Inner Exception: {inner.Message}");
                inner = inner.InnerException;
            }
        }
    }
    private static bool reload = false;
    private static List<System.Timers.Timer> timers = new List<System.Timers.Timer>();

    private static System.Timers.Timer PollForChanges(string path, Action callback)
    {

        var timer = new System.Timers.Timer(500);
        DateTime lastCheck = Directory.Exists(path) ? Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly).Max(File.GetLastWriteTimeUtc) : DateTime.MinValue;
        timer.Elapsed += (sender, args) =>
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var lastWriteTime = Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly)
                .Max(file => File.GetLastWriteTimeUtc(file));

            if (lastWriteTime > lastCheck)
            {
                Console.WriteLine($"Detected change in .cs file in {path}- Reloading sharp");
                callback();
                lastCheck = lastWriteTime;
            }
        };
        timer.Start();
        return timer;
    }

    private static string GetDirHash(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"DirHash path not found: {path}");
            return "";
        }

        using (MD5 md5 = MD5.Create())
        {
            foreach (string file in Directory.GetFiles(path, "*.cs", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.GetFiles(path, "*.csproj", SearchOption.TopDirectoryOnly)))
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

    private static CollectibleAssemblyLoadContext? assemblyContext_ = null;
    private static Assembly? sharpAssembly;

    public static void Initialize()
    {
        var workingDirectory = Directory.GetCurrentDirectory();
        var sharpDir = Path.Combine(workingDirectory, "sharp");
        Logger.Log("Initialize from dotnet");
        Console.WriteLine($"Watching for *.cs file changes in {sharpDir}");

        foreach (var timer in timers)
        {
            timer.Stop();
        }
        timers.Clear();

        if (Directory.Exists(sharpDir))
        {
            ReloadSharpWithLock();
        }

        timers.Add(PollForChanges(sharpDir, ReloadSharpAsync));
    }

    public static void ReloadSharpAsync()
    {
        Task.Run(ReloadSharpWithLock);
    }

    private static string SharpAssemblyPath
    {
        get
        {
            var workingDirectory = Directory.GetCurrentDirectory();
            var hash = GetDirHash($"{workingDirectory}/sharp");
            var sharpGuid = $"sharp-{hash}";
            return $"{workingDirectory}/sharp/out/{sharpGuid}.dll";
        }
    }

    private static void ReloadSharpWithLock()
    {
        if (File.Exists(SharpAssemblyPath))
        {
            ReloadSharp(false);
            return;
        }
        using (var mutex = new LockfileMutex("lockSharpAssemblyReload.lock"))
        {
            var firstAcquire = mutex.Acquire();
            Console.WriteLine($"Acquired first lock: {firstAcquire}");
            var counter = 0;
            if (!firstAcquire)
            {
                while (!mutex.Acquire())
                {
                    if (counter++ % 10 == 0)
                    {
                        Console.WriteLine("Waiting to acquire sharp lock...");
                    }
                    Thread.Sleep(100);
                    if (File.Exists(SharpAssemblyPath))
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

        if (sharpAssembly?.GetType("Sharp")?.GetMethod("Dispose") != null)
        {
            sharpAssembly.GetType("Sharp")?.GetMethod("Dispose")?.Invoke(null, []);
        }
        var workingDirectory = Directory.GetCurrentDirectory();
        var directoryPath = $"{workingDirectory}/sharp";
        var outPath = $"{workingDirectory}/sharp/out";
        var projPath = $"{directoryPath}/sharp.csproj";
        var hash = GetDirHash(directoryPath);
        var sharpGuid = $"sharp-{hash}";
        var sharpAssemblyPath = $"{outPath}/{sharpGuid}.dll";

        // If we were not the first acquire we expect this to be built by someone else eventually
        if (!firstAcquire)
        {
            var counter = 0;
            var continueBuild = false;
            while (!File.Exists(SharpAssemblyPath))
            {
                Console.WriteLine($"Waiting for another process to build {sharpAssemblyPath} before continuing {counter} / 20");
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
                        assemblyContext_ = new CollectibleAssemblyLoadContext(outPath);
                        sharpAssembly = assemblyContext_.LoadFromAssemblyPath(sharpAssemblyPath);
                        if (sharpAssembly.GetType("Sharp")?.GetMethod("Init") != null)
                        {
                            sharpAssembly.GetType("Sharp")?.GetMethod("Init")?.Invoke(null, []);
                        }

                        reload = false;
                        return;

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error loading existing sharp lib, continuing to recompile. {e.Message}");
                    }
                }
            }
        }

        if (File.Exists(sharpAssemblyPath))
        {
            Console.WriteLine($"sharp dotnet lib up to date");
            if (assemblyContext_ == null)
            {
                try
                {
                    assemblyContext_ = new CollectibleAssemblyLoadContext(outPath);
                    sharpAssembly = assemblyContext_.LoadFromAssemblyPath(sharpAssemblyPath);
                    if (sharpAssembly.GetType("Sharp")?.GetMethod("Init") != null)
                    {
                        sharpAssembly.GetType("Sharp")?.GetMethod("Init")?.Invoke(null, []);
                    }
                    Console.WriteLine($"Successfully loaded .NET sharp quests with {sharpAssembly.GetTypes().Count()} exported types.");
                    reload = false;
                    return;

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error loading existing sharp lib, continuing to recompile. {e.Message}");

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

        if (!File.Exists(projPath))
        {
            Console.WriteLine($"Project path does not exist at {projPath}");
            return;
        }
        if (Directory.Exists(outPath))
        {
            // Clean up existing dll and pdb
            string[] filesToDelete = Directory.GetFiles(outPath, "*", SearchOption.TopDirectoryOnly)
                                .Where(f => Path.GetFileName(f).StartsWith("sharp", StringComparison.OrdinalIgnoreCase))
                                .ToArray();

            // Delete each file
            foreach (string file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"An error occurred while deleting file {file}: {ex.Message}");
                }
            }
        }

        assemblyContext_ = new CollectibleAssemblyLoadContext(outPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(workingDirectory + "/bin/dotnet/DotNetCompiler"),
            Arguments = $"{sharpGuid} {outPath} {directoryPath}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = directoryPath,
        };

        using (var process = Process.Start(startInfo))
        {
            if (process == null)
            {
                Console.WriteLine($"Process was null when loading sharp");
                return;
            }
            try
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                string errorOutput = process.StandardError.ReadToEnd();
                if (errorOutput.Length > 0 || output.Contains("FAILED"))
                {
                    Console.WriteLine($"Error compiling sharp:");
                    Console.WriteLine(errorOutput);
                    Console.WriteLine(output);
                }
                else
                {
                    Console.WriteLine(output);
                    Console.WriteLine($"Loading sharp assembly from: {sharpAssemblyPath}");
                    if (!File.Exists(sharpAssemblyPath))
                    {
                        ReloadSharp(true);
                        return;
                    }
                    sharpAssembly = assemblyContext_.LoadFromAssemblyPath(sharpAssemblyPath);
                    Console.WriteLine($"Successfully compiled MQ2Sharp with {sharpAssembly.GetTypes().Count()} exported types.");
                    if (sharpAssembly.GetType("ZoneLoad")?.GetMethod("Init") != null)
                    {
                        sharpAssembly.GetType("ZoneLoad")?.GetMethod("Init")?.Invoke(null, []);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in loading sharp quests {e.Message}");
            }
        }
        reload = false;
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
