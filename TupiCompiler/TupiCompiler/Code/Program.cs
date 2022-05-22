using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using TupiCompiler.Data;
using TupiCompiler.Utility;

namespace TupiCompiler.Code;
internal static class Program
{
    static Compiler? compiler;
    public readonly static string
        libPath = "_tupi/x64/lib/",
        thPath = "_tupi/headers/",
        libDir = Path.GetFullPath(libPath),
        thDir = Path.GetFullPath(thPath);

    static readonly string? exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
    internal static string EXE_PATH
    {
        get
        {
            if(exePath == null)
            {
                return string.Empty;
            }
            else
            {
                return exePath;
            }
        }
    }

    static int Main(string[] args)
    {
        WinUtils.AddEnvironmentPath(libPath);

#if DEBUG
        args = new string[1];
        args[0] = "TupiCode/mycode.tp";
#endif

        Action<string> action = CompileTupi;
        Argument<string> source = new("source", "source for tupi compile");
        RootCommand cmd = new()
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
        compiler.PreCompilerEvent += PreCompileLines_GrammarSub;
        compiler.PreCompilerEvent += PreCompileLines_GrammarAdd;
        compiler.PreCompilerEvent += PreCompileLines_Comment;
        compiler.PreCompilerEvent += PreCompileLines_String;
        compiler.PreCompilerEvent += PreCompileLines_Header;
        compiler.PreCompilerEvent += PreCompileLines_Macro;
        compiler.PreCompilerEvent += PreCompileLines_Empty;

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
        Console.WriteLine("tranform assembly to binary file");
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            CreateNoWindow = !assembler_warning,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/C cd \"{path_dir_asm}\" && call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvars64.bat\" &&"
        };
        startInfo.Arguments += $" ml64 main.asm /link /subsystem:console /defaultlib:{libDir}TupiLib.lib";
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
    static void PreCompileLines_GrammarSub(object? sender, PreCompilerArgs e)
    {
        int totalEdits = 0;
        string codeStr = e.Code;
        char[] codeChars = e.Code.ToCharArray();
        for (int pos = 0; pos < e.Code.Length - 1; pos++)
        {
            if (IsInsideString(e.Code, pos, out _, out _)) continue;
            if (codeChars[pos] == '\t')
            {
                int newPos = pos - totalEdits;
                codeStr = codeStr.Remove(newPos, 1);
                totalEdits++;
                continue;
            }
            if (codeChars[pos] == '\r')
            {
                int newPos = pos - totalEdits;
                codeStr = codeStr.Remove(newPos, 1);
                totalEdits++;
                continue;
            }

            if (pos == e.Code.Length - 1) break;
            if (codeChars[pos] == ' ' && codeChars[pos + 1] == ' ')
            {
                int newPos = pos - totalEdits;
                codeStr = codeStr.Remove(newPos, 1);
                totalEdits++;
                continue;
            }
            if (codeChars[pos] == '\n' && codeChars[pos + 1] == '{')
            {
                int newPos = pos - totalEdits;
                codeStr = codeStr.Remove(newPos, 1);
                totalEdits++;
                continue;
            }
            if (codeChars[pos] == ';' && (codeChars[pos + 1] == '\n' || codeChars[pos + 1] == '\r'))
            {
                int newPos = pos - totalEdits;
                codeStr = codeStr.Remove(newPos, 1);
                totalEdits++;
                continue;
            }
            if (codeChars[pos] == '\n' && (codeChars[pos + 1] == '\n' || codeChars[pos + 1] == ' '))
            {
                int newPos = pos - totalEdits;
                codeStr = codeStr.Remove(newPos, 1);
                totalEdits++;
                continue;
            }
        }
        e.Code = codeStr;
    }

