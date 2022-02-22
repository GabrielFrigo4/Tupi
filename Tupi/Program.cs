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
        CompileData compileData = new CompileData();

        for (int l = 0; l < lines.Length; l++)
        {
            string line = lines[l];
            string[] words = line.Split(new char[] { '\t', '\n', ' ', '?', '!', '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

            for (int w = 0; w < words.Length; w++)
            {
                string word = words[w];
                if (w >= words.Length - 1) break;

                //get extern func
                if(word == "extern")
                {
                    string next_word = words[w+1];
                    line = line.Replace($"{word} {next_word}", $"{word} {next_word}: proc");
                }

                //tupi types
                string[] asm_types = { "byte", "word", "dword", "qword", "real4", "real8" };
                string[] tupi_types = { "int8", "int16", "int32", "int64", "real32", "real64" };
                if (tupi_types.Contains(word))
                {
                    if (!compileData.DotData)
                    {
                        line = ".data\n" + line;
                        compileData.DotData = true;
                    }
                    int pos = Array.IndexOf(tupi_types, word);
                    string next_word = words[w + 1];
                    line = line.Replace($"{word} {next_word}", $"{next_word} {asm_types[pos]}");
                }

                //tupi func
                if (word == "func")
                {
                    line = line.Replace("func main(){", ".code\nmain proc\n\tsub rsp, 28h\t;Reserve the shadow space");
                }
            }

            //errado
            line = line.Replace("printf(ms)", "lea rcx, ms\n\tcall printf");
            //certo
            line = line.Replace("return ", "mov rax, ");
            //certo
            line = line.Replace("}", "\tadd rsp, 28h\t;Remove shadow space\n\tret\nmain endp\nEnd");

            lines[l] = line + '\n';
        }

        foreach (string line in lines)
        {
            asm_code += line;
        }

        string path_dir = "./build";
        Directory.CreateDirectory(path_dir);
        StreamWriter write = File.CreateText(path_dir + @"\main.asm");
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