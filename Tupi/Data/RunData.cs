namespace Tupi.Compiler;

internal struct RunData
{
    internal bool dotData;
    internal bool dotCode;
    internal bool endCode;
    internal bool endLocalVarsDefine;
    internal List<string> funcs;
    internal List<string> localVarsDefine;

    /// <summary>
    /// string = func_name/global
    /// Dictionary<string, string> = name, type
    /// </summary>
    internal Dictionary<string, Dictionary<string, string>> vars;

    public RunData()
    {
        this.dotData = false;
        this.dotCode = false;
        this.endCode = false;
        this.endLocalVarsDefine = true;
        this.funcs = new List<string>();
        this.localVarsDefine = new List<string>();
        this.vars = new Dictionary<string, Dictionary<string, string>>();
    }
}
