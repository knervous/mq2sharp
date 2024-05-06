using System.Diagnostics;

class DotNetCompiler
{
    static void Main(string[] args)
    {
        var guid = args[0];
        var outPath = args[1];
        var directoryPath = args[2];
        var projectFileName = args[3];
        var compilerFlag = Environment.Is64BitProcess ? "-r win-x64" : "-r win-x86";
        Console.WriteLine("Using DotNetCompiler");
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build --output {outPath} {compilerFlag} -nowarn:CS0114 -p:Configuration=Debug -p:AssemblyName={guid} {directoryPath}/{projectFileName}",
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
                Console.Error.WriteLine($"Process was null when loading sharp");
                return;
            }
            try
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd().Trim();
                string errorOutput = process.StandardError.ReadToEnd();
                if (errorOutput.Length > 0 || output.Contains("FAILED"))
                {
                    Console.Error.WriteLine($"Error compiling quests:");
                    Console.Error.WriteLine(errorOutput);
                    Console.Error.WriteLine(output);

                }
                else
                {
                    Console.WriteLine(output);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Exception in loading zone quest {e.Message}");
            }
            finally
            {
                foreach (var dotnetProcess in Process.GetProcessesByName("dotnet"))
                {
                    try
                    {
                        dotnetProcess.Kill();
                        dotnetProcess.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions, possibly logging them or notifying the user.
                        Console.WriteLine($"Error killing process {dotnetProcess.Id}: {ex.Message}");
                    }
                }
            }
        }

    }
}