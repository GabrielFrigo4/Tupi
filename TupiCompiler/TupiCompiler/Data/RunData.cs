namespace TupiCompiler.Data;

internal class RunData
{
    internal bool DotData { get; set; }
    internal bool DotCode { get; set; }
    internal bool EndLocalVarsDefine { get; set; }
    internal List<FuncData> Funcs { get; private set; }
    internal List<StructData> Structs { get; private set; }

    /// <summary>
    /// string = name
    /// VarData = varData
    /// </summary>
    internal Dictionary<string, VarData> GlobalVars { get; set; }

    public RunData()
    {
        this.DotData = false;
        this.DotCode = false;
        this.EndLocalVarsDefine = true;
        this.Funcs = new List<FuncData>();
        this.Structs = new List<StructData>();
        this.GlobalVars = new Dictionary<string, VarData>();
    }
}
