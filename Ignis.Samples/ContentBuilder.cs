using System.Diagnostics;

namespace Ignis.Samples;

/// <summary>
/// Helper class to build content using the MonoGame Content Pipeline
/// </summary>
public static class ContentBuilder
{
    /// <summary>
    /// Builds a .obj file into a .xnb file using MGCB (MonoGame Content Builder)
    /// </summary>
    public static bool BuildModel(string contentPath, string objFileName)
    {
        string objFilePath = Path.Combine(contentPath, objFileName);
        
        if (!File.Exists(objFilePath))
        {
            Console.WriteLine($"Error: {objFilePath} not found!");
            return false;
        }
        
        // Try to find MGCB tool
        string mgcbPath = FindMgcb();
        
        if (string.IsNullOrEmpty(mgcbPath))
        {
            Console.WriteLine("Warning: MGCB tool not found. Please build content manually.");
            return false;
        }
        
        // Setup output directories
        string outputDir = Path.Combine(contentPath, "bin", "DesktopGL");
        string intermediateDir = Path.Combine(contentPath, "obj", "DesktopGL");
        
        // Ensure directories exist
        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(intermediateDir);
        
        // Build arguments using CLI parameters
        var args = new List<string>
        {
            $"/outputDir:\"{outputDir}\"",
            $"/intermediateDir:\"{intermediateDir}\"",
            "/platform:DesktopGL",
            "/profile:HiDef",
            $"/workingDir:\"{contentPath}\"",
            "/importer:FbxImporter",
            "/processor:ModelProcessor",
            "/processorParam:ColorKeyColor=255,0,255,255",
            "/processorParam:ColorKeyEnabled=True",
            "/processorParam:DefaultEffect=BasicEffect",
            "/processorParam:GenerateMipmaps=True",
            "/processorParam:GenerateTangentFrames=False",
            "/processorParam:PremultiplyTextureAlpha=True",
            "/processorParam:PremultiplyVertexColors=True",
            "/processorParam:ResizeTexturesToPowerOfTwo=False",
            "/processorParam:RotationX=0",
            "/processorParam:RotationY=0",
            "/processorParam:RotationZ=0",
            "/processorParam:Scale=1",
            "/processorParam:SwapWindingOrder=False",
            "/processorParam:TextureFormat=Compressed",
            $"/build:\"{objFileName}\""
        };
        
        string arguments = string.Join(" ", args);
        
        Console.WriteLine($"Building {objFileName} with MGCB...");
        Console.WriteLine($"Command: {mgcbPath} {arguments}");
        
        // Build the content
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = mgcbPath,
                Arguments = arguments,
                WorkingDirectory = contentPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start MGCB process");
                return false;
            }
            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            Console.WriteLine("MGCB Output:");
            Console.WriteLine(output);
            
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("MGCB Errors:");
                Console.WriteLine(error);
            }
            
            if (process.ExitCode == 0)
            {
                // Copy the built XNB file to the Content root directory
                string xnbFileName = Path.GetFileNameWithoutExtension(objFileName) + ".xnb";
                string builtXnbPath = Path.Combine(outputDir, xnbFileName);
                string targetXnbPath = Path.Combine(contentPath, xnbFileName);
                
                Console.WriteLine($"Looking for built XNB at: {builtXnbPath}");
                Console.WriteLine($"Target XNB path: {targetXnbPath}");
                
                if (File.Exists(builtXnbPath))
                {
                    // Copy to source Content directory
                    File.Copy(builtXnbPath, targetXnbPath, overwrite: true);
                    Console.WriteLine($"Copied {xnbFileName} to source Content directory");
                    
                    // Also copy to the runtime output directory (bin/Debug/net8.0/Content)
                    // Get the base directory where the executable is running from
                    string executableDir = AppDomain.CurrentDomain.BaseDirectory;
                    string runtimeContentDir = Path.Combine(executableDir, "Content");
                    Directory.CreateDirectory(runtimeContentDir);
                    
                    string runtimeXnbPath = Path.Combine(runtimeContentDir, xnbFileName);
                    File.Copy(builtXnbPath, runtimeXnbPath, overwrite: true);
                    Console.WriteLine($"Copied {xnbFileName} to runtime Content directory: {runtimeXnbPath}");
                    Console.WriteLine($"File exists at runtime location: {File.Exists(runtimeXnbPath)}");
                }
                else
                {
                    Console.WriteLine($"Warning: Built XNB file not found at {builtXnbPath}");
                }
            }
            
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running MGCB: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Builds a .fx file into a .xnb file using MGCB (MonoGame Content Builder)
    /// </summary>
    public static bool BuildEffect(string contentPath, string fxFileName)
    {
        string fxFilePath = Path.Combine(contentPath, fxFileName);
        
        if (!File.Exists(fxFilePath))
        {
            Console.WriteLine($"Error: {fxFilePath} not found!");
            return false;
        }
        
        // Try to find MGCB tool
        string mgcbPath = FindMgcb();
        
        if (string.IsNullOrEmpty(mgcbPath))
        {
            Console.WriteLine("Warning: MGCB tool not found. Please build content manually.");
            return false;
        }
        
        // Setup output directories
        string outputDir = Path.Combine(contentPath, "bin", "DesktopGL");
        string intermediateDir = Path.Combine(contentPath, "obj", "DesktopGL");
        
        // Ensure directories exist
        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(intermediateDir);
        
        // Build arguments using CLI parameters
        var args = new List<string>
        {
            $"/outputDir:\"{outputDir}\"",
            $"/intermediateDir:\"{intermediateDir}\"",
            "/platform:DesktopGL",
            "/profile:HiDef",
            $"/workingDir:\"{contentPath}\"",
            "/importer:EffectImporter",
            "/processor:EffectProcessor",
            "/processorParam:DebugMode=Auto",
            $"/build:\"{fxFileName}\""
        };
        
        string arguments = string.Join(" ", args);
        
        Console.WriteLine($"Building {fxFileName} with MGCB...");
        Console.WriteLine($"Command: {mgcbPath} {arguments}");
        
        // Build the content
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = mgcbPath,
                Arguments = arguments,
                WorkingDirectory = contentPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start MGCB process");
                return false;
            }
            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            Console.WriteLine("MGCB Output:");
            Console.WriteLine(output);
            
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("MGCB Errors:");
                Console.WriteLine(error);
            }
            
            if (process.ExitCode == 0)
            {
                // Copy the built XNB file to the Content root directory
                string xnbFileName = Path.GetFileNameWithoutExtension(fxFileName) + ".xnb";
                string builtXnbPath = Path.Combine(outputDir, xnbFileName);
                string targetXnbPath = Path.Combine(contentPath, xnbFileName);
                
                Console.WriteLine($"Looking for built XNB at: {builtXnbPath}");
                Console.WriteLine($"Target XNB path: {targetXnbPath}");
                
                if (File.Exists(builtXnbPath))
                {
                    // Copy to source Content directory
                    File.Copy(builtXnbPath, targetXnbPath, overwrite: true);
                    Console.WriteLine($"Copied {xnbFileName} to source Content directory");
                    
                    // Also copy to the runtime output directory (bin/Debug/net8.0/Content)
                    string executableDir = AppDomain.CurrentDomain.BaseDirectory;
                    string runtimeContentDir = Path.Combine(executableDir, "Content");
                    Directory.CreateDirectory(runtimeContentDir);
                    
                    string runtimeXnbPath = Path.Combine(runtimeContentDir, xnbFileName);
                    File.Copy(builtXnbPath, runtimeXnbPath, overwrite: true);
                    Console.WriteLine($"Copied {xnbFileName} to runtime Content directory: {runtimeXnbPath}");
                    Console.WriteLine($"File exists at runtime location: {File.Exists(runtimeXnbPath)}");
                }
                else
                {
                    Console.WriteLine($"Warning: Built XNB file not found at {builtXnbPath}");
                }
            }
            
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running MGCB: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Tries to find the MGCB tool
    /// </summary>
    private static string FindMgcb()
    {
        // Try common locations
        string[] possiblePaths =
        [
            "mgcb",
            "dotnet-mgcb",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", "mgcb.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", "dotnet-mgcb.exe")
        ];
        
        foreach (var path in possiblePaths)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "/?",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0 || process.ExitCode == 1) // MGCB returns 1 for help
                    {
                        return path;
                    }
                }
            }
            catch
            {
                // Continue to next path
            }
        }
        
        return string.Empty;
    }
}