    static void PreCompileLines_GrammarAdd(object? sender, PreCompilerArgs e)
    {
        int totalEdits = 0;
        string codeStr = e.Code;
        char[] codeChars = e.Code.ToCharArray();
        for (int pos = 0; pos < e.Code.Length - 1; pos++)
        {
            if (IsInsideString(e.Code, pos, out _, out _)) continue;
            if(codeChars.Length <= pos + 1) break;
            if (codeChars[pos] == '=' && codeChars[pos+1] != ' ')
            {
                int newPos = pos + totalEdits;
                codeStr = codeStr.Insert(newPos + 1, " ");
                totalEdits++;
                continue;
            }

            if (codeChars[pos] != ' ' && codeChars[pos] != '\n' && codeChars[pos + 1] == '{')
            {
                int newPos = pos + totalEdits;
                codeStr = codeStr.Insert(newPos + 1, " ");
                totalEdits++;
                continue;
            }

            if (codeChars[pos] != ' ' && codeChars[pos] != '+' &&
                codeChars[pos] != '-' && codeChars[pos + 1] == '=')
            {
                int newPos = pos + totalEdits;
                codeStr = codeStr.Insert(newPos + 1, " ");
                totalEdits++;
                continue;
            }

            if (codeChars.Length <= pos + 2) break;
            if (codeChars[pos] != ' ' && codeChars[pos + 2] == '=' && 
                (codeChars[pos + 1] == '-' || codeChars[pos + 1] == '+'))
            {
                int newPos = pos + totalEdits;
                codeStr = codeStr.Insert(newPos + 1, " ");
                totalEdits++;
                continue;
            }
        }
        e.Code = codeStr;
    }

