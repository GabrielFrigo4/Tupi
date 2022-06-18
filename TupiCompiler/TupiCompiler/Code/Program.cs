using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using TupiCompiler.Data;

namespace TupiCompiler.Code;
internal static class Program
{
    static Compiler? mainCompiler;

    internal static Compiler MainCompiler
    {
        get
        {
            if (mainCompiler is null)
            {
                throw new Exception("MainCompiler not create");
            }
            else
            {
                return mainCompiler;
            }
        }
    }

    public readonly static string
        x64Path = "_tupi/x64/",
        x86Path = "_tupi/x86/",
        thPath = "_tupi/headers/",
        x64Dir = Path.GetFullPath(x64Path),
        x86Dir = Path.GetFullPath(x86Path),
        thDir = Path.GetFullPath(thPath),
        pathDir = "./build";

    static private string pathCompile = string.Empty;

    static readonly string? exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
    internal static string EXE_PATH
    {
        get
        {
            if (exePath is null)
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
#if DEBUG
        args = new string[1];
        args[0] = "TupiCode/mycode.tp";
#endif
        string? _pathCompile = Path.GetDirectoryName(args[0]);
        if (_pathCompile is not null)
            pathCompile = Path.GetFullPath(_pathCompile);

        Action<string> action = CompileTupiProj;
        Argument<string> source = new("source", "source for tupi compile");
        RootCommand cmd = new()
        {
            source,
        };
        cmd.SetHandler(action, source);
        return cmd.Invoke(args);
    }

    static void CompileTupiProj(string pathTupi)
    {
        string tupiFileName = Path.GetFileNameWithoutExtension(pathTupi);
        Console.WriteLine("compile tupi proj:");
        Console.WriteLine("tranform tupi code to assembly(masm)");

        Directory.CreateDirectory(pathDir);
        StreamWriter write = File.CreateText(pathDir + $"\\{tupiFileName}.asm");
        write.Write(CompileTupiFile(pathTupi, out mainCompiler));
        write.Close();

        List<string> files = new();
        files.Add(tupiFileName);
        if (File.Exists(pathDir + "\\header\\std_tupi_def.inc"))
            File.Delete(pathDir + "\\header\\std_tupi_def.inc");
        File.Copy($"{x64Dir}std_tupi_def.inc", pathDir + "\\header\\std_tupi_def.inc");
        CompileAsm(pathDir, files);
    }

    static string CompileTupiFile(string pathTupiCode, out Compiler compiler, bool isHeader = false)
    {
        compiler = new Compiler(pathTupiCode, isHeader);
        compiler.PreCompilerEvent += PreCompileLines_GrammarSub;
        compiler.PreCompilerEvent += PreCompileLines_GrammarAdd;
        compiler.PreCompilerEvent += PreCompileLines_Comment;
        compiler.PreCompilerEvent += PreCompileLines_String;
        compiler.PreCompilerEvent += PreCompileLines_Macro;
        compiler.PreCompilerEvent += PreCompileLines_Empty;

        compiler.CompilerEvent += Compile_UseTh;
        compiler.CompilerEvent += Compile_UseFn;
        compiler.CompilerEvent += Compile_Struct;
        compiler.CompilerEvent += Compile_Union;
        compiler.CompilerEvent += Compile_Typedef;
        compiler.CompilerEvent += Compile_GlobalVar;
        compiler.CompilerEvent += Compile_Func;
        string asmCode = compiler.Start();

        return asmCode;
    }

    static void CompileAsm(string path_dir_asm, List<string> nameFiles, bool run = false, bool assembler_warning = true)
    {
        Console.WriteLine("tranform assembly to binary file");
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            CreateNoWindow = !assembler_warning,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/C cd \"{path_dir_asm}\" && call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvarsall.bat\" x64  &&"
        };
        foreach (string asmFile in nameFiles)
        {
            startInfo.Arguments += $" ml64 {asmFile}.asm /c &&";
        }
        string linkCommand = " link";
        foreach (string objFile in nameFiles)
        {
            linkCommand += $" {objFile}.obj";
        }
        linkCommand += $" /entry:main /subsystem:console /defaultlib:{x64Dir}lib/TupiLib.lib";
        startInfo.Arguments += linkCommand;
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
        string codeStr = e.Code;
        for (int pos = 0; pos < codeStr.Length - 1; pos++)
        {
            if (IsInsideString(codeStr, pos, out _, out _)) continue;
            if (codeStr[pos] == '\t')
            {
                codeStr = codeStr.Remove(pos, 1);
                pos--;
                continue;
            }
            if (codeStr[pos] == '\r')
            {
                codeStr = codeStr.Remove(pos, 1);
                pos--;
                continue;
            }

            if (pos == e.Code.Length - 1) break;
            if (codeStr[pos] == ' ' && codeStr[pos + 1] == ' ')
            {
                codeStr = codeStr.Remove(pos, 1);
                pos--;
                continue;
            }
            if (codeStr[pos] == '\n' && codeStr[pos + 1] == '{')
            {
                codeStr = codeStr.Remove(pos, 1);
                pos--;
                continue;
            }
            if (codeStr[pos] == ';' && (codeStr[pos + 1] == '\n' || codeStr[pos + 1] == '\r'))
            {
                codeStr = codeStr.Remove(pos, 1);
                pos--;
                continue;
            }
            if (codeStr[pos] == '\n' && (codeStr[pos + 1] == '\n' || codeStr[pos + 1] == ' '))
            {
                codeStr = codeStr.Remove(pos, 1);
                pos--;
                continue;
            }

            if (pos == 0) continue;
            if (codeStr[pos - 1] == '\n' && codeStr[pos] == '{')
            {
                codeStr = codeStr.Remove(pos - 1, 1);
                pos--;
                continue;
            }
        }
        e.Code = codeStr;
    }

