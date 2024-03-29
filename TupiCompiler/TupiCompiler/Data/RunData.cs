﻿namespace TupiCompiler.Data;
internal class RunData: ICodeData, IHeaderData
{
    public List<FuncData> Funcs { get; private set; }
    public List<StructData> Structs { get; private set; }
    public List<UnionData> Unions { get; private set; }
    public List<TypedefData> Typedef { get; private set; }
    public Dictionary<string, VarData> GlobalVars { get; private set; }
    public List<string> Const { get; private set; }
    public Dictionary<string, string> Macros { get; private set; }

    internal RunData()
    {
        Funcs = new();
        Structs = new();
        Unions = new();
        Typedef = new();
        Const = new();
        GlobalVars = new();
        Macros = new();
    }

    internal TypedefData? GetTypedefByName(string name)
    {
        foreach (var s in Typedef)
        {
            if (s.Name == name)
                return s;
        }
        return null;
    }

    internal FuncData? GetFuncByName(string name)
    {
        foreach (var f in Funcs)
        {
            if (f.Name == name)
                return f;
        }
        return null;
    }

    internal StructData? GetStructByName(string name)
    {
        foreach (var s in Structs)
        {
            if (s.Name == name)
                return s;
        }
        return null;
    }

    internal UnionData? GetUnionByName(string name)
    {
        foreach (var u in Unions)
        {
            if (u.Name == name)
                return u;
        }
        return null;
    }

    internal bool ExistConst(string name)
    {
        return Const.Contains(name);
    }

    internal ICodeData GetCodeData()
    {
        return this;
    }

    internal IHeaderData GetHeaderData()
    {
        return this;
    }

    internal void AddCodeData(ICodeData codeData)
    {
        Funcs.AddRange(codeData.Funcs);
        Structs.AddRange(codeData.Structs);
        Unions.AddRange(codeData.Unions);
        Typedef.AddRange(codeData.Typedef);
        Const.AddRange(codeData.Const);
        foreach (var globalVar in codeData.GlobalVars)
            GlobalVars.TryAdd(globalVar.Key, globalVar.Value);
        foreach (var macro in codeData.Macros)
            Macros.TryAdd(macro.Key, macro.Value);
    }

    internal void AddHeaderData(IHeaderData headerData)
    {
        Structs.AddRange(headerData.Structs);
        Unions.AddRange(headerData.Unions);
        Typedef.AddRange(headerData.Typedef);
        Const.AddRange(headerData.Const);
        foreach (var macro in headerData.Macros)
            Macros.TryAdd(macro.Key, macro.Value);
    }
}