    static void PreCompileLines_Comment(object? sender, PreCompilerArgs e)
    {
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.Contains('$'))
            {
                line = line.Remove(line.IndexOf('$'));
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

    static void PreCompileLines_String(object? sender, PreCompilerArgs e)
    {
        const byte newline = (byte)'\n';        //10
        const byte tab = (byte)'\t';            //9
        const byte backspace = (byte)'\b';      //8
        const byte backslash = (byte)'\\';      //92
        const byte nullChar = (byte)'\0';       //0
        const byte singleQuotes = (byte)'\'';   //39
        const byte doubleQuotes = (byte)'\"';   //34

        for (int pos = 0; pos < e.Code.Length - 1; pos++)
        {
            void SetString(string type, byte value)
            {
                if (e.Code[pos..(pos + 2)] == type)
                {
                    bool back = false, front = false;
                    if (e.Code[pos - 1] == '\"')
                        back = true;
                    if (e.Code[pos + 2] == '\"')
                        front = true;

                    if(back == true && front == true)
                    {
                        e.Code = e.Code.Remove(--pos, 4);
                        e.Code = e.Code.Insert(pos, $"{value}");
                    }
                    else if (back == true && front == false)
                    {
                        e.Code = e.Code.Remove(--pos, 3);
                        e.Code = e.Code.Insert(pos, $"{value}, \"");
                    }
                    else if (back == false && front == true)
                    {
                        e.Code = e.Code.Remove(pos, 3);
                        e.Code = e.Code.Insert(pos, $"\", {value}");
                    }
                    else if (back == false && front == false)
                    {
                        e.Code = e.Code.Remove(pos, 2);
                        e.Code = e.Code.Insert(pos, $"\", {value}, \"");
                    }
                }
            }

            if (!IsInsideString(e.Code, pos, out bool simple, out _) || simple) continue;

            SetString(@"\n", newline);
            SetString(@"\t", tab);
            SetString(@"\b", backspace);
            SetString(@"\\", backslash);
            SetString(@"\0", nullChar);
            SetString(@"\'", singleQuotes);
            SetString("\\\"", doubleQuotes);
        }
    }

    static void PreCompileLines_Header(object? sender, PreCompilerArgs e)
    {
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("useth "))
            {
                string path = line.Replace("useth ", "").Replace("<", "").Replace(">", "");
                lines[i] = string.Empty;
                if (File.Exists(path))
                {
                    lines[i] = File.ReadAllText(path);
                }
                else if (File.Exists(thDir + "/" + path))
                {
                    lines[i] = File.ReadAllText(thDir + "/" + path);
                }
                else
                {
                    Console.WriteLine($"{path} header not find");
                }
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

        PreCompileLines_GrammarSub(sender, e);
        PreCompileLines_GrammarAdd(sender, e);
        PreCompileLines_Comment(sender, e);
        PreCompileLines_String(sender, e);
    }

    static void PreCompileLines_Macro(object? sender, PreCompilerArgs e)
    {
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, string> macros = new();
        macros.Add("iptr", "i64");

        for(int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("#macro "))
            {
                line = line.Replace("#macro ", "");
                string macro = line.Remove(line.IndexOf(' '));
                line = line.Remove(0, line.IndexOf(' '));
                string comand = line[1..];
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

    static void PreCompileLines_Empty(object? sender, PreCompilerArgs e)
    {
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        e.Code = string.Empty;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            line = line.Replace("\t", "");
            line = line.Replace("\r", "");
            line = line.Replace(" ", "");

            if (line != string.Empty)
            {
                e.Code += lines[i] + "\n";
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
                    currentStruct = new StructData(terms[1]);
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
                    currentFunc.ShadowSpace = AddShadowSpaceFunc(currentFunc.ShadowSpace, varData.Size);
                    currentFunc.LocalVars.Add(varData);
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
                    string[] registorsType = new string[param.Length];
                    string[] varType = Array.Empty<string>();
                    if(param.Length > 4)
                        varType = new string[param.Length-4];

                    for (int i = 0; i < param.Length; i++)
                    {
                        string varName = param[i];
                        if (varName.ToCharArray()[0] == '&')
                        {
                            comand[i] = "lea";
                            varName = varName.Remove(0, 1);
                            param[i] = varName;
                            registorsType[i] = e.ReadOnlyData.RegistorsAll[3][i];
                        }
                        else
                        {
                            comand[i] = "mov";

                            string this_func_name = currentFunc.Name;
                            if (currentFunc.LocalVars.Select((VarData var) => var.Name).Contains(varName))
                            {
                                string var_type = string.Empty;
                                if (currentFunc.GetLocalVarByName(varName) is VarData var_data)
                                {
                                    var_type = var_data.Type;
                                }
                                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
                                if (i < 5)
                                {
                                    registorsType[i] = e.ReadOnlyData.RegistorsAll[pos][i];
                                }
                                else
                                {
                                    varType[i - 4] = e.ReadOnlyData.AsmTypes[pos];
                                    registorsType[i] = e.ReadOnlyData.RegistorsB[pos];
                                }
                            }
                            else if (e.RunData.GlobalVars.ContainsKey(varName))
                            {
                                string var_type = e.RunData.GlobalVars[varName].Type;
                                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, var_type);
                                if (i < 5)
                                {
                                    registorsType[i] = e.ReadOnlyData.RegistorsAll[pos][i];
                                }
                                else
                                {
                                    varType[i-4] = e.ReadOnlyData.AsmTypes[pos];
                                    registorsType[i] = e.ReadOnlyData.RegistorsB[pos];
                                }
                            }
                            else if(i < 4)
                            {
                                registorsType[i] = e.ReadOnlyData.RegistorsAll[3][i];
                            }
                            else
                            {
                                varType[i - 4] = e.ReadOnlyData.AsmTypes[3];
                                registorsType[i] = e.ReadOnlyData.RegistorsB[3];
                            }
                        }
                    }

                    if (param.Length == 0)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"call {func_name}") + "\n";
                    }
                    else if (param.Length == 1)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"\t{comand[0]} {registorsType[0]}, {param[0]}\n\tcall {func_name}") + "\n";
                    }
                    else if (param.Length == 2)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"\t{comand[0]} {registorsType[0]}, {param[0]}\n\t{comand[1]} {registorsType[1]}, {param[1]}\n\tcall {func_name}") + "\n";
                    }
                    else if (param.Length == 3)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"\t{comand[0]} {registorsType[0]}, {param[0]}\n\t{comand[1]} {registorsType[1]}, {param[1]}\n\t{comand[2]} {registorsType[2]}, {param[2]}\n\tcall {func_name}") + "\n";
                    }
                    else if (param.Length == 4)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"\t{comand[0]} {registorsType[0]}, {param[0]}\n\t{comand[1]} {registorsType[1]}, {param[1]}\n\t{comand[2]} {registorsType[2]}, {param[2]}\n\t{comand[3]} {registorsType[3]}, {param[3]}\n\tcall {func_name}") + "\n";
                    }
                    else if (param.Length > 4)
                    {
                        fnCode += line.Replace($"{terms[0]}", $"\t{comand[0]} {registorsType[0]}, {param[0]}\n\t{comand[1]} {registorsType[1]}, {param[1]}\n\t{comand[2]} {registorsType[2]}, {param[2]}\n\t{comand[3]} {registorsType[3]}, {param[3]}") + "\n";
                        for (int i = 4; i < param.Length; i++)
                        {
                            fnCode += $"\t{comand[i]} {registorsType[i]}, {param[i]}\n";
                            fnCode += $"\tmov {varType[i-4]} ptr [rsp+{i*8}], {registorsType[i]}\n";
                        }
                        fnCode += $"\tcall {func_name}\n";
                    }
                    fnCode += "\txor rax, rax\n";
                }

                //operator
                if(terms.Length == 2)
                {
                    switch (terms[1])
                    {
                        case "++":
                            fnCode += $"\tinc {terms[0]}\n";
                            break;
                        case "--":
                            fnCode += $"\tdec {terms[0]}\n";
                            break;
                    }
                }
                else if (terms.Length == 3)
                {
                    switch (terms[1])
                    {
                        case "+=":
                            fnCode += $"\tadd {terms[0]}, {terms[2]}\n";
                            break;
                        case "-=":
                            fnCode += $"\tsub {terms[0]}, {terms[2]}\n";
                            break;
                        case "=":
                            fnCode += $"\tmov {terms[0]}, {terms[2]}\n";
                            break;
                    }
                }

                //mark
                if (terms[0] == "mark" && terms.Length == 2)
                {
                    fnCode += $"{terms[1]}:\n";
                }

                //goto
                if (terms[0] == "goto" && terms.Length == 2)
                {
                    fnCode += $"\tjmp {terms[1]}\n";
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
                    fnCode += line.Replace($"{terms[0]} ", "\tmov rax, ") + "\n";
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
    private static bool IsInsideString(string code, int pos, out bool isSimpleStr, out bool isCompleteStr)
    {
        int simpleStrCount = 0, completeStrCount = 0;
        bool isInside = false;
        isSimpleStr = isCompleteStr = false;
        if (pos < 0) goto endFunc;

        for (int i = 1; i < pos; i++)
        {
            char c = code[i];
            char _c = code[i - 1];
            if (c == '\"' && (simpleStrCount % 2 == 0 || (completeStrCount % 2 == 1 && _c == '\\')))
            {
                completeStrCount++;
            }
            if (c == '\'' && completeStrCount % 2 == 0)
            {
                simpleStrCount++;
            }
        }
        isSimpleStr = simpleStrCount % 2 == 1;
        isCompleteStr = completeStrCount % 2 == 1;
        isInside = isSimpleStr || isCompleteStr;

    endFunc:
        return isInside;
    }

    private static bool IsInsidePath(string code, int pos)
    {
        int pathStartCount = 0, pathEndCount = 0;
        bool isInside = false;
        if (pos < 0) goto endFunc;

        for (int i = 1; i < pos; i++)
        {
            if (IsInsideString(code, pos, out _, out _)) continue;

            char c = code[i];
            if (c == '<' && pathStartCount == pathEndCount)
            {
                pathStartCount++;
            }
            if (c == '>' && pathStartCount == pathEndCount + 1)
            {
                pathEndCount++;
            }
        }
        isInside = pathStartCount == pathEndCount + 1;

    endFunc:
        return isInside;
    }

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

        int rest = shadowSpace % 8;
        if (rest == 0) return shadowSpace + 8;
        else return shadowSpace + 16 - rest;
    }

    private static int AddShadowSpaceFunc(int shadowSpace, int varSize)
    {
        int alpha = (int)Math.Ceiling(varSize / 8f)*8;
        if (Math.Floor(shadowSpace / 8f) ==
            Math.Floor((shadowSpace + varSize) / 8f))
            return shadowSpace + varSize;

        if (alpha != 8)
        {
            shadowSpace += 8;
        }
        int rest = shadowSpace % alpha;
        if (rest == 0) return shadowSpace + varSize;
        shadowSpace += alpha - rest;
        shadowSpace += varSize;
        if (alpha != 8)
        {
            shadowSpace -= 8;
        }
        return shadowSpace;
    }
    #endregion
}