    static void PreCompileLines_GrammarAdd(object? sender, PreCompilerArgs e)
    {
        string codeStr = e.Code;
        for (int pos = 0; pos < codeStr.Length - 1; pos++)
        {
            if (IsInsideString(codeStr, pos, out _, out _)) continue;
            if (codeStr.Length <= pos + 1) break;
            if (codeStr[pos] == '=' && codeStr[pos + 1] != ' ')
            {
                codeStr = codeStr.Insert(pos + 1, " ");
                continue;
            }

            if (codeStr[pos] != ' ' && codeStr[pos] != '\n' && codeStr[pos + 1] == '{')
            {
                codeStr = codeStr.Insert(pos + 1, " ");
                continue;
            }

            if (codeStr[pos] != ' ' && codeStr[pos] != '+' &&
                codeStr[pos] != '-' && codeStr[pos + 1] == '=')
            {
                codeStr = codeStr.Insert(pos + 1, " ");
                continue;
            }

            if (codeStr.Length <= pos + 2) break;
            if (codeStr[pos] != ' ' && codeStr[pos + 2] == '=' &&
                (codeStr[pos + 1] == '-' || codeStr[pos + 1] == '+'))
            {
                codeStr = codeStr.Insert(pos + 1, " ");
                continue;
            }
        }
        e.Code = codeStr;
    }

