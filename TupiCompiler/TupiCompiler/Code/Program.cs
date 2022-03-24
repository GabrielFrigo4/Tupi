﻿using System.CommandLine;
using System.Diagnostics;
using TupiCompiler.Data;

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
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if(line == "\r")
            {
                line = string.Empty;
            }
            lines[i] = line;
        }

        e.Code = string.Empty;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line != string.Empty)
            {
                e.Code += line + "\n";
            }
        }
    }

    static void PreCompileLines_Comment(object? sender, PreCompilerArgs e)
    {
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.Contains("//"))
            {
                string comment = line.Remove(0, line.IndexOf("//"));
                line = line.Remove(line.IndexOf("//"));
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
            else if (i + 1 < line.Length)
            {
                lines[i + 1] = lines[i + 1].Replace("\r", "");
            }
        }
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
            else if (i + 1 < line.Length) 
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

                        string this_func_name = e.RunData.Funcs.Last().Name;
                        if (e.RunData.Funcs.Last().LocalVar.Select((VarData var) => var.Name).Contains(var_name))
                        {
                            string var_type = string.Empty;
                            if(e.RunData.Funcs.Last().GetDataByName(var_name) is VarData var_data)
                            {
                                var_type = var_data.Type;
                            }
                            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
                            registors_type[i] = e.ReadOnlyData.RegistorsAll[pos][i];
                        }
                        else if (e.RunData.GlobalVars.ContainsKey(var_name))
                        {
                            string var_type = e.RunData.GlobalVars[var_name].Type;
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
                    e.SetLine = e.SetLine.Replace($"{word}", $"call {func_name}");
                }
                else if (param.Length == 1)
                {
                    e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\tcall {func_name}");
                }
                else if (param.Length == 2)
                {
                    e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\tcall {func_name}");
                }
                else if (param.Length == 3)
                {
                    e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\tcall {func_name}");
                }
                else if (param.Length == 3)
                {
                    e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\t{comand[3]} {registors_type[3]}, {param[3]}\n\tcall {func_name}");
                }
                e.SetLine += "\n\txor rax, rax";
            }
        }
    }

    static void CompilerLines_Return(object? sender, CompilerArgs e)
    {
        if (e.RunData.Funcs.Count == 0) return;

        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

            if (word == "return" && e.Terms.Length == 1)
            {
                e.SetLine = $"\tadd rsp, {CorrectShadowSpaceFunc(e.RunData.Funcs.Last().ShadowSpace)}\t;Remove shadow space";
                e.SetLine += "\n\tpop rdi";
                e.SetLine += "\n\tret";
            }
            else if (word == "return" && e.Terms.Length > 1)
            {
                e.SetLine = e.SetLine.Replace($"{word} ", "mov rax, ");
                e.SetLine += $"\n\tadd rsp, {CorrectShadowSpaceFunc(e.RunData.Funcs.Last().ShadowSpace)}\t;Remove shadow space";
                e.SetLine += "\n\tpop rdi";
                e.SetLine += "\n\tret";
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
                FuncData func = e.RunData.Funcs.Last();
                string func_name = func.Name;
                e.SetLine = e.SetLine.Replace($"{word}", $"{func_name} endp");
                e.RunData.Funcs.Remove(func);
            }
        }
    }

    static void CompilerLines_TupiTypeDef(object? sender, CompilerArgs e)
    {
        if (e.RunData.Funcs.Count == 0) return;

        for (int w = 0; w < e.Terms.Length; w++)
        {
            if (!e.RunData.EndLocalVarsDefine)
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
                    foreach (string _line in e.RunData.Funcs.Last().Args.Select((VarData var) => var.Def))
                    {
                        newLine += _line + "\n";
                    }
                    foreach (string _line in e.RunData.Funcs.Last().LocalVar.Select((VarData var) => var.Def))
                    {
                        newLine += _line + "\n";
                    }

                    newLine += "\tpush rdi";
                    newLine += $"\n\tsub rsp, {CorrectShadowSpaceFunc(e.RunData.Funcs.Last().ShadowSpace)}\t;Reserve the shadow space";
                    newLine += "\n\tmov rdi, rsp\n";
                    e.SetLine = newLine + e.SetLine;
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
                e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"extern {next_word}: proc");
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
                    e.SetLine = ".data\n" + e.SetLine;
                    e.RunData.DotData = true;
                }
                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
                e.RunData.GlobalVars.Add(next_word, new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos]));

                e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{next_word} {e.ReadOnlyData.DefAsmTypes[pos]}");
            }
            else if (e.ReadOnlyData.TupiTypes.Contains(word) && e.RunData.Funcs.Count > 0)
            {
                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
                VarData varData;

                if (w + 3 < e.Terms.Length)
                {
                    string val = e.Terms[w + 3];
                    e.SetLine = $"\tlocal {next_word}: {e.ReadOnlyData.AsmTypes[pos]}";
                    varData = new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos], $"\tmov {next_word}, {val}");
                    e.RunData.EndLocalVarsDefine = false;
                }
                else
                {
                    e.SetLine = $"\tlocal {next_word}: {e.ReadOnlyData.AsmTypes[pos]}";
                    varData = new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos]);
                }

                e.RunData.Funcs.Last().LocalVar.Add(varData);
                e.RunData.Funcs.Last().ShadowSpace = AddShadowSpaceFunc(e.RunData.Funcs.Last().ShadowSpace, e.ReadOnlyData.TupiTypeSize[pos]);
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
                string lineCode = e.Line;

                int index1 = lineCode.IndexOf('(');
                int index2 = lineCode.IndexOf(')');
                string func_name = next_word.Remove(next_word.IndexOf('('));
                index1++;
                string func_arguments = lineCode[index1..index2];
                string[] args = func_arguments.Split(',', StringSplitOptions.RemoveEmptyEntries);
                FuncData funcData = new FuncData(func_name);
                e.RunData.Funcs.Add(funcData);
                string argsLine = string.Empty;
                for (int a = 0; a < args.Length; a++)
                {
                    string arg = args[a];
                    string[] argWords = arg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string type = argWords[0];
                    string name = argWords[1];

                    string var_type = type;
                    int _pos1 = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
                    string val = e.ReadOnlyData.RegistorsAll[_pos1][a];

                    int _pos2 = Array.IndexOf(e.ReadOnlyData.TupiTypes, type);
                    argsLine += $"\n\tlocal {name}: {e.ReadOnlyData.AsmTypes[_pos2]}";
                    VarData varData = new VarData(name, type, e.ReadOnlyData.TupiTypeSize[_pos2], $"\tmov {name}, {val}");
                    funcData.Args.Add(varData);
                    e.RunData.Funcs.Last().ShadowSpace = AddShadowSpaceFunc(e.RunData.Funcs.Last().ShadowSpace, e.ReadOnlyData.TupiTypeSize[_pos2]);
                }

                e.SetLine = $"{func_name} proc" + argsLine;
                if (!e.RunData.DotCode)
                {
                    e.SetLine = ".code\n" + e.SetLine;
                    e.RunData.DotCode = true;
                }

                if (!e.ReadOnlyData.TupiTypes.Contains(e.Lines[e.LinePos + 1].Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]))
                {
                    foreach (string _line in e.RunData.Funcs.Last().Args.Select((VarData var) => var.Def))
                    {
                        e.SetLine += "\n" + _line;
                    }

                    e.SetLine += "\n\tpush rdi";
                    e.SetLine += $"\n\tsub rsp, {CorrectShadowSpaceFunc(e.RunData.Funcs.Last().ShadowSpace)}\t;Reserve the shadow space";
                    e.SetLine += "\n\tmov rdi, rsp";
                }

                e.RunData.EndLocalVarsDefine = true;
            }
        }
    }

    static void CompilerLines_StartStruct(object? sender, CompilerArgs e)
    {
        for (int w = 0; w < e.Terms.Length; w++)
        {
            string word = e.Terms[w];

            if (w >= e.Terms.Length - 1) continue;
            string next_word = e.Terms[w + 1];

            if (word == "struct")
            {
                string lineCode = e.Line;
                if (!e.RunData.DotCode)
                {
                    e.SetLine = ".code\n" + e.SetLine;
                    e.RunData.DotCode = true;
                }

                string struct_name = next_word.Remove(next_word.IndexOf('('));
                e.RunData.Structs.Add(new StructData(struct_name));
                //e.RunData.FuncVars.Add(struct_name, new Dictionary<string, VarData>());

                //e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{func_name} proc");
                e.SetLine = $"{struct_name} struct";
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

    private static int CorrectShadowSpaceFunc(int shadowSpace)
    {
        if (shadowSpace == 32) return 32;

        int result = shadowSpace;
        int rest = result % 8;
        if (rest == 0) return result + 8;

        result += 16 - rest;
        return result;
    }

    private static int AddShadowSpaceFunc(int shadowSpace, int varSize)
    {
        int result = shadowSpace;
        double firsDiv = Math.Floor(shadowSpace / 8f);
        double secondDiv = Math.Floor((shadowSpace + varSize) / 8f);
        if (firsDiv == secondDiv) return result + varSize;

        int rest = shadowSpace % 8;
        if (rest == 0) return result + varSize;

        result += 8 - rest;
        result += varSize;
        return result;
    }
    #endregion
}