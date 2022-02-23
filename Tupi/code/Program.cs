using System;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Diagnostics;

namespace Tupi;
public class Program
{
    static int Main(string[] args)
    {
        Action<string> action = CompileTupi;
        Argument<string> source = new Argument<string>("source", "source for tupi compile");
        RootCommand cmd = new RootCommand()
        {
            source,
        };
        cmd.SetHandler(action, source);
        return cmd.Invoke(args);
    }

    static void CompileTupi(string path_tupi)
    {
        Console.WriteLine("compile tupi code:");
        Console.WriteLine("tranform tupi to assembly");

        string tupi_code = File.ReadAllText(path_tupi);
        string[] lines = tupi_code.Split('\n');
        string asm_code = string.Empty;
        CompileData compileData = new CompileData();

        for (int l = 0; l < lines.Length; l++)
        {
            string line = lines[l];
            string[] words = line.Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int w = 0; w < words.Length; w++)
            {
                string word = words[w];

                //call func
                if (word.Contains('(') && w == 0)
                {
                    string func_name = word.Remove(word.IndexOf('('));
                    string param = word.Substring(word.IndexOf('(') + 1, word.IndexOf(')') - word.IndexOf('(') - 1);
                    if(param == string.Empty)
                    {
                        line = line.Replace($"{word}", $"call {func_name}");
                    }
                    else
                    {
                        line = line.Replace($"{word}", $"lea rcx, {param}\n\tcall {func_name}");
                    }
                }

                //return
                if (word == "return" && words.Length == 1)
                {
                    line = line.Replace($"{word}", "ret");
                }
                else if(word == "return" && words.Length > 1)
                {
                    line = line.Replace($"{word}", "mov rax, ");
                    if(compileData.funcs[compileData.funcs.Count - 1] == "main")
                    {
                        line += "\n\tadd rsp, 28h\t;Remove shadow space";
                    }
                    line += "\n\tret";
                }

                //end func
                if (word == "}")
                {
                    string func_name = compileData.funcs[compileData.funcs.Count - 1];
                    line = line.Replace($"{word}", $"{func_name} endp");
                }

                if (w >= words.Length - 1) continue;
                string next_word = words[w + 1];

                //get extern func
                if(word == "use")
                {
                    line = line.Replace($"{word} {next_word}", $"extern {next_word}: proc");
                }

                //tupi types
                string[] asm_types = { "byte", "word", "dword", "qword", "real4", "real8" };
                string[] tupi_types = { "i8", "i16", "i32", "i64", "f32", "f64" };
                if (tupi_types.Contains(word))
                {
                    if (!compileData.dotData)
                    {
                        line = ".data\n" + line;
                        compileData.dotData = true;
                    }
                    int pos = Array.IndexOf(tupi_types, word);
                    line = line.Replace($"{word} {next_word}", $"{next_word} {asm_types[pos]}");
                }

                //start func
                if (word == "func")
                {
                    if (!compileData.dotCode)
                    {
                        line = ".code\n" + line;
                        compileData.dotCode = true;
                    }

                    string func_name = next_word.Remove(next_word.IndexOf('('));
                    compileData.funcs.Add(func_name);

                    line = line.Replace($"{word} {next_word}", $"{func_name} proc");
                    if(func_name == "main")
                    {
                        line += "\n\tsub rsp, 28h\t;Reserve the shadow space";
                    }
                }
            }

            if (compileData.dotCode && l == lines.Length - 1)
            {
                line += "\nEnd";
            }

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

    static void CompileAsm(string path_dir_asm, bool run = false, bool assembler_warning = true)
    {
        Console.WriteLine("tranform assembly to binary file");
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = !assembler_warning;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C cd \"{path_dir_asm}\" && call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\" && ml64 main.asm /link /subsystem:console /defaultlib:kernel32.lib /defaultlib:user32.lib /defaultlib:libcmt.lib";
        if (run)
        {
            startInfo.Arguments += " && main";
        }
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        Console.WriteLine("compile finished!!");
    }
}