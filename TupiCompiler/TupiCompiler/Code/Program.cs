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
        startInfo.Arguments = $"/C cd \"{path_dir_asm}\" && call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\" &&";
        startInfo.Arguments += $" ml64 main.asm /link /subsystem:console /defaultlib:{libDir}\\TupiLib.lib";
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

                if(isInsideStruct == false)
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
        bool isInsideFunc = false, isInsideStruct = false, isDefVarEnd = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideStruct(terms, ref isInsideStruct);
            if (!isInsideFunc)
            {
                UpdateInsideFunc(terms, ref isInsideFunc);
                if (terms.Length < 2 || isInsideStruct) continue;
                if (terms[0] == "fn")
                {
                    string funcArguments = line[(line.IndexOf('(')+1)..line.IndexOf(')')];
                    string[] args = funcArguments.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    currentFunc = new FuncData(terms[1].Remove(terms[1].IndexOf('(')));
                    e.RunData.Funcs.Add(currentFunc);
                    fnCode += $"{currentFunc.Name} proc\n";
                    //args
                    for (int a = 0; a < args.Length; a++)
                    {
                        string arg = args[a];
                        string[] argWords = arg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (argWords.Length < 2) continue;
                        string type = argWords[0];
                        string name = argWords[1];

                        int _pos1 = Array.IndexOf(e.ReadOnlyData.TupiTypes, type);
                        string val = e.ReadOnlyData.RegistorsAll[_pos1][a];

                        int _pos2 = Array.IndexOf(e.ReadOnlyData.TupiTypes, type);
                        fnCode += $"\tlocal {name}: {e.ReadOnlyData.AsmTypes[_pos2]}\n";
                        VarData varData = new VarData(name, type, e.ReadOnlyData.TupiTypeSize[_pos2], $"\tmov {name}, {val}\n");
                        currentFunc.Args.Add(varData);
                        currentFunc.ShadowSpace = AddShadowSpaceFunc(currentFunc.ShadowSpace, e.ReadOnlyData.TupiTypeSize[_pos2]);
                    }
                }
            }
            else if(currentFunc is not null)
            {
                UpdateInsideFunc(terms, ref isInsideFunc);

                bool contains = false;
                if (!isDefVarEnd)
                {
                    foreach (var types in e.ReadOnlyData.TupiTypes)
                    {
                        if (terms.Contains(types))
                        {
                            contains = true;
                            break;
                        }
                    }
                    foreach (var structType in e.RunData.Structs.Select((StructData data) => data.Name))
                    {
                        if (terms.Contains(structType))
                        {
                            contains = true;
                            break;
                        }
                    }
                }

                //local vars
                if (contains && !isDefVarEnd)
                {
                    VarData varData;

                    if (terms.Length > 3)
                    {
                        if (e.RunData.GetStructByName(terms[0]) is StructData structData)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}";
                            varData = new VarData(terms[1], terms[0], structData.Size, $"\tmov {terms[1]}, {terms[3]}\n");
                        }
                        else
                        {
                            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                            fnCode += $"\tlocal {terms[1]}: {e.ReadOnlyData.AsmTypes[pos]}\n";
                            varData = new VarData(terms[1], terms[0], e.ReadOnlyData.TupiTypeSize[pos], $"\tmov {terms[1]}, {terms[3]}\n");
                        }
                    }
                    else
                    {
                        if (e.RunData.GetStructByName(terms[0]) is StructData structData)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new VarData(terms[1], terms[0], structData.Size);
                        }
                        else
                        {
                            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                            fnCode += $"\tlocal {terms[1]}: {e.ReadOnlyData.AsmTypes[pos]}\n";
                            varData = new VarData(terms[1], terms[0], e.ReadOnlyData.TupiTypeSize[pos]);
                        }
                    }

                    currentFunc.LocalVars.Add(varData);
                    currentFunc.ShadowSpace = AddShadowSpaceFunc(currentFunc.ShadowSpace, varData.Size);
                }
                //def vars(local and args)
                else if(!isDefVarEnd)
                {
                    isDefVarEnd = true;
                    foreach (string _line in currentFunc.Args.Select((VarData var) => var.Def))
                    {
                        if(_line != string.Empty)
                            fnCode += _line;
                    }
                    foreach (string _line in currentFunc.LocalVars.Select((VarData var) => var.Def))
                    {
                        if (_line != string.Empty)
                            fnCode += _line;
                    }

                    fnCode += "\tpush rdi\n";
                    fnCode += $"\tsub rsp, {CorrectShadowSpaceFunc(currentFunc.ShadowSpace)}\t;Reserve the shadow space\n";
                    fnCode += "\tmov rdi, rsp\n";
                }

                // call funcs
                if (terms[0].Contains('('))
                {
                    string func_name = terms[0].Remove(terms[0].IndexOf('('));
                    string _param = terms[0].Substring(terms[0].IndexOf('(') + 1, terms[0].IndexOf(')') - terms[0].IndexOf('(') - 1);
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

                            string this_func_name = currentFunc.Name;
                            if (currentFunc.LocalVars.Select((VarData var) => var.Name).Contains(var_name))
                            {
                                string var_type = string.Empty;
                                if (currentFunc.GetLocalVarByName(var_name) is VarData var_data)
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
                        fnCode += line.Replace($"{terms[0]}", $"call {func_name}") + "\n";
                    }
                    else if (param.Length == 1)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\tcall {func_name}") + "\n";
                    }
                    else if (param.Length == 2)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\tcall {func_name}") + "\n";
                    }
                    else if (param.Length == 3)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\tcall {func_name}") + "\n";
                    }
                    else if (param.Length == 3)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"{comand[0]} {registors_type[0]}, {param[0]}\n\t{comand[1]} {registors_type[1]}, {param[1]}\n\t{comand[2]} {registors_type[2]}, {param[2]}\n\t{comand[3]} {registors_type[3]}, {param[3]}\n\tcall {func_name}") + "\n";
                    }
                    fnCode += "\txor rax, rax\n";
                }

                //return
                if (terms[0] == "return" && terms.Length == 1)
                {
                    fnCode += $"\tadd rsp, {CorrectShadowSpaceFunc(currentFunc.ShadowSpace)}\t;Remove shadow space\n";
                    fnCode += "\tpop rdi\n";
                    fnCode += "\tret\n";
                }
                else if (terms[0] == "return" && terms.Length > 1)
                {
                    fnCode += (line.Replace($"{terms[0]} ", "mov rax, ")+"\n").Replace("\r","");
                    fnCode += $"\tadd rsp, {CorrectShadowSpaceFunc(currentFunc.ShadowSpace)}\t;Remove shadow space\n";
                    fnCode += "\tpop rdi\n";
                    fnCode += "\tret\n";
                }

                //end
                if (!isInsideFunc)
                {
                    fnCode += $"{currentFunc.Name} endp";
                    e.CodeData.Func.Add(fnCode);
                    fnCode = string.Empty;
                    isInsideFunc = isInsideStruct = isDefVarEnd = false;
                    currentFunc = null;
                }
            }
        }
    }
    #endregion

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