namespace TupiCompiler.Data;
internal class RunData
{
    internal List<FuncData> Funcs { get; private set; }
    internal List<StructData> Structs { get; private set; }
    internal List<UnionData> Unions { get; private set; }

    /// <summary>
    /// string = name
    /// VarData = varData
    /// </summary>
    internal Dictionary<string, VarData> GlobalVars { get; set; }

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
}
