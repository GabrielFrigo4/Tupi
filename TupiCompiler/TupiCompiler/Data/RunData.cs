namespace TupiCompiler.Data;
internal class RunData: ICodeData, IHeaderData
{
    public List<FuncData> Funcs { get; private set; }
    public List<StructData> Structs { get; private set; }
    public List<UnionData> Unions { get; private set; }
    public Dictionary<string, VarData> GlobalVars { get; private set; }

    internal RunData()
    {
        this.Funcs = new List<FuncData>();
        this.Structs = new List<StructData>();
        this.Unions = new List<UnionData>();
        this.GlobalVars = new Dictionary<string, VarData>();
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

    internal FuncData? GetFuncByName(string name)
    {
        foreach (var f in Funcs)
        {
            if (f.Name == name)
                return f;
        }
        return null;
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
        foreach(var globalVar in codeData.GlobalVars)
            GlobalVars.TryAdd(globalVar.Key, globalVar.Value);
    }

    internal void AddHeaderData(IHeaderData headerData)
    {
        Structs.AddRange(headerData.Structs);
        Unions.AddRange(headerData.Unions);
    }
}