    static void PreCompileLines_Comment(object? sender, PreCompilerArgs e)
    {
        string code = e.Code;

        //coment ==> ($ here $)
        int start = -1, end = -1;
        for (int i = 0; i < code.Length - 1; i++)
        {
            if (code[i] == '(' && code[i + 1] == '$')
            {
                start = i;
            }
            if (code[i] == '$' && code[i + 1] == ')' && start > 0)
            {
                end = i + 2;
                int count = end - start;
                code = code.Remove(start, count);
                i -= count;

                start = -1;
                end = -1;
            }
        }

        //coment ==> $ here
        string[] lines = code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.Contains('$'))
            {
                line = line.Remove(line.IndexOf('$'));
                lines[i] = line;
            }
        }

        code = string.Empty;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line != string.Empty)
            {
                code += line + "\n";
            }
            else if (i + 1 < line.Length)
            {
                lines[i + 1] = lines[i + 1].Replace("\r", "");
            }
        }

        e.Code = code;
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

                    if (back == true && front == true)
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

    static void PreCompileLines_Macro(object? sender, PreCompilerArgs e)
    {
        string[] lines = e.Code.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        char[] seps1 = new[] { '\t', ' ', ',', '(', '{', '[', '=', '+', '-', '/', '*' };
        char[] seps2 = new[] { ' ', ',', ')', '}', ']', '=', '+', '-', '/', '*', '\n', '\r', };

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("#macro "))
            {
                line = line.Replace("#macro ", "");
                string macro = line.Remove(line.IndexOf(' '));
                line = line.Remove(0, line.IndexOf(' '));
                string comand = line[1..];
                e.Macros.Add(macro, comand);
                lines[i] = string.Empty;
            }
            else
            {
                foreach (string macro in e.Macros.Keys)
                {
                    int ind = line.IndexOf(macro);
                    if (ind > -1 && !IsInsideString(line, ind, out _, out _) && !IsInsidePath(line, ind))
                    {
                        foreach(char sep1 in seps1)
                        {
                            foreach (char sep2 in seps2)
                            {
                                if (line.StartsWith($"{macro}{sep2}"))
                                {
                                    line = line.Remove(ind, macro.Length);
                                    line = line.Insert(ind, e.Macros[macro]);
                                    ind = line.IndexOf(macro);
                                    if (ind < 0 || IsInsideString(line, ind, out _, out _) || IsInsidePath(line, ind)) break;
                                }
                                else if (line.EndsWith($"{sep1}{macro}"))
                                {
                                    line = line.Remove(ind, macro.Length);
                                    line = line.Insert(ind, e.Macros[macro]);
                                    ind = line.IndexOf(macro);
                                    if (ind < 0 || IsInsideString(line, ind, out _, out _) || IsInsidePath(line, ind)) break;
                                }
                                else if(ind - 1 == line.IndexOf($"{sep1}{macro}{sep2}"))
                                {
                                    line = line.Remove(ind, macro.Length);
                                    line = line.Insert(ind, e.Macros[macro]);
                                    ind = line.IndexOf(macro);
                                    if (ind < 0 || IsInsideString(line, ind, out _, out _) || IsInsidePath(line, ind)) break;
                                }
                                if (ind < 0 || IsInsideString(line, ind, out _, out _) || IsInsidePath(line, ind)) break;
                            }
                            if (ind < 0 || IsInsideString(line, ind, out _, out _) || IsInsidePath(line, ind)) break;
                        }
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
    static void Compile_UseTh(object? sender, CompilerArgs e)
    {
        if (e.IsHeader)
            e.CodeCompiled.UseTh.Add("include std_tupi_def.inc");
        else
            e.CodeCompiled.UseTh.Add("include header/std_tupi_def.inc");

        for (int i = 0; i < e.Lines.Length; i++)
        {
            string line = e.Lines[i];
            if (line.StartsWith("useth "))
            {
                string path = line.Replace("useth ", "").Replace("<", "").Replace(">", "");
                if (File.Exists(path))
                {
                    string incName = CreateIncludeFile(path, out IHeaderData headerData);
                    MainCompiler.GetRunData().AddHeaderData(headerData);
                    if (e.IsHeader)
                        e.CodeCompiled.UseTh.Add($"include {incName}");
                    else
                        e.CodeCompiled.UseTh.Add($"include header/{incName}");
                }
                else if (File.Exists(pathCompile + "/" + path))
                {
                    string incName = CreateIncludeFile(pathCompile + "/" + path, out IHeaderData headerData);
                    MainCompiler.GetRunData().AddHeaderData(headerData);
                    if (e.IsHeader)
                        e.CodeCompiled.UseTh.Add($"include {incName}");
                    else
                        e.CodeCompiled.UseTh.Add($"include header/{incName}");
                }
                else if (File.Exists(thDir + path))
                {
                    string incName = CreateIncludeFile(thDir + path, out IHeaderData headerData);
                    MainCompiler.GetRunData().AddHeaderData(headerData);
                    if (e.IsHeader)
                        e.CodeCompiled.UseTh.Add($"include {incName}");
                    else
                        e.CodeCompiled.UseTh.Add($"include header/{incName}");
                }
                else
                {
                    ConsoleColor consoleColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{path} header not find");
                    Console.WriteLine($"{pathCompile + "/" + path} header not find");
                    Console.WriteLine($"{thDir + path} header not find");
                    Console.ForegroundColor = consoleColor;
                }
            }
        }
    }

    static void Compile_UseFn(object? sender, CompilerArgs e)
    {
        int totalKey = 0;
        bool isInsideFunc = false, isInsideStruct = false, isInsideUnion = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref totalKey, ref isInsideFunc);
            UpdateInsideStruct(terms, ref isInsideStruct);
            UpdateInsideStruct(terms, ref isInsideUnion);
            if (terms.Length < 2 || isInsideFunc || isInsideStruct || isInsideUnion) continue;
            if (terms[0] == "usefn")
            {
                e.CodeCompiled.UseFn.Add($"extern {terms[1]}: proc");
            }
        }
    }

    static void Compile_Struct(object? sender, CompilerArgs e)
    {
        StructData? currentStruct = null;
        string structCode = string.Empty;
        int totalKey = 0;
        bool isInsideFunc = false, isInsideStruct = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref totalKey, ref isInsideFunc);
            if (!isInsideStruct)
            {
                UpdateInsideStruct(terms, ref isInsideStruct);
                if (terms.Length < 2 || isInsideFunc) continue;
                if (terms[0] == "struct")
                {
                    currentStruct = new StructData(terms[1]);
                    e.RunData.Structs.Add(currentStruct);
                    structCode = $"{currentStruct.Name} struct\n";
                }
            }
            else if (currentStruct is not null)
            {
                UpdateInsideStruct(terms, ref isInsideStruct);
                if (e.ReadOnlyData.TupiTypes.Contains(terms[0]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentStruct.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos]));
                    currentStruct.Size += e.ReadOnlyData.TypeSize[pos];
                }
                else if (e.ReadOnlyData.AsmTypes.Contains(terms[0]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, terms[0]);
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentStruct.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos]));
                    currentStruct.Size += e.ReadOnlyData.TypeSize[pos];
                }
                else if (e.RunData.GetTypedefByName(terms[0]) is TypedefData typedef)
                {
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentStruct.Vars.Add(new(terms[1], terms[0], typedef.Size));
                    currentStruct.Size += typedef.Size;
                }
                else if (e.RunData.GetStructByName(terms[0]) is StructData @struct)
                {
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentStruct.Vars.Add(new(terms[1], terms[0], @struct.Size));
                    currentStruct.Size += @struct.Size;
                }
                else if (e.RunData.GetUnionByName(terms[0]) is UnionData union)
                {
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentStruct.Vars.Add(new(terms[1], terms[0], union.Size));
                    currentStruct.Size += union.Size;
                }

                if (!isInsideStruct)
                {
                    structCode += $"{currentStruct.Name} ends";
                    e.CodeCompiled.Struct.Add(structCode);
                    structCode = string.Empty;
                    currentStruct = null;
                }
            }
        }
    }

    static void Compile_Union(object? sender, CompilerArgs e)
    {
        UnionData? currentUnion = null;
        string unionCode = string.Empty;
        int totalKey = 0;
        bool isInsideFunc = false, isInsideUnion = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref totalKey, ref isInsideFunc);
            if (!isInsideUnion)
            {
                UpdateInsideUnion(terms, ref isInsideUnion);
                if (terms.Length < 2 || isInsideFunc) continue;
                if (terms[0] == "union")
                {
                    currentUnion = new UnionData(terms[1]);
                    e.RunData.Unions.Add(currentUnion);
                    unionCode = $"{currentUnion.Name} union\n";
                }
            }
            else if (currentUnion is not null)
            {
                UpdateInsideUnion(terms, ref isInsideUnion);
                if (e.ReadOnlyData.TupiTypes.Contains(terms[0]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos]));
                    if (currentUnion.Size < e.ReadOnlyData.TypeSize[pos])
                        currentUnion.Size = e.ReadOnlyData.TypeSize[pos];
                }
                else if (e.ReadOnlyData.AsmTypes.Contains(terms[0]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos]));
                    if (currentUnion.Size < e.ReadOnlyData.TypeSize[pos])
                        currentUnion.Size = e.ReadOnlyData.TypeSize[pos];
                }
                else if (e.RunData.GetTypedefByName(terms[0]) is TypedefData typedef)
                {
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], typedef.Size));
                    if (currentUnion.Size < typedef.Size)
                        currentUnion.Size = typedef.Size;
                }
                else if (e.RunData.GetStructByName(terms[0]) is StructData @struct)
                {
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], @struct.Size));
                    if (currentUnion.Size < @struct.Size)
                        currentUnion.Size = @struct.Size;
                }
                else if (e.RunData.GetUnionByName(terms[0]) is UnionData union)
                {
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], union.Size));
                    if (currentUnion.Size < union.Size)
                        currentUnion.Size = union.Size;
                }

                if (!isInsideUnion)
                {
                    unionCode += $"{currentUnion.Name} ends";
                    e.CodeCompiled.Union.Add(unionCode);
                    unionCode = string.Empty;
                }
            }
        }
    }

    static void Compile_Typedef(object? sender, CompilerArgs e)
    {
        int totalKey = 0;
        bool isInsideFunc = false, isInsideStruct = false, isInsideUnion = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref totalKey, ref isInsideFunc);
            UpdateInsideStruct(terms, ref isInsideStruct);
            UpdateInsideStruct(terms, ref isInsideUnion);
            if (terms.Length < 2 || isInsideFunc || isInsideStruct || isInsideUnion) continue;

            if (terms[0] == "typedef")
            {
                if (e.RunData.GetTypedefByName(terms[2]) is not null) continue;
                e.CodeCompiled.Typedef.Add($"{terms[2]} typedef {terms[1]}");

                if (e.ReadOnlyData.TupiTypes.Contains(terms[1]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[1]);
                    e.RunData.Typedef.Add(new(terms[2], e.ReadOnlyData.TypeSize[pos]));
                }
                else if (e.ReadOnlyData.AsmTypes.Contains(terms[1]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, terms[1]);
                    e.RunData.Typedef.Add(new(terms[2], e.ReadOnlyData.TypeSize[pos]));
                }
                else if (e.RunData.GetTypedefByName(terms[1]) is TypedefData typedef)
                {
                    e.RunData.Typedef.Add(new(terms[2], typedef.Size));
                }
                else if (e.RunData.GetStructByName(terms[1]) is StructData @struct)
                {
                    e.RunData.Typedef.Add(new(terms[2], @struct.Size));
                }
                else if (e.RunData.GetUnionByName(terms[1]) is UnionData union)
                {
                    e.RunData.Typedef.Add(new(terms[2], union.Size));
                }
            }
        }
    }

    static void Compile_GlobalVar(object? sender, CompilerArgs e)
    {
        int totalKey = 0;
        bool isInsideFunc = false, isInsideStruct = false, isInsideUnion = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideFunc(terms, ref totalKey, ref isInsideFunc);
            UpdateInsideStruct(terms, ref isInsideStruct);
            UpdateInsideUnion(terms, ref isInsideUnion);
            if (terms.Length < 3 || isInsideFunc || isInsideStruct || isInsideUnion) continue;
            if (e.ReadOnlyData.TupiTypes.Contains(terms[0]))
            {
                e.CodeCompiled.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}"));
            }
            else if (e.ReadOnlyData.AsmTypes.Contains(terms[0]))
            {
                e.CodeCompiled.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}"));
            }
            else if (e.RunData.GetStructByName(terms[0]) is not null)
            {
                e.CodeCompiled.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}"));
            }
            else if (e.RunData.GetTypedefByName(terms[0]) is not null)
            {
                e.CodeCompiled.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}"));
            }
            else if (e.RunData.GetStructByName(terms[0]) is not null)
            {
                e.CodeCompiled.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}"));
            }
            else if (e.RunData.GetUnionByName(terms[0]) is not null)
            {
                e.CodeCompiled.GlobalVar.Add(line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}"));
            }
        }
    }

    static void Compile_Func(object? sender, CompilerArgs e)
    {
        if (e.IsHeader) return;

        FuncData? currentFunc = null;
        string fnCode = string.Empty;

        int keysInd = 0;
        int totalKeys = 0;
        List<Tuple<int, string>> keysData = new();

        bool isInsideFunc = false, isInsideStruct = false, isInsideUnion = false, isDefVarEnd = false;
        foreach (var line in e.Lines)
        {
            string[] terms = e.GetTermsLine(line);
            UpdateInsideStruct(terms, ref isInsideStruct);
            UpdateInsideUnion(terms, ref isInsideUnion);
            if (!isInsideFunc)
            {
                UpdateInsideFunc(terms, ref totalKeys, ref isInsideFunc);
                if (terms.Length < 2 || isInsideStruct || isInsideUnion) continue;
                if (terms[0] == "fn")
                {
                    string funcArguments = line[(line.IndexOf('(') + 1)..line.IndexOf(')')];
                    string[] args = funcArguments.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    currentFunc = new(terms[1].Remove(terms[1].IndexOf('(')));
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

                        int _pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, type);
                        string val = string.Empty;
                        if ( _pos > 0 && _pos < 4 )
                            val = e.ReadOnlyData.RegistorsAll[_pos][a];
                        else
                            val = e.ReadOnlyData.RegistorsAll[3][a];

                        fnCode += $"\tlocal {name}: {type}\n";
                        VarData varData = new(name, type, e.ReadOnlyData.TypeSize[_pos], $"\tmov {name}, {val}\n");
                        currentFunc.Args.Add(varData);
                        currentFunc.ShadowSpace = AddShadowSpaceFunc(currentFunc.ShadowSpace, varData.Size);
                    }
                }
            }
            else if (currentFunc is not null)
            {
                UpdateInsideFunc(terms, ref totalKeys, ref isInsideFunc);

                bool contains = false;
                if (!isDefVarEnd)
                {
                    contains = ContainsDefVar(terms, e);
                }

                //local vars
                if (contains && !isDefVarEnd)
                {
                    VarData? varData = null;
                    if (terms.Length > 3)
                    {
                        if (e.ReadOnlyData.TupiTypes.Contains(terms[0]))
                        {
                            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos], $"\tmov {terms[1]}, {terms[3]}\n");
                        }
                        else if (e.ReadOnlyData.AsmTypes.Contains(terms[0]))
                        {
                            int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, terms[0]);
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos], $"\tmov {terms[1]}, {terms[3]}\n");
                        }
                        else if (e.RunData.GetTypedefByName(terms[0]) is TypedefData typedef)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], typedef.Size, $"\tmov {terms[1]}, {terms[3]}\n");
                        }
                        else if (e.RunData.GetStructByName(terms[0]) is StructData @struct)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], @struct.Size, $"\tmov {terms[1]}, {terms[3]}\n");
                        }
                        else if (e.RunData.GetUnionByName(terms[0]) is UnionData union)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], union.Size, $"\tmov {terms[1]}, {terms[3]}\n");
                        }
                    }
                    else
                    {
                        if (e.ReadOnlyData.TupiTypes.Contains(terms[0]))
                        {
                            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos]);
                        }
                        else if (e.ReadOnlyData.AsmTypes.Contains(terms[0]))
                        {
                            int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, terms[0]);
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos]);
                        }
                        else if (e.RunData.GetTypedefByName(terms[0]) is TypedefData typedef)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], typedef.Size);
                        }
                        else if (e.RunData.GetStructByName(terms[0]) is StructData @struct)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], @struct.Size);
                        }
                        else if (e.RunData.GetUnionByName(terms[0]) is UnionData union)
                        {
                            fnCode += $"\tlocal {terms[1]}: {terms[0]}\n";
                            varData = new(terms[1], terms[0], union.Size);
                        }
                    }

                    if (varData is not null)
                    {
                        currentFunc.ShadowSpace = AddShadowSpaceFunc(currentFunc.ShadowSpace, varData.Size);
                        currentFunc.LocalVars.Add(varData);
                    }
                }
                else if (!isDefVarEnd)   //def vars(local and args)
                {
                    isDefVarEnd = true;
                    foreach (string _line in currentFunc.Args.Select((VarData var) => var.Def))
                    {
                        if (_line != string.Empty)
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
                    if (param.Length > 4)
                        varType = new string[param.Length - 4];

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
                                    varType[i - 4] = e.ReadOnlyData.AsmTypes[pos];
                                    registorsType[i] = e.ReadOnlyData.RegistorsB[pos];
                                }
                            }
                            else if (i < 4)
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
                            fnCode += $"\tmov {varType[i - 4]} ptr [rsp+{i * 8}], {registorsType[i]}\n";
                        }
                        fnCode += $"\tcall {func_name}\n";
                    }
                    fnCode += "\txor rax, rax\n";
                }

                //operator
                if (terms.Length == 2)
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

                //keyData add
                if (terms[^1] == "{")
                {
                    keysInd++;
                    keysData.Add(new(keysInd, string.Empty));
                }

                //loop
                if (terms[0] == "loop" && terms.Length == 2)
                {
                    keysData[^1] = new(keysInd, "loop");
                    fnCode += $"$loop{keysInd}:\n";
                }

                //while
                if (terms[0] == "while" && terms.Length == 2)
                {
                    keysData[^1] = new(keysInd, "while");
                    fnCode += $"while{keysInd}:\n";
                }

                //for
                if (terms[0] == "while" && terms.Length == 2)
                {
                    keysData[^1] = new(keysInd, "while");
                    fnCode += $"while{keysInd}:\n";
                }

                //if
                if (terms[0] == "if" && terms.Length == 2)
                {
                    keysData[^1] = new(keysInd, "if");
                    fnCode += $"if{keysInd}:\n";
                }

                //break
                if (terms[0] == "break" && terms.Length == 1)
                {
                    int i = keysData.Count - 1;
                    var key = keysData[i];
                backStart:
                    if (key.Item2 == string.Empty ||
                        key.Item2 == "if" ||
                        key.Item2 == "elseif" ||
                        key.Item2 == "else")
                    {
                        key = keysData[--i];
                        goto backStart;
                    }
                    else
                    {
                        fnCode += $"\tjmp $break{key.Item1}\n";
                    }
                }

                //continue
                if (terms[0] == "continue" && terms.Length == 1)
                {
                    int i = keysData.Count - 1;
                    var key = keysData[i];
                backStart:
                    if (key.Item2 == string.Empty ||
                        key.Item2 == "if" ||
                        key.Item2 == "elseif" ||
                        key.Item2 == "else")
                    {
                        key = keysData[--i];
                        goto backStart;
                    }
                    else
                    {
                        fnCode += $"\tjmp ${key.Item2}{key.Item1}\n";
                    }
                }

                //keyData remove
                if (terms[^1] == "}" && terms.Length == 1 && keysData.Count > 0)
                {
                    var key = keysData.Last();
                    fnCode += $"\tjmp ${key.Item2}{key.Item1}\n$break{key.Item1}:\n";
                    keysData.Remove(key);
                    keysInd++;
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
                    e.CodeCompiled.Func.Add(fnCode);
                    fnCode = string.Empty;
                    isInsideFunc = isInsideStruct = isInsideUnion = isDefVarEnd = false;
                    currentFunc = null;
                    totalKeys = 0;
                    keysInd = 0;
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

    private static void UpdateInsideFunc(string[] terms, ref int totalKeys, ref bool isInsideFunc)
    {
        if (terms.Length == 0) return;

        if (terms[^1] == "{")
        {
            totalKeys++;
        }
        if (terms[0] == "}")
        {
            totalKeys--;
        }

        if (terms[0] == "fn")
        {
            isInsideFunc = true;
        }
        else if (totalKeys == 0)
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

    private static void UpdateInsideUnion(string[] terms, ref bool isInsideUnion)
    {
        if (terms.Length == 0) return;
        if (terms[0] == "union")
        {
            isInsideUnion = true;
        }
        else if (terms[0] == "}")
        {
            isInsideUnion = false;
        }
    }

    private static bool ContainsDefVar(string[] terms, CompilerArgs e)
    {
        bool contains = false;
        if (!contains)
            foreach (var types in e.ReadOnlyData.TupiTypes)
            {
                if (terms.Contains(types))
                {
                    contains = true;
                    break;
                }
            }
        if (!contains)
            foreach (var types in e.ReadOnlyData.AsmTypes)
            {
                if (terms.Contains(types))
                {
                    contains = true;
                    break;
                }
            }
        if (!contains)
            foreach (var typedefType in e.RunData.Typedef.Select((TypedefData data) => data.Name))
            {
                if (terms.Contains(typedefType))
                {
                    contains = true;
                    break;
                }
            }
        if (!contains)
            foreach (var structType in e.RunData.Structs.Select((StructData data) => data.Name))
            {
                if (terms.Contains(structType))
                {
                    contains = true;
                    break;
                }
            }
        if (!contains)
            foreach (var unionType in e.RunData.Unions.Select((UnionData data) => data.Name))
            {
                if (terms.Contains(unionType))
                {
                    contains = true;
                    break;
                }
            }
        return contains;
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
        int alpha = (int)Math.Ceiling(varSize / 8f) * 8;
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

    private static string CreateIncludeFile(string path, out IHeaderData headerData)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        Directory.CreateDirectory($"{pathDir}/header/");
        StreamWriter writer = File.CreateText($"{pathDir}/header/{fileName}.inc");
        writer.Write(CompileTupiFile(path, out Compiler compiler, true));
        writer.Close();
        headerData = compiler.GetRunData().GetHeaderData();
        return fileName + ".inc";
    }
    #endregion
}