using System.CommandLine;
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

        compiler.CompilerEvent += Compile_UseFn;
        compiler.CompilerEvent += Compile_Struct;
        compiler.CompilerEvent += Compile_GlobalVar;
        compiler.CompilerEvent += Compile_Func;
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
        string libDir = Path.GetFullPath("./lib");
        Console.WriteLine("tranform assembly to binary file");
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = !assembler_warning;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C cd \"{path_dir_asm}\" && call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\" && ml64 main.asm /link /subsystem:console /defaultlib:kernel32.lib /defaultlib:{libDir}\\TupiLib.lib";
        if (run)
        {
            startInfo.Arguments += " && main";
        }
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        Console.WriteLine("compile finished!!");
    }

    #region PreCompile
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

    #region Compile
    static void Compile_UseFn(object? sender, CompilerArgs e)
    {
        bool isInsideFunc = false, isInsideStruct = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref isInsideFunc);
            UpdateInsideStruct(terms, ref isInsideStruct);
            if (terms.Length < 2 || isInsideFunc || isInsideStruct) continue;
            if (terms[0] == "usefn")
            {
                e.CodeData.UseFn.Add($"extern {terms[1]}: proc");
            }
        }
    }

    static void Compile_Struct(object? sender, CompilerArgs e)
    {
        StructData? currentStruct = null;
        string structCode = string.Empty;
        bool isInsideFunc = false, isInsideStruct = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref isInsideFunc);
            if (!isInsideStruct)
            {
                UpdateInsideStruct(terms, ref isInsideStruct);
                if (terms.Length < 2 || isInsideFunc) continue;
                if (terms[0] == "struct")
                {
                    currentStruct = new StructData(terms[1].Remove(terms[1].IndexOf('{')));
                    e.RunData.Structs.Add(currentStruct);
                    structCode += $"{currentStruct.Name} struct\n";
                }
            }
            else if(currentStruct is not null)
            {
                UpdateInsideStruct(terms, ref isInsideStruct);
                if (e.ReadOnlyData.TupiTypes.Contains(terms[0]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {e.ReadOnlyData.AsmTypes[pos]}") + "\n";
                    currentStruct.Vars.Add(new VarData(terms[1], terms[0], e.ReadOnlyData.TupiTypeSize[pos]));
                    currentStruct.Size += e.ReadOnlyData.TupiTypeSize[pos];
                }
                else if (e.RunData.GetStructByName(terms[0]) is StructData @struct)
                {
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentStruct.Vars.Add(new VarData(terms[1], terms[0], @struct.Size));
                    currentStruct.Size += @struct.Size;
                }
                else if(isInsideStruct == false)
                {
                    structCode += $"{currentStruct.Name} ends";
                    e.CodeData.Struct.Add(structCode);
                    structCode = string.Empty;
                }
            }
        }
    }

    static void Compile_GlobalVar(object? sender, CompilerArgs e)
    {
        bool isInsideFunc = false, isInsideStruct = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref isInsideFunc);
            UpdateInsideStruct(terms, ref isInsideStruct);
            if (terms.Length < 3 || isInsideFunc || isInsideStruct) continue;
            if (e.ReadOnlyData.TupiTypes.Contains(terms[0]))
            {
                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                e.CodeData.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {e.ReadOnlyData.AsmTypes[pos]}"));
            }
            else if (e.RunData.GetStructByName(terms[0]) is not null)
            {
                e.CodeData.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}"));
            }
        }
    }

    static void Compile_Func(object? sender, CompilerArgs e)
    {
        FuncData? currentFunc = null;
        string fnCode = string.Empty;
        bool isInsideFunc = false, isInsideStruct = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideStruct(terms, ref isInsideStruct);
            if (!isInsideFunc)
            {
                UpdateInsideFunc(terms, ref isInsideFunc);
                if (terms.Length < 2 || isInsideFunc || isInsideStruct) continue;
                if (terms[0] == "fn")
                {
                    currentFunc = new FuncData(terms[1].Remove(terms[1].IndexOf('(')));
                    e.RunData.Funcs.Add(currentFunc);
                    fnCode += $"{currentFunc.Name} proc\n";
                }
            }
            else if(currentFunc is not null)
            {
                UpdateInsideFunc(terms, ref isInsideFunc);
            }
        }
    }
    #endregion

    //#region CompilerLines
    ////static void CompilerLines_GetExternFunc(object? sender, CompilerArgs e)
    ////{
    ////    for (int w = 0; w < e.Terms.Length; w++)
    ////    {
    ////        string word = e.Terms[w];

    ////        if (w >= e.Terms.Length - 1) continue;
    ////        string next_word = e.Terms[w + 1];

    ////        if (word == "usefn")
    ////        {
    ////            e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"extern {next_word}: proc");
    ////        }
    ////    }
    ////}

    //static void CompilerLines_TupiTypeDef(object? sender, CompilerArgs e)
    //{
    //    if (e.RunData.CurrentFunc == null) return;

    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        if (!e.RunData.EndLocalVarsDefine)
    //        {
    //            bool contains = false;
    //            foreach (var types in e.ReadOnlyData.TupiTypes)
    //            {
    //                if (e.Terms.Contains(types))
    //                {
    //                    contains = true;
    //                    break;
    //                }
    //            }

    //            foreach (var structType in e.RunData.Structs.Select((StructData data) => data.Name))
    //            {
    //                if (e.Terms.Contains(structType))
    //                {
    //                    contains = true;
    //                    break;
    //                }
    //            }

    //            if (!contains)
    //            {
    //                e.RunData.EndLocalVarsDefine = true;
    //                string newLine = string.Empty;
    //                foreach (string _line in e.RunData.CurrentFunc.Args.Select((VarData var) => var.Def))
    //                {
    //                    newLine += _line + "\n";
    //                }
    //                foreach (string _line in e.RunData.CurrentFunc.LocalVars.Select((VarData var) => var.Def))
    //                {
    //                    newLine += _line + "\n";
    //                }

    //                newLine += "\tpush rdi";
    //                newLine += $"\n\tsub rsp, {CorrectShadowSpaceFunc(e.RunData.CurrentFunc.ShadowSpace)}\t;Reserve the shadow space";
    //                newLine += "\n\tmov rdi, rsp\n";
    //                e.SetLine = newLine + e.SetLine;
    //            }
    //        }
    //    }
    //}

    //static void CompilerLines_StartFunc(object? sender, CompilerArgs e)
    //{
    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        string word = e.Terms[w];

    //        if (w >= e.Terms.Length - 1) continue;
    //        string next_word = e.Terms[w + 1];

    //        if (word == "fn")
    //        {
    //            string lineCode = e.Line;

    //            int index1 = lineCode.IndexOf('(');
    //            int index2 = lineCode.IndexOf(')');
    //            string func_name = next_word.Remove(next_word.IndexOf('('));
    //            index1++;
    //            string func_arguments = lineCode[index1..index2];
    //            string[] args = func_arguments.Split(',', StringSplitOptions.RemoveEmptyEntries);
    //            e.RunData.CurrentFunc = new FuncData(func_name);
    //            e.RunData.Funcs.Add(e.RunData.CurrentFunc);
    //            string argsLine = string.Empty;
    //            for (int a = 0; a < args.Length; a++)
    //            {
    //                string arg = args[a];
    //                string[] argWords = arg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    //                string type = argWords[0];
    //                string name = argWords[1];

    //                string var_type = type;
    //                int _pos1 = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
    //                string val = e.ReadOnlyData.RegistorsAll[_pos1][a];

    //                int _pos2 = Array.IndexOf(e.ReadOnlyData.TupiTypes, type);
    //                argsLine += $"\n\tlocal {name}: {e.ReadOnlyData.AsmTypes[_pos2]}";
    //                VarData varData = new VarData(name, type, e.ReadOnlyData.TupiTypeSize[_pos2], $"\tmov {name}, {val}");
    //                e.RunData.CurrentFunc.Args.Add(varData);
    //                e.RunData.CurrentFunc.ShadowSpace = AddShadowSpaceFunc(e.RunData.CurrentFunc.ShadowSpace, e.ReadOnlyData.TupiTypeSize[_pos2]);
    //            }

    //            e.SetLine = $"{func_name} proc" + argsLine;
    //            if (!e.RunData.DotCode)
    //            {
    //                e.SetLine = ".code\n" + e.SetLine;
    //                e.RunData.DotCode = true;
    //            }

    //            if (!e.ReadOnlyData.TupiTypes.Contains(e.Lines[e.LinePos + 1].Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]))
    //            {
    //                foreach (string _line in e.RunData.CurrentFunc.Args.Select((VarData var) => var.Def))
    //                {
    //                    e.SetLine += "\n" + _line;
    //                }

    //                e.SetLine += "\n\tpush rdi";
    //                e.SetLine += $"\n\tsub rsp, {CorrectShadowSpaceFunc(e.RunData.CurrentFunc.ShadowSpace)}\t;Reserve the shadow space";
    //                e.SetLine += "\n\tmov rdi, rsp";
    //            }

    //            e.RunData.EndLocalVarsDefine = true;
    //        }
    //    }
    //}

    //static void CompilerLines_StartStruct(object? sender, CompilerArgs e)
    //{
    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        string word = e.Terms[w];

    //        if (w >= e.Terms.Length - 1) continue;
    //        string next_word = e.Terms[w + 1];

    //        if (word == "struct")
    //        {
    //            string struct_name = next_word.Remove(next_word.IndexOf('{'));
    //            e.RunData.CurrentStruct = new StructData(struct_name);
    //            e.RunData.Structs.Add(e.RunData.CurrentStruct);

    //            e.SetLine = $"{struct_name} struct";
    //        }
    //    }
    //}

    //static void CompilerLines_TupiType(object? sender, CompilerArgs e)
    //{
    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        string word = e.Terms[w];

    //        if (w >= e.Terms.Length - 1) continue;
    //        string next_word = e.Terms[w + 1];

    //        bool containWord = e.ReadOnlyData.TupiTypes.Contains(word);
    //        bool wordIsStruct = false;
    //        if (!containWord && e.RunData.Structs.Select((StructData data) => data.Name).Contains(word))
    //        {
    //            containWord = wordIsStruct = true;
    //        }

    //        if (containWord && e.RunData.CurrentFunc == null && e.RunData.CurrentStruct == null)
    //        {
    //            if (!e.RunData.DotData)
    //            {
    //                e.SetLine = ".data\n" + e.SetLine;
    //                e.RunData.DotData = true;
    //            }

    //            if (wordIsStruct && e.RunData.GetStructByName(word) is StructData structData)
    //            {
    //                e.RunData.GlobalVars.Add(next_word, new VarData(next_word, word, structData.Size));
    //                e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{next_word} {word}");
    //            }
    //            else
    //            {
    //                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
    //                e.RunData.GlobalVars.Add(next_word, new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos]));
    //                e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{next_word} {e.ReadOnlyData.DefAsmTypes[pos]}");
    //            }
    //        }
    //        else if (containWord && e.RunData.CurrentFunc != null && e.RunData.CurrentStruct == null)
    //        {
    //            VarData varData;

    //            if (w + 3 < e.Terms.Length)
    //            {
    //                if (wordIsStruct && e.RunData.GetStructByName(word) is StructData structData)
    //                {
    //                    string val = e.Terms[w + 3];
    //                    e.SetLine = $"\tlocal {next_word}: {word}";
    //                    varData = new VarData(next_word, word, structData.Size, $"\tmov {next_word}, {val}");
    //                    e.RunData.EndLocalVarsDefine = false;
    //                }
    //                else
    //                {
    //                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
    //                    string val = e.Terms[w + 3];
    //                    e.SetLine = $"\tlocal {next_word}: {e.ReadOnlyData.AsmTypes[pos]}";
    //                    varData = new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos], $"\tmov {next_word}, {val}");
    //                    e.RunData.EndLocalVarsDefine = false;
    //                }
    //            }
    //            else
    //            {
    //                if (wordIsStruct && e.RunData.GetStructByName(word) is StructData structData)
    //                {
    //                    e.SetLine = $"\tlocal {next_word}: {word}";
    //                    varData = new VarData(next_word, word, structData.Size);
    //                }
    //                else
    //                {
    //                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
    //                    e.SetLine = $"\tlocal {next_word}: {e.ReadOnlyData.AsmTypes[pos]}";
    //                    varData = new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos]);
    //                }
    //            }

    //            e.RunData.CurrentFunc.LocalVars.Add(varData);
    //            e.RunData.CurrentFunc.ShadowSpace = AddShadowSpaceFunc(e.RunData.CurrentFunc.ShadowSpace, varData.Size);
    //        }
    //        else if (containWord && e.RunData.CurrentFunc == null && e.RunData.CurrentStruct != null)
    //        {
    //            VarData varData;

    //            if (w + 3 < e.Terms.Length)
    //            {
    //                if (wordIsStruct && e.RunData.GetStructByName(word) is StructData structData)
    //                {
    //                    string val = e.Terms[w + 3];
    //                    e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{next_word} {word}");
    //                    varData = new VarData(next_word, word, structData.Size, $"\tmov {next_word}, {val}");
    //                    e.RunData.EndLocalVarsDefine = false;
    //                }
    //                else
    //                {
    //                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
    //                    string val = e.Terms[w + 3];
    //                    e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{next_word} {e.ReadOnlyData.DefAsmTypes[pos]}");
    //                    varData = new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos], $"\tmov {next_word}, {val}");
    //                    e.RunData.EndLocalVarsDefine = false;
    //                }
    //            }
    //            else
    //            {
    //                if (wordIsStruct && e.RunData.GetStructByName(word) is StructData structData)
    //                {
    //                    e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{next_word} {word}");
    //                    varData = new VarData(next_word, word, structData.Size);
    //                }
    //                else
    //                {
    //                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, word);
    //                    e.SetLine = e.SetLine.Replace($"{word} {next_word}", $"{next_word} {e.ReadOnlyData.DefAsmTypes[pos]}");
    //                    varData = new VarData(next_word, word, e.ReadOnlyData.TupiTypeSize[pos]);
    //                }
    //            }

    //            e.RunData.CurrentStruct.Vars.Add(varData);
    //            e.RunData.CurrentStruct.Size += varData.Size;
    //        }
    //    }
    //}

    //static void CompilerLines_CallFunc(object? sender, CompilerArgs e)
    //{
    //    if (e.RunData.CurrentFunc == null) return;

    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        string word = e.Terms[w];

    //        if (word.Contains('(') && w == 0)
    //        {
    //            string func_name = word.Remove(word.IndexOf('('));
    //            string _param = word.Substring(word.IndexOf('(') + 1, word.IndexOf(')') - word.IndexOf('(') - 1);
    //            string[] param = _param.Split(new char[] { ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
    //            string[] comand = new string[param.Length];
    //            string[] registors_type = new string[param.Length];

    //            for (int i = 0; i < param.Length; i++)
    //            {
    //                string var_name = param[i];
    //                if (var_name.ToCharArray()[0] == '&')
    //                {
    //                    comand[i] = "lea";
    //                    var_name = var_name.Remove(0, 1);
    //                    param[i] = var_name;
    //                    registors_type[i] = e.ReadOnlyData.RegistorsAll[3][i];
    //                }
    //                else
    //                {
    //                    comand[i] = "mov";

    //                    string this_func_name = e.RunData.CurrentFunc.Name;
    //                    if (e.RunData.CurrentFunc.LocalVars.Select((VarData var) => var.Name).Contains(var_name))
    //                    {
    //                        string var_type = string.Empty;
    //                        if (e.RunData.CurrentFunc.GetLocalVarByName(var_name) is VarData var_data)
    //                        {
    //                            var_type = var_data.Type;
    //                        }
    //                        int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
    //                        registors_type[i] = e.ReadOnlyData.RegistorsAll[pos][i];
    //                    }
    //                    else if (e.RunData.GlobalVars.ContainsKey(var_name))
    //                    {
    //                        string var_type = e.RunData.GlobalVars[var_name].Type;
    //                        int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
    //                        registors_type[i] = e.ReadOnlyData.RegistorsAll[pos][i];
    //                    }
    //                    else
    //                    {
    //                        registors_type[i] = e.ReadOnlyData.RegistorsAll[3][i];
    //                    }
    //                }
    //            }

    //            if (param.Length == 0)
    //            {
    //                e.SetLine = e.SetLine.Replace($"{word}", $"call {func_name}");
    //            }
    //            else if (param.Length == 1)
    //            {
    //                e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\tcall {func_name}");
    //            }
    //            else if (param.Length == 2)
    //            {
    //                e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\tcall {func_name}");
    //            }
    //            else if (param.Length == 3)
    //            {
    //                e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\tcall {func_name}");
    //            }
    //            else if (param.Length == 3)
    //            {
    //                e.SetLine = e.SetLine.Replace($"{word}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\t{comand[3]} {registors_type[3]}, {param[3]}\n\tcall {func_name}");
    //            }
    //            e.SetLine += "\n\txor rax, rax";
    //        }
    //    }
    //}

    //static void CompilerLines_Return(object? sender, CompilerArgs e)
    //{
    //    if (e.RunData.CurrentFunc == null) return;

    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        string word = e.Terms[w];

    //        if (word == "return" && e.Terms.Length == 1)
    //        {
    //            e.SetLine = string.Empty;
    //            e.SetLine += $"\tadd rsp, {CorrectShadowSpaceFunc(e.RunData.CurrentFunc.ShadowSpace)}\t;Remove shadow space";
    //            e.SetLine += "\n\tpop rdi";
    //            e.SetLine += "\n\tret";
    //        }
    //        else if (word == "return" && e.Terms.Length > 1)
    //        {
    //            e.SetLine = string.Empty;
    //            e.SetLine += e.SetLine.Replace($"{word} ", "mov rax, ");
    //            e.SetLine += $"\n\tadd rsp, {CorrectShadowSpaceFunc(e.RunData.CurrentFunc.ShadowSpace)}\t;Remove shadow space";
    //            e.SetLine += "\n\tpop rdi";
    //            e.SetLine += "\n\tret";
    //        }
    //    }
    //}

    //static void CompilerLines_EndFunc(object? sender, CompilerArgs e)
    //{
    //    if (e.RunData.CurrentFunc == null) return;

    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        string word = e.Terms[w];

    //        if (word == "}")
    //        {
    //            FuncData funcData = e.RunData.CurrentFunc;
    //            string func_name = funcData.Name;
    //            e.SetLine = e.SetLine.Replace($"{word}", $"{func_name} endp");
    //            e.RunData.CurrentFunc = null;
    //            break;
    //        }
    //    }
    //}

    //static void CompilerLines_EndStruct(object? sender, CompilerArgs e)
    //{
    //    if (e.RunData.CurrentStruct == null) return;

    //    for (int w = 0; w < e.Terms.Length; w++)
    //    {
    //        string word = e.Terms[w];

    //        if (word == "}")
    //        {
    //            StructData structData = e.RunData.CurrentStruct;
    //            string struct_name = structData.Name;
    //            e.SetLine = e.SetLine.Replace($"{word}", $"{struct_name} ends");
    //            e.RunData.CurrentStruct = null;
    //            break;
    //        }
    //    }
    //}
    //#endregion

    #region Private Funcs
    private static void UpdateInsideFunc(string[] terms, ref bool isInsideFunc)
    {
        if (terms.Length == 0) return;
        if(terms[0] == "fn")
        {
            isInsideFunc = true;
        }
        else if (terms[0] == "}")
        {
            isInsideFunc = false;
        }
    }

    private static void UpdateInsideStruct(string[] terms, ref bool isInsideStruct)
    {
        if (terms.Length == 0) return;
        if (terms[0] == "struct")
        {
            isInsideStruct = true;
        }
        else if (terms[0] == "}")
        {
            isInsideStruct = false;
        }
    }

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