using System;
using System.Diagnostics;

namespace Tupi;
public class Program
{
    static int Main(string[] args)
    {
        CompileTupi("./mycode.tp");
        return 0;
    }

    static void CompileTupi(string path_tupi)
    {
        string tupi_code = File.ReadAllText(path_tupi);
        string[] lines = tupi_code.Split('\n');
        string asm_code = string.Empty;

        for(int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] words = line.Split(new char[] { '\t', '\n', ' ', '?', '!', '.', ',', ';' });

            //certo
            line = line.Replace("extern printf", "extern printf: proc");
            //certo
            line = line.Replace("int8 ms", ".data\nms db");
            //certo
            line = line.Replace("func main(){", ".code\nmain proc\n\tsub rsp, 28h\t;Reserve the shadow space");
            //errado
            line = line.Replace("printf(ms)", "lea rcx, ms\n\tcall printf");
            //certo
            line = line.Replace("return ", "mov rax, ");
            //certo
            line = line.Replace("}", "\tadd rsp, 28h\t;Remove shadow space\n\tret\nmain endp\nEnd");

            lines[i] = line;
        }

        foreach(string line in lines)
        {
            asm_code += line;
        }

        string path_dir = "./build";
        Directory.CreateDirectory(path_dir);
        StreamWriter write = File.CreateText(path_dir+@"\main.asm");
        write.Write(asm_code);
        write.Close();
        CompileAsm(path_dir);
    }

    static void CompileAsm(string path_dir_asm)
    {
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C cd \"{path_dir_asm}\" && call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\" && ml64 main.asm /link /subsystem:console /defaultlib:kernel32.lib /defaultlib:user32.lib /defaultlib:libcmt.lib && main";
        process.StartInfo = startInfo;
        process.Start();
    }
}