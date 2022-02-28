using System.CommandLine;
using System.Diagnostics;
using Tupi.Compiler;
using Tupi.Data;

namespace Tupi;

internal class Program
{
    static readonly ReadonlyData readonlyData = new ReadonlyData();

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
        RunData runData = new RunData();
        runData.vars.Add(string.Empty, new Dictionary<string, string>());

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
                    string _param = word.Substring(word.IndexOf('(') + 1, word.IndexOf(')') - word.IndexOf('(') - 1);
                    string[] param = _param.Split(new char[] { ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] comand = new string[param.Length];
                    string[] registors_type = new string[param.Length];

                    for (int i = 0; i < param.Length; i++)
                    {
                        string var_name = param[i];
                        if (var_name.ToCharArray()[0] == '&')
                        {
                            comand[i] = "lea";
                            var_name = var_name.Remove(0, 1);
                            param[i] = var_name;
                            registors_type[i] = readonlyData.RegistorsAll[3][i];
                        }
                        else
                        {
                            comand[i] = "mov";

                            string this_func_name = runData.funcs.Last();
                            if (runData.vars[this_func_name].ContainsKey(var_name))
                            {
                                string var_type = runData.vars[this_func_name][var_name];
                                int pos = Array.IndexOf(readonlyData.TupiTypes, var_type);
                                registors_type[i] = readonlyData.RegistorsAll[pos][i];
                            }
                            else if (runData.vars[string.Empty].ContainsKey(var_name))
                            {
                                string var_type = runData.vars[string.Empty][var_name];
                                int pos = Array.IndexOf(readonlyData.TupiTypes, var_type);
                                registors_type[i] = readonlyData.RegistorsAll[pos][i];
                            }
                            else
                            {
                                registors_type[i] = readonlyData.RegistorsAll[3][i];
                            }
                        }
                    }

                    if (param.Length == 0)
                    {
                        line = line.Replace($"{word}", $"call {func_name}");
                    }
                    else if(param.Length == 1)
                    {
                        line = line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\tcall {func_name}");
                    }
                    else if (param.Length == 2)
                    {
                        line = line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\tcall {func_name}");
                    }
                    else if (param.Length == 3)
                    {
                        line = line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\tcall {func_name}");
                    }
                    else if (param.Length == 3)
                    {
                        line = line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\t{comand[3]} {registors_type[3]}, {param[3]}\n\tcall {func_name}");
                    }
                    line += "\n\txor rax,rax";
                }

                //return
                if (word == "return" && words.Length == 1)
                {
                    line = "\tadd rsp, 28h\t;Remove shadow space\n\tret";
                }
                else if(word == "return" && words.Length > 1)
                {
                    line = line.Replace($"{word}", "mov rax, ");
                    line += "\n\tadd rsp, 28h\t;Remove shadow space";
                    line += "\n\tret";
                }

                //end func
                if (word == "}")
                {
                    string func_name = runData.funcs[runData.funcs.Count - 1];
                    line = line.Replace($"{word}", $"{func_name} endp");
                    runData.vars.Remove(func_name);
                }

                //tupi types defs
                if (runData.funcs.Count > 0 && !runData.endLocalVarsDefine)
                {
                    bool contains = false;
                    foreach (var types in readonlyData.TupiTypes)
                    {
                        if (words.Contains(types))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                    {
                        runData.endLocalVarsDefine = true;
                        string newLine = string.Empty;
                        foreach (string _line in runData.localVarsDefine)
                        {
                            newLine += _line + "\n";
                        }
                        newLine += "\tsub rsp, 28h\t;Reserve the shadow space";
                        line = newLine + line;
                    }
                }

                if (w >= words.Length - 1) continue;
                string next_word = words[w + 1];

                //get extern func
                if(word == "use")
                {
                    line = line.Replace($"{word} {next_word}", $"extern {next_word}: proc");
                }

                //tupi types
                if (readonlyData.TupiTypes.Contains(word) && runData.funcs.Count == 0)
                {
                    if (!runData.dotData)
                    {
                        line = ".data\n" + line;
                        runData.dotData = true;
                    }
                    int pos = Array.IndexOf(readonlyData.TupiTypes, word);
                    runData.vars[string.Empty].Add(next_word, word);

                    line = line.Replace($"{word} {next_word}", $"{next_word} {readonlyData.AsmTypes[pos]}");
                }
                else if (readonlyData.TupiTypes.Contains(word) && runData.funcs.Count > 0)
                {
                    int pos = Array.IndexOf(readonlyData.TupiTypes, word);
                    string func_name = runData.funcs[runData.funcs.Count - 1];
                    runData.vars[func_name].Add(next_word, word);
                    if (w + 3 < words.Length)
                    {
                        string val = words[w + 3];
                        line = $"\tlocal {next_word}: {readonlyData.AsmTypes[pos]}";
                        runData.localVarsDefine.Add($"\tmov {next_word}, {val}");
                        runData.endLocalVarsDefine = false;
                        //line += $"\n\tmov {next_word}, {val}";
                    }
                    else
                    {
                        line = $"\tlocal {next_word}: {readonlyData.AsmTypes[pos]}";
                    }

                    //if (!tupiData.tupi_types.Contains(lines[l + 1].Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]))
                    //{
                    //    line += "\n\tsub rsp, 28h\t;Reserve the shadow space";
                    //}
                }

                //start func
                if (word == "func")
                {
                    if (!runData.dotCode)
                    {
                        line = ".code\n" + line;
                        runData.dotCode = true;
                    }

                    string func_name = next_word.Remove(next_word.IndexOf('('));
                    runData.funcs.Add(func_name);
                    runData.vars.Add(func_name, new Dictionary<string, string>());

                    line = line.Replace($"{word} {next_word}", $"{func_name} proc");
                    if(!readonlyData.TupiTypes.Contains(lines[l+1].Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]))
                    {
                        line += "\n\tsub rsp, 28h\t;Reserve the shadow space";
                    }

                    runData.localVarsDefine.Clear();
                    runData.endLocalVarsDefine = true;
                }
            }

            if (runData.dotCode && l == lines.Length - 1)
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