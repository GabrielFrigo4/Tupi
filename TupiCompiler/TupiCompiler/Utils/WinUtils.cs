using System.Runtime.InteropServices;

namespace TupiCompiler.Utility;
internal class WinUtils
{
    /// <summary>
    /// Add environment path in this process on Windows
    /// </summary>
    /// <param name="path"></param>
    public static void AddEnvironmentPath(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path") + ";" + Path.GetFullPath(path));
    }
}
