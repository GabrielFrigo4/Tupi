using System.CommandLine;
using System.Diagnostics;

namespace TupiCompiler.Code;

internal static class Program
{
    static Compiler? compiler;

    static int Main(string[] args)
    {
        args = new string[1];
        args[0] = "mycode.tp";

        Action<string> action = CompileTupi;
        Argument<string> source = new Argument<string>("source", "source for tupi compile");
        RootCommand cmd = new RootCommand()
        {
            source,
        };
        cmd.SetHandler(action, source);
        return cmd.Invoke(args);
    }

    static void CompileTupi(string pathTupi)
    {
        Console.WriteLine("compile tupi code:");
        Console.WriteLine("tranform tupi to assembly");

        compiler = new Compiler(pathTupi);

        compiler.PreCompilerEvent += PreCompileLines_Grammar;
        compiler.PreCompilerEvent += PreCompileLines_Comment;
        compiler.PreCompilerEvent += PreCompileLines_Macro;

        compiler.CompilerEvent += CompilerLines_CallFunc;
        compiler.CompilerEvent += CompilerLines_Return;
        compiler.CompilerEvent += CompilerLines_EndFunc;
        compiler.CompilerEvent += CompilerLines_TupiTypeDef;
        compiler.CompilerEvent += CompilerLines_GetExternFunc;
        compiler.CompilerEvent += CompilerLines_TupiType;
        compiler.CompilerEvent += CompilerLines_StartFunc;

        string asmCode = compiler.Start();

        string pathDir = "./build";
        Directory.CreateDirectory(pathDir);
        StreamWriter write = File.CreateText(pathDir + @"\main.asm");
        write.Write(asmCode);
        write.Close();
        CompileAsm(pathDir);
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

    #region PreCompileLines
    static void PreCompileLines_Grammar(object? sender, PreCompilerArgs e)
    {

    }

    static void PreCompileLines_Comment(object? sender, PreCompilerArgs e)
    {

    }

    static void PreCompileLines_Macro(object? sender, PreCompilerArgs e)
    {
        Dictionary<string, string> macros = new Dictionary<string, string>();
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for(int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.Contains("#macro "))
            {
                line = line.Replace("#macro ", "");
                string macro = line.Remove(line.IndexOf('\"') - 1);
                line = line.Remove(0, line.IndexOf('\"'));
                string comand = line[1..^2];
                macros.Add(macro, comand);
                lines[i] = string.Empty;
            }
            else
            {
                foreach (string macro in macros.Keys)
                {
                    if (line.Contains(macro))
                    {
                        line = line.Replace(macro, macros[macro]);
                    }
                }
                lines[i] = line;
            }
        }

        e.Code = string.Empty;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line != string.Empty)
            {
                e.Code += line + "\n";
            }
            else
            {
                lines[i + 1] = lines[i + 1].Replace("\r", "");
            }
        }
    }
    #endregion

    #region CompilerLines
    static void CompilerLines_CallFunc(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

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
                        registors_type[i] = e.ReadOnlyData.RegistorsAll[3][i];
                    }
                    else
                    {
                        comand[i] = "mov";

                        string this_func_name = e.RunData.Funcs.Last();
                        if (e.RunData.Vars[this_func_name].ContainsKey(var_name))
                        {
                            string var_type = e.RunData.Vars[this_func_name][var_name];
                            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
                            registors_type[i] = e.ReadOnlyData.RegistorsAll[pos][i];
                        }
                        else if (e.RunData.Vars[string.Empty].ContainsKey(var_name))
                        {
                            string var_type = e.RunData.Vars[string.Empty][var_name];
                            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
                            registors_type[i] = e.ReadOnlyData.RegistorsAll[pos][i];
                        }
                        else
                        {
                            registors_type[i] = e.ReadOnlyData.RegistorsAll[3][i];
                        }
                    }
                }

                if (param.Length == 0)
                {
                    e.Line = e.Line.Replace($"{word}", $"call {func_name}");
                }
                else if (param.Length == 1)
                {
                    e.Line = e.Line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\tcall {func_name}");
                }
                else if (param.Length == 2)
                {
                    e.Line = e.Line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\tcall {func_name}");
                }
                else if (param.Length == 3)
                {
                    e.Line = e.Line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\tcall {func_name}");
                }
                else if (param.Length == 3)
                {
                    e.Line = e.Line.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\t{comand[3]} {registors_type[3]}, {param[3]}\n\tcall {func_name}");
                }
                e.Line += "\n\txor rax, rax";
            }
        }
    }

    static void CompilerLines_Return(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

            if (word == "return" && e.Terms.Length == 1)
            {
                e.Line = "\tadd rsp, 40\t;Remove shadow space\n\tret";
            }
            else if (word == "return" && e.Terms.Length > 1)
            {
                e.Line = e.Line.Replace($"{word} ", "mov rax, ");
                e.Line += "\n\tadd rsp, 28h\t;Remove shadow space";
                e.Line += "\n\tret";
            }
        }
    }

    static void CompilerLines_EndFunc(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

            if (word == "}")
            {
                string func_name = e.RunData.Funcs[e.RunData.Funcs.Count - 1];
                e.Line = e.Line.Replace($"{word}", $"{func_name} endp");
                e.RunData.Vars.Remove(func_name);
                e.RunData.Funcs.Remove(func_name);
            }
        }
    }

    static void CompilerLines_TupiTypeDef(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            if (e.RunData.Funcs.Count > 0 && !e.RunData.EndLocalVarsDefine)
            {
                bool contains = false;
                foreach (var types in e.ReadOnlyData.TupiTypes)
                {
                    if (e.Terms.Contains(types))
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    e.RunData.EndLocalVarsDefine = true;
                    string newLine = string.Empty;
                    foreach (string _line in e.RunData.LocalVarsDefine)
                    {
                        newLine += _line + "\n";
                    }
                    newLine += "\tsub rsp, 40\t;Reserve the shadow space\n";
                    e.Line = newLine + e.Line;
                }
            }
        }
    }

    static void CompilerLines_GetExternFunc(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

            if (w >= e.Terms.Length - 1) continue;
            string next_word = e.Terms[w + 1];

            if (word == "use")
            {
                e.Line = e.Line.Replace($"{word} {next_word}", $"extern {next_word}: proc");
            }
        }
    }

    static void CompilerLines_TupiType(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

            if (w >= e.Terms.Length - 1) continue;
            string next_word = e.Terms[w + 1];

            if (e.ReadOnlyData.TupiTypes.Contains(word) && e.RunData.Funcs.Count == 0)
            {
                if (!e.RunData.DotData)
                {
                    e.Line = ".data\n" + e.Line;
                    e.RunData.DotData = true;
                }
                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
                e.RunData.Vars[string.Empty].Add(next_word, word);

                e.Line = e.Line.Replace($"{word} {next_word}", $"{next_word} {e.ReadOnlyData.AsmTypes[pos]}");
            }
            else if (e.ReadOnlyData.TupiTypes.Contains(word) && e.RunData.Funcs.Count > 0)
            {
                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
                string func_name = e.RunData.Funcs[e.RunData.Funcs.Count - 1];
                e.RunData.Vars[func_name].Add(next_word, word);
                if (w + 3 < e.Terms.Length)
                {
                    string val = e.Terms[w + 3];
                    e.Line = $"\tlocal {next_word}: {e.ReadOnlyData.AsmTypes[pos]}";
                    e.RunData.LocalVarsDefine.Add($"\tmov {next_word}, {val}");
                    e.RunData.EndLocalVarsDefine = false;
                }
                else
                {
                    e.Line = $"\tlocal {next_word}: {e.ReadOnlyData.AsmTypes[pos]}";
                }
            }
        }
    }

    static void CompilerLines_StartFunc(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

            if (w >= e.Terms.Length - 1) continue;
            string next_word = e.Terms[w + 1];

            if (word == "func")
            {
                if (!e.RunData.DotCode)
                {
                    e.Line = ".code\n" + e.Line;
                    e.RunData.DotCode = true;
                }

                string func_name = next_word.Remove(next_word.IndexOf('('));
                e.RunData.Funcs.Add(func_name);
                e.RunData.Vars.Add(func_name, new Dictionary<string, string>());

                e.Line = e.Line.Replace($"{word} {next_word}", $"{func_name} proc");
                if (!e.ReadOnlyData.TupiTypes.Contains(e.Lines[e.LinePos + 1].Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]))
                {
                    e.Line += "\n\tsub rsp, 40\t;Reserve the shadow space";
                }

                e.RunData.LocalVarsDefine.Clear();
                e.RunData.EndLocalVarsDefine = true;
            }
        }
    }
    #endregion

    #region Private Funcs
    private static bool GetIfWordExist(string word, string line)
    {
        bool wordExist = false;
        return wordExist;
    }
    #endregion
}