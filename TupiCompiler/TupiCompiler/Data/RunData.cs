namespace TupiCompiler.Data;

internal class RunData
{
    internal bool DotData { get; set; }
    internal bool DotCode { get; set; }
    internal bool EndLocalVarsDefine { get; set; }
    internal FuncData? CurrentFunc { get; set; }
    internal StructData? CurrentStruct { get; set; }
    internal List<FuncData> Funcs { get; private set; }
    internal List<StructData> Structs { get; private set; }

    /// <summary>
    /// string = name
    /// VarData = varData
    /// </summary>
    internal Dictionary<string, VarData> GlobalVars { get; set; }

    internal RunData()
    {
        this.DotData = false;
        this.DotCode = false;
        this.EndLocalVarsDefine = true;
        this.CurrentFunc = null;
        this.CurrentStruct = null;
        this.Funcs = new List<FuncData>();
        this.Structs = new List<StructData>();
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
