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
        string[] palavras = tupi_code.Split(new char[] { '\t','\n',' ', '?', '!', '.', ',', ';' });
        string asm_code = tupi_code;

        //certo
        asm_code = asm_code.Replace("extern printf", "extern printf: proc");
        //certo
        asm_code = asm_code.Replace("func main(){", ".code\nmain proc\n\tsub rsp, 28h\t;Reserve the shadow space");
        //errado
        asm_code = asm_code.Replace("printf(\"ola mundo\")", "lea rcx, ms\n\tcall printf");
        //errado
        List<string> lines = new List<string>(asm_code.Split(new char[] { '\n' }));
        lines.Insert(1, "\n.data\nms db \"ola mundo\", 0");
        asm_code = "";
        foreach(string line in lines)
        {
            asm_code += line+'\n';
        }
        //certo
        asm_code = asm_code.Replace("return ", "mov rax, ");
        //certo
        asm_code = asm_code.Replace("}", "\tadd rsp, 28h\t;Remove shadow space\n\tret\nmain endp\nEnd");

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