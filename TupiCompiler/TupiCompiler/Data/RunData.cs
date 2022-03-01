namespace Tupi.Data;

internal class RunData
{
    internal bool DotData { get; set; }
    internal bool DotCode { get; set; }
    internal bool EndLocalVarsDefine { get; set; }
    internal List<string> Funcs { get; private set; }
    internal List<string> LocalVarsDefine { get; private set; }

    /// <summary>
    /// string = func_name/global
    /// Dictionary<string, string> = name, type
    /// </summary>
    internal Dictionary<string, Dictionary<string, string>> Vars { get; set; }

    public RunData()
    {
        this.DotData = false;
        this.DotCode = false;
        this.EndLocalVarsDefine = true;
        this.Funcs = new List<string>();
        this.LocalVarsDefine = new List<string>();
        this.Vars = new Dictionary<string, Dictionary<string, string>>();
    }
}
