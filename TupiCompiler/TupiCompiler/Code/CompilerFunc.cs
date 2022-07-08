using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal class CompilerFunc : ICompilerCodeFunc, ICompilerHeaderFunc
{
    #region ICompilerCodeFunc
    void ICompilerCodeFunc.PreCompilerEventAdd(ref ICompilerCode compilerCode)
    {
        compilerCode.PreCompilerEvent += PreCompileLines_GrammarSub;
        compilerCode.PreCompilerEvent += PreCompileLines_GrammarAdd;
        compilerCode.PreCompilerEvent += PreCompileLines_Comment;
        compilerCode.PreCompilerEvent += PreCompileLines_String;
        compilerCode.PreCompilerEvent += PreCompileLines_Macro;
        compilerCode.PreCompilerEvent += PreCompileLines_Empty;
    }

    void ICompilerCodeFunc.CompilerEventAdd(ref ICompilerCode compilerCode)
    {
        compilerCode.CompilerEvent += Compile_UseTh;
        compilerCode.CompilerEvent += Compile_UseTp;
        compilerCode.CompilerEvent += Compile_UseFn;
        compilerCode.CompilerEvent += Compile_UseLib;
        compilerCode.CompilerEvent += Compile_Struct;
        compilerCode.CompilerEvent += Compile_Union;
        compilerCode.CompilerEvent += Compile_Typedef;
        compilerCode.CompilerEvent += Compile_GlobalVar;
        compilerCode.CompilerEvent += Compile_Const;
        compilerCode.CompilerEvent += Compile_Func;
    }
    #endregion

    #region ICompilerHeaderFunc
    void ICompilerHeaderFunc.PreCompilerEventAdd(ref ICompilerHeader compilerHeader)
    {
        compilerHeader.PreCompilerEvent += PreCompileLines_GrammarSub;
        compilerHeader.PreCompilerEvent += PreCompileLines_GrammarAdd;
        compilerHeader.PreCompilerEvent += PreCompileLines_Comment;
        compilerHeader.PreCompilerEvent += PreCompileLines_String;
        compilerHeader.PreCompilerEvent += PreCompileLines_Macro;
        compilerHeader.PreCompilerEvent += PreCompileLines_Empty;
    }

    void ICompilerHeaderFunc.CompilerEventAdd(ref ICompilerHeader compilerHeader)
    {
        compilerHeader.CompilerEvent += Compile_UseTh;
        compilerHeader.CompilerEvent += Compile_UseFn;
        compilerHeader.CompilerEvent += Compile_UseLib;
        compilerHeader.CompilerEvent += Compile_Struct;
        compilerHeader.CompilerEvent += Compile_Union;
        compilerHeader.CompilerEvent += Compile_Typedef;
        compilerHeader.CompilerEvent += Compile_Const;
    }
    #endregion

    #region PreCompile
    void PreCompileLines_GrammarSub(object? sender, PreCompilerArgs e)
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
            if (codeStr[pos] == ',' && codeStr[pos + 1] == ' ')
            {
                codeStr = codeStr.Remove(pos + 1, 1);
                pos--;
                continue;
            }
            if (codeStr[pos] == ' ' && codeStr[pos + 1] == ',')
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

    void PreCompileLines_GrammarAdd(object? sender, PreCompilerArgs e)
    {
        string codeStr = e.Code;
        for (int pos = 0; pos < codeStr.Length - 1; pos++)
        {
            if (IsInsideString(codeStr, pos, out _, out _)) continue;
            if (codeStr.Length <= pos + 1) break;
            if (codeStr[pos] == '=' && codeStr[pos + 1] != ' ' && codeStr[pos + 1] != '=')
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
                codeStr[pos] != '>' && codeStr[pos] != '=' &&
                codeStr[pos] != '<' && codeStr[pos] != '-' &&
                codeStr[pos + 1] == '=')
            {
                codeStr = codeStr.Insert(pos + 1, " ");
                continue;
            }

            if (codeStr.Length <= pos + 2) break;
            if (codeStr[pos] != ' ' && codeStr[pos + 2] == '=' &&
                (codeStr[pos + 1] == '-' || codeStr[pos + 1] == '+' ||
                codeStr[pos + 1] == '>' || codeStr[pos + 1] == '=' ||
                codeStr[pos + 1] == '<'))
            {
                codeStr = codeStr.Insert(pos + 1, " ");
                continue;
            }

            if (codeStr.Length <= pos + 2) break;
            if ((codeStr[pos] != ' ' && codeStr[pos + 1] == '-' && codeStr[pos + 2] == '-') ||
                (codeStr[pos] != ' ' && codeStr[pos + 1] == '+' && codeStr[pos + 2] == '+'))
            {
                codeStr = codeStr.Insert(pos + 1, " ");
                continue;
            }
        }
        e.Code = codeStr;
    }

    void PreCompileLines_Comment(object? sender, PreCompilerArgs e)
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

    void PreCompileLines_String(object? sender, PreCompilerArgs e)
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

    void PreCompileLines_Macro(object? sender, PreCompilerArgs e)
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
        }

        lines = ReplaceMacro(lines, e.Macros);

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

    void PreCompileLines_Empty(object? sender, PreCompilerArgs e)
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

        //Console.WriteLine(e.Code);
    }
    #endregion

    #region Compile
    void Compile_UseTh(object? sender, CompilerArgs e)
    {
        bool isMacrosSet = false;

        if (e.IsHeader)
            e.CompiledCode.UseTh.Add("include std_tupi_def.inc");
        else
            e.CompiledCode.UseTh.Add("include header/std_tupi_def.inc");

        for (int i = 0; i < e.Lines.Length; i++)
        {
            string line = e.Lines[i];
            if (line.StartsWith("useth "))
            {
                string path = line.Replace("useth ", "").Replace("<", "").Replace(">", "");
                if (File.Exists(path))
                {
                    string incName = CreateIncludeFile(path, out IHeaderData headerData, out List<string> linkLibs);
                    Program.MainCompiler.GetRunData().AddHeaderData(headerData);
                    Program.MainCompiler.LinkLibs.AddRange(linkLibs);
                    isMacrosSet = true;

                    if (e.IsHeader)
                        e.CompiledCode.UseTh.Add($"include {incName}");
                    else
                        e.CompiledCode.UseTh.Add($"include header/{incName}");
                }
                else if (File.Exists(Program.pathCompile + "/" + path))
                {
                    string incName = CreateIncludeFile(Program.pathCompile + "/" + path, out IHeaderData headerData, out List<string> linkLibs);
                    Program.MainCompiler.GetRunData().AddHeaderData(headerData);
                    Program.MainCompiler.LinkLibs.AddRange(linkLibs);
                    isMacrosSet = true;

                    if (e.IsHeader)
                        e.CompiledCode.UseTh.Add($"include {incName}");
                    else
                        e.CompiledCode.UseTh.Add($"include header/{incName}");
                }
                else if (File.Exists(Program.thPath + path))
                {
                    string incName = CreateIncludeFile(Program.thPath + path, out IHeaderData headerData, out List<string> linkLibs);
                    Program.MainCompiler.GetRunData().AddHeaderData(headerData);
                    Program.MainCompiler.LinkLibs.AddRange(linkLibs);
                    isMacrosSet = true;

                    if (e.IsHeader)
                        e.CompiledCode.UseTh.Add($"include {incName}");
                    else
                        e.CompiledCode.UseTh.Add($"include header/{incName}");
                }
                else
                {
                    ShowErrors($"{path} tupiheader not find",
                        $"{Program.pathCompile + "/" + path} tupiheader not find",
                        $"{Program.thPath + path} tupiheader not find");
                }
            }
        }

        if (isMacrosSet)
            e.SetLines(ReplaceMacro(e.Lines, e.RunData.Macros));
    }

    void Compile_UseTp(object? sender, CompilerArgs e)
    {
        bool isMacrosSet = false;

        for (int i = 0; i < e.Lines.Length; i++)
        {
            string line = e.Lines[i];
            if (line.StartsWith("usetp "))
            {
                string path = line.Replace("usetp ", "").Replace("<", "").Replace(">", "");
                if (File.Exists(path))
                {
                    string asmName = CreateAssemblyFile(path, out ICodeData codeData, out List<string> linkLibs);
                    Program.MainCompiler.GetRunData().AddCodeData(codeData);
                    Program.MainCompiler.LinkLibs.AddRange(linkLibs);
                    isMacrosSet = true;

                    e.CompiledCode.UseTh.Add($"include {asmName}");
                }
                else if (File.Exists(Program.pathCompile + "/" + path))
                {
                    string asmName = CreateAssemblyFile(Program.pathCompile + "/" + path, out ICodeData codeData, out List<string> linkLibs);
                    Program.MainCompiler.GetRunData().AddCodeData(codeData);
                    Program.MainCompiler.LinkLibs.AddRange(linkLibs);
                    isMacrosSet = true;

                    e.CompiledCode.UseTh.Add($"include {asmName}");
                }
                else if (File.Exists(Program.tpPath + path))
                {
                    string asmName = CreateAssemblyFile(Program.tpPath + path, out ICodeData codeData, out List<string> linkLibs);
                    Program.MainCompiler.GetRunData().AddCodeData(codeData);
                    Program.MainCompiler.LinkLibs.AddRange(linkLibs);
                    isMacrosSet = true;

                    e.CompiledCode.UseTh.Add($"include {asmName}");
                }
                else
                {
                    ShowErrors($"{path} tupicode not find",
                        $"{Program.pathCompile + "/" + path} tupicode not find",
                        $"{Program.tpPath + path} tupicode not find");
                }
            }
        }

        if (isMacrosSet)
            e.SetLines(ReplaceMacro(e.Lines, e.RunData.Macros));
    }

    void Compile_UseFn(object? sender, CompilerArgs e)
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
                e.CompiledCode.UseFn.Add($"extern {terms[1]}: proc");
            }
        }
    }

    void Compile_UseLib(object? sender, CompilerArgs e)
    {
        ICompiler? compiler = (ICompiler?)sender;
        if (compiler is null) return;

        for (int i = 0; i < e.Lines.Length; i++)
        {
            string line = e.Lines[i];
            if (line.StartsWith("uselib "))
            {
                string path = line.Replace("uselib ", "").Replace("<", "").Replace(">", "");
                if (File.Exists(path))
                {
                    compiler.LinkLibs.Add(path);
                }
                else if (File.Exists($"{Program.pathCompile}/{path}"))
                {
                    compiler.LinkLibs.Add($"{Program.pathCompile}/{path}");
                }
                else if (File.Exists($"{Program.x64Path}lib/{path}"))
                {
                    compiler.LinkLibs.Add($"{Program.x64Path}lib/{path}");
                }
                else
                {
                    ConsoleColor consoleColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{path} lib not find");
                    Console.WriteLine($"{Program.pathCompile}/{path} lib not find");
                    Console.WriteLine($"{Program.x64Path}lib/{path} lib not find");
                    Console.ForegroundColor = consoleColor;
                }
            }
        }
    }

    void Compile_Struct(object? sender, CompilerArgs e)
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
                if (terms[0] == "struct" || terms[0] == "cstruct")
                {
                    if (terms[0] == "struct")
                        currentStruct = new StructData(terms[1], false);
                    else
                        currentStruct = new StructData(terms[1], true);
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
                    currentStruct.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos], false));
                    int mod = currentStruct.Size % e.ReadOnlyData.TypeSize[pos];
                    if (mod != 0 && currentStruct.IsCStruct)
                    {
                        int dist = e.ReadOnlyData.TypeSize[pos] - mod;
                        structCode += $"db {dist} dup(?)\n";
                        currentStruct.CStructSpaces.Add(new(currentStruct.Vars.Count, dist));
                        currentStruct.Size += dist;
                    }
                    currentStruct.Size += e.ReadOnlyData.TypeSize[pos];
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                }
                else if (e.ReadOnlyData.AsmTypes.Contains(terms[0]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, terms[0]);
                    currentStruct.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos], false));
                    int mod = currentStruct.Size % e.ReadOnlyData.TypeSize[pos];
                    if (mod != 0 && currentStruct.IsCStruct)
                    {
                        int dist = e.ReadOnlyData.TypeSize[pos] - mod;
                        structCode += $"db {dist} dup(?)\n";
                        currentStruct.CStructSpaces.Add(new(currentStruct.Vars.Count, dist));
                        currentStruct.Size += dist;
                    }
                    currentStruct.Size += e.ReadOnlyData.TypeSize[pos];
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                }
                else if (e.RunData.GetTypedefByName(terms[0]) is TypedefData typedef)
                {
                    currentStruct.Vars.Add(new(terms[1], terms[0], typedef.Size, false));
                    int mod = currentStruct.Size % typedef.Size;
                    if (mod != 0 && currentStruct.IsCStruct)
                    {
                        int dist = typedef.Size - mod;
                        structCode += $"db {dist} dup(?)\n";
                        currentStruct.CStructSpaces.Add(new(currentStruct.Vars.Count, dist));
                        currentStruct.Size += dist;
                    }
                    currentStruct.Size += typedef.Size;
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                }
                else if (e.RunData.GetStructByName(terms[0]) is StructData @struct)
                {
                    currentStruct.Vars.Add(new(terms[1], terms[0], @struct.Size, false));
                    int mod = currentStruct.Size % @struct.Size;
                    if (mod != 0 && currentStruct.IsCStruct)
                    {
                        int dist = @struct.Size - mod;
                        structCode += $"db {dist} dup(?)\n";
                        currentStruct.CStructSpaces.Add(new(currentStruct.Vars.Count, dist));
                        currentStruct.Size += dist;
                    }
                    currentStruct.Size += @struct.Size;
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                }
                else if (e.RunData.GetUnionByName(terms[0]) is UnionData union)
                {
                    currentStruct.Vars.Add(new(terms[1], terms[0], union.Size, false));
                    int mod = currentStruct.Size % union.Size;
                    if (mod != 0 && currentStruct.IsCStruct)
                    {
                        int dist = union.Size - mod;
                        structCode += $"db {dist} dup(?)\n";
                        currentStruct.CStructSpaces.Add(new(currentStruct.Vars.Count, dist));
                        currentStruct.Size += dist;
                    }
                    currentStruct.Size += union.Size;
                    structCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                }

                if (!isInsideStruct)
                {
                    structCode += $"{currentStruct.Name} ends";
                    e.CompiledCode.Struct.Add(structCode);
                    structCode = string.Empty;
                    currentStruct = null;
                }
            }
        }
    }

    void Compile_Union(object? sender, CompilerArgs e)
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
                    currentUnion.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos], false));
                    if (currentUnion.Size < e.ReadOnlyData.TypeSize[pos])
                        currentUnion.Size = e.ReadOnlyData.TypeSize[pos];
                }
                else if (e.ReadOnlyData.AsmTypes.Contains(terms[0]))
                {
                    int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, terms[0]);
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], e.ReadOnlyData.TypeSize[pos], false));
                    if (currentUnion.Size < e.ReadOnlyData.TypeSize[pos])
                        currentUnion.Size = e.ReadOnlyData.TypeSize[pos];
                }
                else if (e.RunData.GetTypedefByName(terms[0]) is TypedefData typedef)
                {
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], typedef.Size, false));
                    if (currentUnion.Size < typedef.Size)
                        currentUnion.Size = typedef.Size;
                }
                else if (e.RunData.GetStructByName(terms[0]) is StructData @struct)
                {
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], @struct.Size, false));
                    if (currentUnion.Size < @struct.Size)
                        currentUnion.Size = @struct.Size;
                }
                else if (e.RunData.GetUnionByName(terms[0]) is UnionData union)
                {
                    unionCode += line.Replace($"{terms[0]} {terms[1]}", $"{terms[1]} {terms[0]}") + "\n";
                    currentUnion.Vars.Add(new(terms[1], terms[0], union.Size, false));
                    if (currentUnion.Size < union.Size)
                        currentUnion.Size = union.Size;
                }

                if (!isInsideUnion)
                {
                    unionCode += $"{currentUnion.Name} ends";
                    e.CompiledCode.Union.Add(unionCode);
                    unionCode = string.Empty;
                }
            }
        }
    }

    void Compile_Typedef(object? sender, CompilerArgs e)
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
                e.CompiledCode.Typedef.Add($"{terms[2]} typedef {terms[1]}");

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

    void Compile_GlobalVar(object? sender, CompilerArgs e)
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

            VarData? varData = GetMetaGlobalVar(line, e);
            if (varData is not null)
            {
                e.RunData.GlobalVars.Add(varData.Name, varData);
            }
        }
    }

    void Compile_Const(object? sender, CompilerArgs e)
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

            if (terms[0] == "const" && !e.RunData.ExistConst(terms[1]) && terms.Length == 3 &&
                !(terms[2] == "sizeof" || terms[2] == "size" || terms[2] == "lengthof" ||
                terms[2] == "length" || terms[2] == "offset"))
            {
                e.RunData.Const.Add(terms[1]);
                e.CompiledCode.Const.Add($"{terms[1]} equ {terms[2]}");
            }
            else if (terms[0] == "const" && !e.RunData.ExistConst(terms[1]) && terms.Length == 4 &&
                terms[2] == "sizeof" || terms[2] == "size" || terms[2] == "lengthof" ||
                terms[2] == "length" || terms[2] == "offset")
            {
                e.RunData.Const.Add(terms[1]);
                e.CompiledCode.Const.Add($"{terms[1]} equ {terms[2]} {terms[3]}");
            }
        }
    }

    void Compile_Func(object? sender, CompilerArgs e)
    {
        FuncData? currentFunc = null;
        string fnCode = string.Empty;
        int localVarPos = 0;

        int keysInd = 0;
        int totalKeys = 0;
        List<Tuple<int, string>> keysData = new();
        List<int> keysIf = new();

        bool isInsideFunc = false, isInsideStruct = false, isInsideUnion = false;
        for (int lpos = 0; lpos < e.Lines.Length; lpos++)
        {
            string line = e.Lines[lpos];
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
                        if (_pos > 0 && _pos < 4)
                            val = e.ReadOnlyData.RegistorsAll[_pos][a];
                        else
                            val = e.ReadOnlyData.RegistorsAll[3][a];

                        fnCode += $"\tlocal {name}: {type}\n";
                        VarData varData = new(name, type, e.ReadOnlyData.TypeSize[_pos], $"\tmov {name}, {val}\n", false);
                        currentFunc.Args.Add(varData);
                        currentFunc.ShadowSpace = AddShadowSpaceFunc(currentFunc.ShadowSpace, varData.Size);
                    }
                    localVarPos += fnCode.Length;
                }
            }
            else if (currentFunc is not null)
            {
                UpdateInsideFunc(terms, ref totalKeys, ref isInsideFunc);

                bool contains = ContainsDefVar(terms, e); ;

                //fn vars
                if (contains)
                {
                    VarData? varData = GetMetaFnVar(line, out string varCode, e);
                    fnCode = fnCode.Insert(localVarPos, varCode);
                    localVarPos += varCode.Length;

                    if (varData is not null)
                    {
                        currentFunc.ShadowSpace = AddShadowSpaceFunc(currentFunc.ShadowSpace, varData.Size);
                        currentFunc.LocalVars.Add(varData);
                    }
                }

                //fn vars def
                if (!isInsideFunc)
                {
                    foreach (string _line in currentFunc.Args.Select((VarData var) => var.Def))
                    {
                        if (_line != string.Empty)
                        {
                            fnCode = fnCode.Insert(localVarPos, _line);
                            localVarPos += _line.Length;
                        }
                    }
                    foreach (string _line in currentFunc.LocalVars.Select((VarData var) => var.Def))
                    {
                        if (_line != string.Empty)
                        {
                            fnCode = fnCode.Insert(localVarPos, _line);
                            localVarPos += _line.Length;
                        }
                    }

                    string temp;
                    temp = "\tpush rdi\n";
                    fnCode = fnCode.Insert(localVarPos, temp);
                    localVarPos += temp.Length;
                    temp = $"\tsub rsp, {CorrectShadowSpaceFunc(currentFunc.ShadowSpace)}\t;Reserve the shadow space\n";
                    fnCode = fnCode.Insert(localVarPos, temp);
                    localVarPos += temp.Length;
                    temp = "\tmov rdi, rsp\n";
                    fnCode = fnCode.Insert(localVarPos, temp);
                    localVarPos += temp.Length;
                }

                // call funcs
                if (CallFunc(line, currentFunc, e, ref fnCode))
                    fnCode += "\txor rax, rax\n";

                //operator
                if (terms.Length == 2)
                {
                    string operate = string.Empty;
                    switch (terms[1])
                    {
                        case "++":
                            operate = "inc";
                            break;
                        case "--":
                            operate = "dec";
                            break;
                    }
                    if (operate != string.Empty)
                    {
                        ArgState state = GetArgState(terms[0], out string? var, out VarData? varData, currentFunc, e);
                        if (state == ArgState.Var)
                        {
                            fnCode += $"\t{operate} {terms[0]}\n";
                        }
                        else if (state == ArgState.PtrData && var is not null && varData is not null)
                        {
                            int pos = GetPosRegistorBySize(varData.Size);
                            fnCode += $"\tmov rbx, {var}\n";
                            fnCode += $"\t{operate} {e.ReadOnlyData.AsmTypes[pos]} ptr [rbx]\n";
                        }
                    }
                }
                else if (terms.Length >= 3)
                {
                    string operate = string.Empty;
                    switch (terms[1])
                    {
                        case "+=":
                            operate = "add";
                            break;
                        case "-=":
                            operate = "sub";
                            break;
                        case "=":
                            operate = "mov";
                            break;
                    }
                    if (operate != string.Empty)
                    {
                        ArgState state1 = GetArgState(terms[0], out string? var1, out VarData? varData1, currentFunc, e);
                        int pos1 = -1;
                        if (varData1 is not null)
                            pos1 = GetPosRegistorBySize(varData1.Size);
                        bool existFn = CallFunc(line, currentFunc, e, ref fnCode, 2);

                        if (existFn && pos1 != -1)
                        {
                            string fistArg = terms[0];
                            if (state1 == ArgState.PtrData && var1 is not null)
                            {
                                fistArg = $"{e.ReadOnlyData.AsmTypes[pos1]} ptr [rbx]";
                                fnCode += $"\tmov rbx, {var1}\n";
                            }

                            fnCode += $"\t{operate} {fistArg}, {e.ReadOnlyData.RegistorsA[pos1]}\n";
                            fnCode += "\txor rax, rax\n";
                            if (state1 == ArgState.PtrData && var1 is not null)
                                fnCode += "\txor rbx, rbx\n";
                        }
                        else if(pos1 != -1)
                        {
                            ArgState state2 = GetArgState(terms[2], out string? var2, out VarData? varData2, currentFunc, e);

                            string thirdArg = terms[0];
                            if (state1 == ArgState.PtrData && var1 is not null)
                            {
                                thirdArg = $"{e.ReadOnlyData.AsmTypes[pos1]} ptr [rax]";
                                fnCode += $"\tmov rax, {var1}\n";
                            }

                            string fistOp = "mov", secondArg = terms[2],
                                fistArg = e.ReadOnlyData.RegistorsB[pos1],
                                quadArg = e.ReadOnlyData.RegistorsB[pos1];
                            string thirdArg2 = thirdArg;
                            if (state2 == ArgState.RefVar && var2 is not null)
                            {
                                fistOp = "lea";
                                secondArg = var2;
                            }
                            else if (state2 == ArgState.PtrData && var2 is not null)
                            {
                                secondArg = var2;
                                quadArg = "[rbx]";
                                fistArg = "rbx";
                                fnCode += $"\tmov rcx, {thirdArg}\n";
                                thirdArg = "rcx";
                            }

                            fnCode += $"\t{fistOp} {fistArg}, {secondArg}\n";
                            fnCode += $"\t{operate} {thirdArg}, {quadArg}\n";
                            if (state2 == ArgState.PtrData && var2 is not null)
                            {
                                fnCode += $"\tmov {thirdArg2}, rcx\n";
                                fnCode += $"\txor rcx, rcx\n";
                            }
                            fnCode += "\txor rbx, rbx\n";
                            if (state1 == ArgState.PtrData && var1 is not null)
                                fnCode += "\txor rax, rax\n";
                        }
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
                if (terms[0].StartsWith("while(") && terms.Length > 1)
                {
                    List<string> args = GetArgs(terms);
                    string term1 = args[0], compare = "je", term2 = "1";
                    if (args.Count >= 3)
                    {
                        compare = GetCompare(terms[1]);
                        term2 = args[2];
                    }
                    keysData[^1] = new(keysInd, "while");
                    fnCode += $"$while{keysInd}:\n";
                    fnCode += $"\tcmp {args[0]}, {term2}\n";
                    fnCode += $"\t{compare} $contunie_while{keysInd}\n";
                    fnCode += $"\tjmp $break{keysInd}\n";
                    fnCode += $"$contunie_while{keysInd}:\n";
                }

                //if
                if (terms[0].StartsWith("if(") && terms.Length > 1)
                {
                    List<string> args = GetArgs(terms);
                    string term1 = args[0], compare = "je", term2 = "1";
                    if (args.Count >= 3)
                    {
                        compare = GetCompare(terms[1]);
                        term2 = args[2];
                    }
                    keysIf.Add(keysInd);
                    keysData[^1] = new(keysInd, "if");
                    fnCode += $"\tcmp {term1}, {term2}\n";
                    fnCode += $"\t{compare} $if{keysInd}\n";
                    fnCode += $"\tjmp $endif{keysInd}\n";
                    fnCode += $"$if{keysInd}:\n";
                }

                //elseif
                if (terms[0].StartsWith("elseif(") && terms.Length > 1)
                {
                    List<string> args = GetArgs(terms);
                    string term1 = args[0], compare = "je", term2 = "1";
                    if (args.Count >= 3)
                    {
                        compare = GetCompare(terms[1]);
                        term2 = args[2];
                    }
                    keysData[^1] = new(keysInd, "elseif");
                    fnCode += $"\tcmp {term1}, {term2}\n";
                    fnCode += $"\t{compare} $elseif{keysInd}\n";
                    fnCode += $"\tjmp $endelseif{keysInd}\n";
                    fnCode += $"$elseif{keysInd}:\n";
                }

                //else
                if (terms[0].StartsWith("else") && terms.Length == 2)
                {
                    keysData[^1] = new(keysInd, "else");
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
                    if (lpos + 1 < e.Lines.Length)
                    {
                        string[] nextTerms = e.GetTermsLine(e.Lines[lpos + 1]);
                        if (key.Item2 == "if" || key.Item2 == "elseif"){
                            if (nextTerms[0].StartsWith("elseif(") || nextTerms[0].StartsWith("else"))
                            {
                                fnCode += $"\tjmp $endallif{keysIf[^1]}\n";
                                fnCode += $"$end{key.Item2}{key.Item1}:\n";
                            }
                            else if (!nextTerms[0].StartsWith("elseif(") && !nextTerms[0].StartsWith("else"))
                            {
                                fnCode += $"$endallif{keysIf[^1]}:\n";
                                keysIf.RemoveAt(keysIf.Count - 1);
                                fnCode += $"$end{key.Item2}{key.Item1}:\n";
                            }
                        }
                        else if(key.Item2 == "else")
                        {
                            fnCode += $"$endallif{keysIf[^1]}:\n";
                        }
                        else
                        {
                            fnCode += $"\tjmp ${key.Item2}{key.Item1}\n$break{key.Item1}:\n";
                        }
                    }

                    keysData.Remove(key);
                }

                //return
                if (terms[0] == "return" && terms.Length == 1)
                {
                    fnCode += $"\tadd rsp, {CorrectShadowSpaceFunc(currentFunc.ShadowSpace)}\t;Remove shadow space\n";
                    fnCode += "\tpop rdi\n";
                    fnCode += "\tret\n";
                }
                else if (terms[0] == "return" && terms.Length > 1 && GetVarData(terms[1], currentFunc, e) is VarData varData)
                {
                    int pos = GetPosRegistorBySize(varData.Size);
                    fnCode += line.Replace($"{terms[0]} ", $"\tmov {e.ReadOnlyData.RegistorsA[pos]}, ") + "\n";
                    fnCode += $"\tadd rsp, {CorrectShadowSpaceFunc(currentFunc.ShadowSpace)}\t;Remove shadow space\n";
                    fnCode += "\tpop rdi\n";
                    fnCode += "\tret\n";
                }

                //end
                if (!isInsideFunc)
                {
                    fnCode += $"\tadd rsp, {CorrectShadowSpaceFunc(currentFunc.ShadowSpace)}\t;Remove shadow space\n";
                    fnCode += "\tpop rdi\n";
                    fnCode += "\tret\n";
                    fnCode += $"{currentFunc.Name} endp";
                    e.CompiledCode.Func.Add(fnCode);
                    fnCode = string.Empty;
                    localVarPos = 0;
                    isInsideFunc = isInsideStruct = isInsideUnion = false;
                    currentFunc = null;
                    totalKeys = 0;
                    keysInd = 0;
                }
            }
        }
    }
    #endregion

    #region Private Funcs
    private bool IsInsideString(string code, int pos, out bool isSimpleStr, out bool isCompleteStr)
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

    private bool IsInsidePath(string code, int pos)
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

    private void UpdateInsideFunc(string[] terms, ref int totalKeys, ref bool isInsideFunc)
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

    private void UpdateInsideStruct(string[] terms, ref bool isInsideStruct)
    {
        if (terms.Length == 0) return;
        if (terms[0] == "struct" || terms[0] == "cstruct")
        {
            isInsideStruct = true;
        }
        else if (terms[0] == "}")
        {
            isInsideStruct = false;
        }
    }

    private void UpdateInsideUnion(string[] terms, ref bool isInsideUnion)
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

    private bool ContainsDefVar(string[] terms, CompilerArgs e)
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

    private int CorrectShadowSpaceFunc(int shadowSpace)
    {
        if (shadowSpace == 32) return 32;

        int rest = shadowSpace % 8;
        if (rest == 0) return shadowSpace + 8;
        else return shadowSpace + 16 - rest;
    }

    private int AddShadowSpaceFunc(int shadowSpace, int varSize)
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

    private string[] ReplaceMacro(string[] lines, Dictionary<string, string> macros)
    {
        char[] seps1 = new[] { '\t', ' ', ',', '(', '{', '[', '=', '+', '-', '/', '*' };
        char[] seps2 = new[] { ' ', ',', ')', '}', ']', '=', '+', '-', '/', '*', '\n', '\r', };

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line == string.Empty) continue;

            foreach (string macro in macros.Keys)
            {
                int ind = line.IndexOf(macro);
                if (ind > -1 && !IsInsideString(line, ind, out _, out _) && !IsInsidePath(line, ind))
                {
                    foreach (char sep1 in seps1)
                    {
                        foreach (char sep2 in seps2)
                        {
                            if (line.StartsWith($"{macro}{sep2}"))
                            {
                                line = line.Remove(ind, macro.Length);
                                line = line.Insert(ind, macros[macro]);
                                ind = line.IndexOf(macro);
                                if (ind < 0 || IsInsideString(line, ind, out _, out _) || IsInsidePath(line, ind)) break;
                            }
                            else if (line.EndsWith($"{sep1}{macro}"))
                            {
                                line = line.Remove(ind, macro.Length);
                                line = line.Insert(ind, macros[macro]);
                                ind = line.IndexOf(macro);
                                if (ind < 0 || IsInsideString(line, ind, out _, out _) || IsInsidePath(line, ind)) break;
                            }
                            else if (ind - 1 == line.IndexOf($"{sep1}{macro}{sep2}"))
                            {
                                line = line.Remove(ind, macro.Length);
                                line = line.Insert(ind, macros[macro]);
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
        return lines;
    }

    private string CreateIncludeFile(string path, out IHeaderData headerData, out List<string> linkLibs)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        Directory.CreateDirectory($"{Program.pathDir}/header/");
        StreamWriter writer = File.CreateText($"{Program.pathDir}/header/{fileName}.inc");
        writer.Write(Program.CompileTupiHeaderFile(path, out ICompilerHeader compilerHeader));
        writer.Close();
        headerData = compilerHeader.GetRunData().GetHeaderData();
        linkLibs = compilerHeader.LinkLibs;
        return fileName + ".inc";
    }

    private string CreateAssemblyFile(string path, out ICodeData codeData, out List<string> linkLibs)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        StreamWriter writer = File.CreateText($"{Program.pathDir}/{fileName}.asm");
        writer.Write(Program.CompileTupiCodeFile(path, out ICompilerCode compilerCode, false));
        writer.Close();
        codeData = compilerCode.GetRunData().GetCodeData();
        linkLibs = compilerCode.LinkLibs;
        return fileName + ".asm";
    }

    private VarData? GetMetaGlobalVar(string line, CompilerArgs e)
    {
        string[] terms = e.GetTermsLine(line);
        string varType = string.Empty, varName = string.Empty, varDef = string.Empty;
        int varSize;
        bool varRef = false;

        if (terms[0] == "ref" && terms.Length > 3)
        {
            varType = terms[1];
            varName = terms[2];
            varDef = line.Remove(0, "ref".Length + varType.Length + varName.Length + 3);
            varRef = true;
        }
        else if(terms.Length > 2)
        {
            varType = terms[0];
            varName = terms[1];
            varDef = line.Remove(0, varType.Length + varName.Length + 2);
            varRef = false;
        }

        if (e.ReadOnlyData.TupiTypes.Contains(varType))
        {
            int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, varType);
            varSize = e.ReadOnlyData.TypeSize[pos];
            e.CompiledCode.GlobalVar.Add($"{varName} {varType} {varDef}");
        }
        else if (e.ReadOnlyData.AsmTypes.Contains(varType))
        {
            int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, varType);
            varSize = e.ReadOnlyData.TypeSize[pos];
            e.CompiledCode.GlobalVar.Add($"{varName} {varType} {varDef}");
        }
        else if (e.RunData.GetStructByName(varType) is StructData @struct)
        {
            varSize = @struct.Size;
            if (@struct.IsCStruct && @struct.CStructSpaces.Count > 0)
            {
                string init = $"{varName} {varType} ";
                string declare = line.Replace($"{varType} {varName} ", "");
                for (int _i = 0, i = 0, brk = 0; _i < declare.Length; _i++)
                {
                    if (@struct.CStructSpaces.Count <= i) break;
                    if (declare[_i] == ',' && !IsInsideString(declare, _i, out _, out _))
                    {
                        brk++;
                        if (brk == @struct.CStructSpaces[i].Item1 - 1 + i)
                        {
                            i++;
                            declare = declare.Insert(_i + 1, " {},");
                        }
                    }
                }
                e.CompiledCode.GlobalVar.Add(init + declare);
            }
            else
            {
                e.CompiledCode.GlobalVar.Add($"{varName} {varType} {varDef}");
            }
        }
        else if (e.RunData.GetTypedefByName(varType) is TypedefData typedef)
        {
            varSize = typedef.Size;
            e.CompiledCode.GlobalVar.Add($"{varName} {varType} {varDef}");
        }
        else if (e.RunData.GetUnionByName(varType) is UnionData union)
        {
            varSize = union.Size;
            e.CompiledCode.GlobalVar.Add($"{varName} {varType} {varDef}");
        }
        else
        {
            return null;
        }

        return new(varName, varType, varSize, varDef, varRef);
    }

    private VarData? GetMetaFnVar(string line, out string fnCode, CompilerArgs e)
    {
        string[] terms = e.GetTermsLine(line);
        string varType = string.Empty, varName = string.Empty, varVal = string.Empty, varDef = string.Empty;
        int varSize = 0;
        bool varRef = false;

        if (terms[0] == "ref" && terms.Length > 3)
        {
            varType = terms[1];
            varName = terms[2];
            for (int i = 4; i < terms.Length; i++)
                varVal += terms[i];
            if (varVal != string.Empty)
                varDef = $"\tmov {varName}, {varVal}\n";
            varRef = true;
        }
        else
        {
            varType = terms[0];
            varName = terms[1];
            for (int i = 3; i < terms.Length; i++)
                varVal += terms[i];
            if (varVal != string.Empty)
                varDef = $"\tmov {varName}, {varVal}\n";
            varRef = false;
        }

        if (terms.Length > 3)
        {
            if (e.ReadOnlyData.TupiTypes.Contains(varType))
            {
                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, varType);
                varSize = e.ReadOnlyData.TypeSize[pos];
            }
            else if (e.ReadOnlyData.AsmTypes.Contains(varType))
            {
                int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, varType);
                varSize = e.ReadOnlyData.TypeSize[pos];
            }
            else if (e.RunData.GetTypedefByName(varType) is TypedefData typedef)
            {
                varSize = typedef.Size;
            }
            else if (e.RunData.GetStructByName(varType) is StructData @struct)
            {
                varSize = @struct.Size;
            }
            else if (e.RunData.GetUnionByName(varType) is UnionData union)
            {
                varSize = union.Size;
            }
            else
            {
                fnCode = string.Empty;
                return null;
            }
            fnCode = $"\tlocal {varName}: {varType}\n";
        }
        else
        {
            if (e.ReadOnlyData.TupiTypes.Contains(varType))
            {
                int pos = Array.IndexOf(e.ReadOnlyData.TupiTypes, varType);
                varSize = e.ReadOnlyData.TypeSize[pos];
            }
            else if (e.ReadOnlyData.AsmTypes.Contains(varType))
            {
                int pos = Array.IndexOf(e.ReadOnlyData.AsmTypes, varType);
                varSize = e.ReadOnlyData.TypeSize[pos];
            }
            else if (e.RunData.GetTypedefByName(varType) is TypedefData typedef)
            {
                varSize = typedef.Size;
            }
            else if (e.RunData.GetStructByName(varType) is StructData @struct)
            {
                varSize = @struct.Size;
            }
            else if (e.RunData.GetUnionByName(varType) is UnionData union)
            {
                varSize = union.Size;
            }
            else
            {
                fnCode = string.Empty;
                return null;
            }
            fnCode = $"\tlocal {varName}: {varType}\n";
        }

        return new(varName, varType, varSize, varDef, varRef);
    }

    private VarData? GetVarData(string varName, FuncData currentFunc, CompilerArgs e)
    {
        if (currentFunc.Args.Find(x => x.Name == varName) is VarData argVar)
        {
            return argVar;
        }
        else if (currentFunc.GetLocalVarByName(varName) is VarData localVar)
        {
            return localVar;
        }
        else if (e.RunData.GlobalVars.ContainsKey(varName))
        {
            VarData? globalVar = e.RunData.GlobalVars[varName];
            return globalVar;
        }
        else
        {
            return null;
        }
    }

    private bool CallFunc(string line, FuncData currentFunc, CompilerArgs e, ref string fnCode, int start = 0)
    {
        string[] terms = e.GetTermsLine(line);
        if (terms[start].Contains('(') && !terms[start].StartsWith("if(") && !terms[start].StartsWith("elseif(") && !terms[start].StartsWith("while("))
        {
            string func_name = terms[start].Remove(terms[start].IndexOf('('));
            string _param = terms[start].Substring(terms[start].IndexOf('(') + 1, terms[start].IndexOf(')') - terms[start].IndexOf('(') - 1);
            string[] param = _param.Split(new char[] { ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string[] param1 = new string[param.Length];
            string[] param2 = new string[param.Length];
            string[] comand = new string[param.Length];
            string[] registorsType = new string[4];
            string[] registorsB = new string[param.Length];
            string[] varType = Array.Empty<string>();
            if (param.Length > 4)
                varType = new string[param.Length - 4];

            for (int i = 0; i < param.Length; i++)
            {
                string varPath = param[i];
                string[] varSemiPath = param[i].Split(new[] { '.' });
                ArgState state = GetArgState(varSemiPath[0], out string? varArg, out VarData? varData, currentFunc, e);

                if (state == ArgState.RefVar && varArg is not null)
                {
                    comand[i] = "lea";
                    param1[i] = varArg;
                    if (i < 4)
                    {
                        registorsType[i] = e.ReadOnlyData.RegistorsAll[3][i];
                    }
                    registorsB[i] = e.ReadOnlyData.RegistorsB[3];
                    param2[i] = registorsB[i];
                }
                else if (state == ArgState.Var && varData is not null)
                {
                    int pos;

                    comand[i] = "mov";
                    for (int p = 1; p < varSemiPath.Length; p++)
                    {
                        if (e.RunData.GetStructByName(varData.Type)?.GetVarByName(varSemiPath[p]) is VarData der)
                            varData = der;
                        else
                            break;
                    }
                    pos = GetPosRegistorBySize(varData.Size);

                    param1[i] = varPath;
                    if (i < 4)
                    {
                        registorsType[i] = e.ReadOnlyData.RegistorsAll[pos][i];
                    }
                    registorsB[i] = e.ReadOnlyData.RegistorsB[pos];
                    param2[i] = registorsB[i];
                }
                else if(state == ArgState.PtrData && varArg is not null)
                {
                    comand[i] = "mov";
                    param1[i] = varArg;
                    if (i < 4)
                    {
                        registorsType[i] = e.ReadOnlyData.RegistorsAll[3][i];
                    }
                    registorsB[i] = e.ReadOnlyData.RegistorsB[3];
                    param2[i] = $"[{registorsB[i]}]";
                }
                else if (state == ArgState.Number || state == ArgState.BasicEqu || state == ArgState.Const)
                {
                    comand[i] = "mov";
                    param1[i] = varPath;
                    if (i < 4)
                    {
                        registorsType[i] = e.ReadOnlyData.RegistorsAll[3][i];
                    }
                    registorsB[i] = e.ReadOnlyData.RegistorsB[3];
                    param2[i] = registorsB[i];
                }
                else
                {
                    ShowErrors($"var {varSemiPath[0]} dosn't exist in this context");
                }
            }

            for (int i = 0; !(i >= 4 || i >= param.Length); i++)
            {
                fnCode += $"\t{comand[i]} {registorsB[i]}, {param1[i]}\n";
                fnCode += $"\tmov {registorsType[i]}, {param2[i]}\n";
            }
            for (int i = 4; i < param.Length; i++)
            {
                fnCode += $"\t{comand[i]} {registorsB[i]}, {param1[i]}\n";
                fnCode += $"\tmov {varType[i - 4]} ptr [rsp+{i * 8}], {param2[i]}\n";
            }
            fnCode += $"\tcall {func_name}\n";
            return true;
        }
        else
        {
            return false;
        }
    }

    private int GetPosRegistorBySize(int size)
    {
        return size switch
        {
            1 => 0,
            2 => 1,
            4 => 2,
            8 => 3,
            _ => -1,
        };
    }

    private List<string> GetArgs(string[] terms)
    {
        List<string> args = new();
        for (int i = 0; i < terms.Length - 1; i++)
        {
            string arg = terms[i];
            arg = arg.Replace("while(", "");
            arg = arg.Replace("elseif(", "");
            arg = arg.Replace("if(", "");
            arg = arg.Replace(",", "");
            arg = arg.Replace(")", "");
            arg = arg.Replace("}", "");
            if (arg != string.Empty)
                args.Add(arg);
        }
        return args;
    }

    private string GetCompare(string term)
    {
        return term switch
        {
            ">" => "jg",
            ">=" => "jge",
            "==" => "je",
            "<=" => "jle",
            "<" => "jl",
            _ => string.Empty,
        };
    }

    private ArgState GetArgState(string arg, out string? var, out VarData? varData,
        FuncData currentFunc, CompilerArgs e)
    {
        varData = GetVarData(arg, currentFunc, e);

        if (varData != null && !varData.Ref)
        {
            var = arg;
            return ArgState.Var;
        }
        else if (varData != null && varData.Ref)
        {
            var = arg;
            return ArgState.RefVar;
        }
        else if (arg.StartsWith("&"))
        {
            var = arg.Remove(0, 1);
            varData = GetVarData(var, currentFunc, e);
            return ArgState.RefVar;
        }
        else if (arg.StartsWith("[") && arg.EndsWith("]"))
        {
            var = arg.Remove(arg.Length - 1).Remove(0, 1);
            varData = GetVarData(var, currentFunc, e);
            return ArgState.PtrData;
        }
        else if (long.TryParse(arg, out _) || double.TryParse(arg, out _))
        {
            var = null;
            return ArgState.Number;
        }
        else if (arg == "TRUE" || arg == "FALSE" || arg == "NULL")
        {
            var = null;
            return ArgState.BasicEqu;
        }
        else if (e.RunData.ExistConst(arg))
        {
            var = null;
            return ArgState.Const;
        }
        else
        {
            var = null;
            return ArgState.None;
        }
    }

    private void ShowErrors(params string[] errors)
    {
        ConsoleColor consoleColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (string error in errors)
            Console.WriteLine(error);
        Console.ForegroundColor = consoleColor;
    }

    private void ShowWarnigns(params string[] warnigns)
    {
        ConsoleColor consoleColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (string warnign in warnigns)
            Console.WriteLine(warnign);
        Console.ForegroundColor = consoleColor;
    }
    #endregion

    #region MyEnums
    enum ArgState
    {
        None,
        Var,
        RefVar,
        PtrData,
        Number,
        BasicEqu,
        Const,
    }
    #endregion
}