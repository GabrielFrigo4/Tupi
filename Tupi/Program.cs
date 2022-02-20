using System;
using System.Diagnostics;

namespace Tupi;
public class Program
{
    static int Main(string[] args)
    {
        CompileTupi("./mycode.tu");
        return 0;
    }

    static void CompileTupi(string path_tupi)
    {

    }

    static void CompileAsm(string path_dir_asm)
    {
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C cd \"{path_dir_asm}\" call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\" && ml64 main.asm /link /subsystem:console /defaultlib:kernel32.lib /defaultlib:user32.lib /defaultlib:libcmt.lib && main";
        process.StartInfo = startInfo;
        process.Start();
    }
}