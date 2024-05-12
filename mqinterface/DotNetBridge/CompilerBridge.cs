using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.MQConsole.Themes;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Serilog.Extensions.Logging;
using Serilog.Core;

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
    static readonly ILogger<MQ2Sharp> _logger;
    static MQInterface()
    {
        var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.MQConsole(theme: MQConsoleTheme.Colored)
                    .WriteTo.Async(a => a.File($"{MQ2Sharp.gPathLogs}\\MQ2Sharp_log.txt", rollingInterval: RollingInterval.Day))
                    .CreateLogger();

        var loggerFactory = new LoggerFactory()
            .AddSerilog(serilogLogger);

        _logger = loggerFactory.CreateLogger<MQ2Sharp>();
    }

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
            _logger.LogError(e, "Error running sharp subtype {args.Subtype} :: {Message}", args.Subtype, e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Error running sharp. Inner Exception: {Message}", inner.Message);
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
            _logger.LogError(e, "Error running sharp subtype :: {Message}", e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Error running sharp. Inner Exception: {Message}", inner.Message);
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
            string? message = Marshal.PtrToStringAnsi(args.Line);

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
            _logger.LogError(e, "Error running sharp subtype :: {Message}", e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Error running sharp. Inner Exception: {Message}", inner.Message);
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
            string? message = Marshal.PtrToStringAnsi(args.Line);
            foreach (var handler in handlers)
            {
                ret = ret || handler.IncomingChat(message ?? "", args.Color);
            }
            _logger.LogInformation("Incoming Chat Line: '{message}', Color: {Color}", message, args.Color);
        }
        catch (Exception e)
        {
            if (reload)
            {
                return ret;
            }
            _logger.LogError(e, "Error running sharp subtype :: {Message}", e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Error running sharp. Inner Exception: {Message}", inner.Message);
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
            _logger.LogError(e, "Error running sharp subtype {args.Subtype} :: {Message}", args.Subtype, e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Error running sharp. Inner Exception: {Message}", inner.Message);
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
            _logger.LogError(e, "Error running sharp subtype {args.Subtype} :: {Message}", args.Subtype, e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Error running sharp. Inner Exception: {Message}", inner.Message);
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
            _logger.LogError(e, "Error running sharp subtype {args.Subtype} :: {Message}", args.Subtype, e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Error running sharp. Inner Exception: {Message}", inner.Message);
                inner = inner.InnerException;
            }
        }
    }

    private static ProgramSettings? _programSettings;
    private static bool reload = false;
    private static List<MQEventHandler> handlers = [];
    private static List<System.Timers.Timer> timers = new List<System.Timers.Timer>();
    private static CollectibleAssemblyLoadContext? assemblyContext_ = null;
    private static Assembly? sharpAssembly;
    private static Lazy<string> AssemblyDirectory = new Lazy<string>(() =>
    {
        string assemblyPath = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(assemblyPath) ?? "";
    });

    // https://stackoverflow.com/questions/67405323/c-sharp-throttle-example
    // https://stackoverflow.com/questions/60230128/debounce-event-command-in-mvvm-pattern
    // https://stackoverflow.com/questions/55107390/net-async-await-event-debouncer-throttler
    public class Throttle<TEventArgs>
    {
        private readonly System.Timers.Timer _timer;
        private object _lastSender;
        private TEventArgs _lastEventArgs;

        public Throttle(EventHandler<TEventArgs> eventHandler, TimeSpan interval)
        {
            _timer = new System.Timers.Timer
            {
                Interval = interval.TotalMilliseconds,
                AutoReset = false
            };
            _timer.Elapsed += (s, e) =>
            {
                _timer.Stop();
                eventHandler(_lastSender, _lastEventArgs);
            };
        }

        public void ProcessEvent(object sender, TEventArgs args)
        {
            _timer.Stop();
            _timer.Start();

            _lastSender = sender;
            _lastEventArgs = args;
        }
    }

    private static FileSystemWatcher? _fileSystemWatcher;
    private static FileSystemWatcher CreateFileSystemWatcher(ProgramSettings programSettings, Action<ProgramSettings> callback)
    {
        var timer = new System.Timers.Timer
        {
            Interval = 200,
            AutoReset = false
        };
        timer.Elapsed += (s, e) =>
        {
            timer.Stop();
            _logger.LogInformation("Detected change in file at {CompiledAssemblyPath} - Reloading {ProjectFileName}", programSettings.CompiledAssemblyPath(), programSettings.ProjectFileName);
            callback(programSettings);
        };

        var watcher = new FileSystemWatcher(programSettings.AbsolutePath);
        watcher.NotifyFilter = NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite;

        watcher.Changed += (s, e) => {
            if (e.FullPath.EndsWith(programSettings.MacroType.SourceFileSuffix))
            {
                timer.Stop();
                timer.Start();
            }
        };
        
        watcher.Created += (s, e) => {
            if (e.FullPath.EndsWith(programSettings.MacroType.SourceFileSuffix))
            {
                timer.Stop();
                timer.Start();
            }
        }; 

        watcher.Deleted += (s, e) => {
            timer.Stop();
            timer.Start();
        };

        watcher.Renamed += (s, e) => {
            if (e.FullPath.EndsWith(programSettings.MacroType.SourceFileSuffix))
            {
                timer.Stop();
                timer.Start();
            }
        };

        watcher.Error += (s, e) => {
            var exception = e.GetException();
            _logger.LogError(exception, "Exception trying to detect file changes for {CompiledAssemblyPath} - {Message}", programSettings.CompiledAssemblyPath(), exception.Message);
        };

        watcher.Filter = $"*{programSettings.MacroType.SourceFileSuffix}";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    public static void Initialize()
    {
        try
        {
            _programSettings = new ProgramSettings(AssemblyDirectory.Value, "sharp", "sharp.csproj", "Sharp", _logger);
            _logger.LogInformation("Initialize from dotnet");

            // Example of adding a /command and handling it in C#
            EQCommands.AddCommand("/sharp", (player, message) =>
            {
                _logger.LogInformation("Got sharp command with args: {message} - My player name {PlayerName}", message, player.Name);
            }, false, false, true);

            foreach (var timer in timers)
            {
                timer.Stop();
            }
            timers.Clear();

            if (Directory.Exists(_programSettings.AbsolutePath))
            {
                ReloadSharpWithLock(_programSettings);
            }

            _logger.LogInformation("Watching for {SourceFileSuffix} file changes in {AbsolutePath}", _programSettings.MacroType.SourceFileSuffix, _programSettings.AbsolutePath);
            _fileSystemWatcher = CreateFileSystemWatcher(_programSettings, ReloadSharpAsync);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Got Exception {Message}", e.Message);
        }
    }

    internal static void ReloadSharpAsync(ProgramSettings programSettings)
    {
        Task.Run(() => ReloadSharpWithLock(programSettings));
    }

    private static void ReloadSharpWithLock(ProgramSettings programSettings)
    {
        var programAssemblyPath = programSettings.CompiledAssemblyPath();
        if (File.Exists(programAssemblyPath))
        {
            ReloadSharp(false, programSettings);
            return;
        }
        using (var mutex = new LockfileMutex($"lock{programSettings.ProjectDirectory}AssemblyReload.lock"))
        {
            var firstAcquire = mutex.Acquire();
            _logger.LogInformation("Acquired first lock: {FirstAcquire}", firstAcquire);
            var counter = 0;
            if (!firstAcquire)
            {
                while (!mutex.Acquire())
                {
                    if (counter++ % 10 == 0)
                    {
                        _logger.LogInformation("Waiting to acquire {ProjectDirectory} lock...", programSettings.ProjectDirectory);
                    }
                    Thread.Sleep(100);
                    if (File.Exists(programAssemblyPath))
                    {
                        break;
                    }
                }
            }

            ReloadSharp(firstAcquire, programSettings);
        }
    }

    internal static void ReloadSharp(bool firstAcquire, ProgramSettings programSettings)
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
            while (!File.Exists(programSettings.CompiledAssemblyPath()))
            {
                _logger.LogInformation("Waiting for another process to build {CompiledAssemblyPath} before continuing {Counter} / 20", programSettings.CompiledAssemblyPath() ,counter);
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

                        LoadDotNetProgram(programSettings);
                        reload = false;
                        return;

                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error loading existing {ProjectFileName} lib, continuing to recompile. {Message}", programSettings.ProjectFileName, e.Message);
                    }
                }
            }
        }

        if (File.Exists(programSettings.CompiledAssemblyPath()))
        {
            _logger.LogInformation("{ProjectFileName} dotnet lib up to date", programSettings.ProjectFileName);
            if (assemblyContext_ == null)
            {
                try
                {
                    LoadDotNetProgram(programSettings);
                    reload = false;
                    return;

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error loading existing {ProjectFileName} lib, continuing to recompile. {Message}", programSettings.ProjectFileName, e.Message);

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

        if (!File.Exists(programSettings.AbsoluteProjectFilePath))
        {
            _logger.LogError("Project path does not exist at {AbsoluteProjectFilePath}", programSettings.AbsoluteProjectFilePath);
            return;
        }

        if (Directory.Exists(programSettings.OutDirectory))
        {
            // Clean up existing dll and pdb
            string[] filesToDelete = Directory.GetFiles(programSettings.OutDirectory, "*", SearchOption.TopDirectoryOnly)
                                .Where(f => Path.GetFileName(f).StartsWith(programSettings.ProjectDirectory, StringComparison.OrdinalIgnoreCase))
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
                    _logger.LogInformation($"An error occurred while deleting file {file}: {ex.Message}");
                }
            }
        }

        assemblyContext_ = new CollectibleAssemblyLoadContext(programSettings.OutDirectory);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(AssemblyDirectory.Value, "DotNetCompiler.exe"),
            Arguments = $"{programSettings.AssemblyGuid()} {programSettings.OutDirectory} {programSettings.AbsolutePath} {programSettings.ProjectFileName}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = AssemblyDirectory.Value,
        };


        _logger.LogDebug("RootPath {RootPath}", programSettings.RootPath);
        _logger.LogDebug("ProjectDirectory {ProjectDirectory}", programSettings.ProjectDirectory);
        _logger.LogDebug("ProjectFileName {ProjectFileName}", programSettings.ProjectFileName);
        _logger.LogDebug("ProjectFileName {AbsolutePath}", programSettings.AbsolutePath);
        _logger.LogDebug("Directory {AbsoluteProjectFilePath}", programSettings.AbsoluteProjectFilePath);
        _logger.LogDebug("OutDirectory {OutDirectory}", programSettings.OutDirectory);
        _logger.LogDebug("AssemblyGuid() {AssemblyGuid}", programSettings.AssemblyGuid());
        _logger.LogDebug("CompiledAssemblyPath() {CompiledAssemblyPath}", programSettings.CompiledAssemblyPath());
        _logger.LogDebug("Arguments {Arguments}", startInfo.Arguments);

        using (var process = Process.Start(startInfo))
        {
            if (process == null)
            {
                _logger.LogInformation($"Process was null when loading macro");
                return;
            }
            try
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                string errorOutput = process.StandardError.ReadToEnd();
                if (errorOutput.Length > 0 || output.Contains("FAILED"))
                {
                    _logger.LogError("Error compiling macro {ProjectFileName}:", programSettings.ProjectFileName);
                    _logger.LogError(errorOutput);
                    _logger.LogError(output);
                }
                else
                {
                    _logger.LogInformation(output);
                    _logger.LogInformation("Loading macro assembly from: {CompiledAssemblyPath}", programSettings.CompiledAssemblyPath());
                    if (!File.Exists(programSettings.CompiledAssemblyPath()))
                    {
                        ReloadSharp(true, programSettings);
                        return;
                    }

                    LoadDotNetProgram(programSettings);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in loading {ProjectDirectory} macro {Message}", programSettings.ProjectDirectory, e.Message);
            }
        }
        reload = false;
    }

    private static void LoadDotNetProgram(ProgramSettings programSettings)
    {
        _logger.LogInformation("Loading sharp assembly from: {CompiledAssemblyPath}", programSettings.CompiledAssemblyPath());
        assemblyContext_ = new CollectibleAssemblyLoadContext(programSettings.OutDirectory);
        sharpAssembly = assemblyContext_.LoadFromAssemblyPath(programSettings.CompiledAssemblyPath());
        Type? startupType = sharpAssembly.GetType(programSettings.StartupType);
        if (startupType != null)
        {
            var Sharp = Activator.CreateInstance(startupType) as MQEventHandler;
            if (Sharp != null)
            {
                handlers.Add(Sharp);
                Sharp.Main(_logger);
                _logger.LogInformation("Successfully loaded .NET sharp Macro/Program with {TypesCount} exported types.", sharpAssembly.GetTypes().Count());
            }
        }
        else
        {
            _logger.LogError("Unable to load type '{StartupType}'. Loaded assembly types: {NumTypes}", programSettings.StartupType, string.Join(", ", sharpAssembly.GetTypes().Select(x => x.Name)));
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


    protected override Assembly? Load(AssemblyName assemblyName)
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

internal class MacroType
{
    public static MacroType Unknown = new MacroType("");
    public static MacroType CSharp = new MacroType(".cs");
    public static MacroType FSharp = new MacroType(".fs");

    private MacroType(string sourceFileSuffix) {
        SourceFileSuffix = sourceFileSuffix;
    }

    public string SourceFileSuffix { get; }

    public string[] GetFileNames(string path) {
        if (string.IsNullOrWhiteSpace(SourceFileSuffix) || string.IsNullOrWhiteSpace(path)) { 
            return Array.Empty<string>();
        }

        return System.IO.Directory.GetFiles(path, SourceFileSuffix, SearchOption.TopDirectoryOnly);
    }

    public static MacroType From(string projectFilePath) {
        if (projectFilePath.EndsWith(".csproj"))
        {
            return MacroType.CSharp;
        }
        else if (projectFilePath.EndsWith(".fsproj"))
        {
            return MacroType.FSharp;
        }

        return MacroType.Unknown;

    }
}

internal record ProgramSettings(string RootPath, string ProjectDirectory, string ProjectFileName, string StartupType, ILogger<MQ2Sharp> Logger)
{
    public string AbsolutePath { get; } = Path.Combine(RootPath, ProjectDirectory);
    public string OutDirectory => Path.Combine(AbsolutePath, "out");
    public string AbsoluteProjectFilePath => Path.Combine(AbsolutePath, ProjectFileName);
    public string AssemblyGuid() => $"{ProjectDirectory}-{GetDirHash(AbsolutePath, Logger)}";
    public string CompiledAssemblyPath() => Path.Combine(OutDirectory, $"{AssemblyGuid()}.dll");
    public MacroType MacroType => MacroType.From(AbsoluteProjectFilePath);

    public string[] GetFileNames()
    {
        return MacroType.GetFileNames(Path.Combine(AbsolutePath));
    }

    private static string GetDirHash(string path, ILogger<MQ2Sharp> logger)
    {
        if (!System.IO.Directory.Exists(path))
        {
            logger.LogInformation($"DirHash path not found: {path}");
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
            return BitConverter.ToString(md5.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty);
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
