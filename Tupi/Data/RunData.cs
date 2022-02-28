namespace Tupi.Data;

internal struct RunData
{
    internal bool DotData { get; private set; }
    internal bool DotCode { get; private set; }
    internal bool EndLocalVarsDefine { get; private set; }
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

    internal RunData SetDotData(bool dotData)
    {
        this.DotData = dotData;
        return this;
    }

    internal RunData SetDotCode(bool dotCode)
    {
        this.DotCode = dotCode;
        return this;
    }

    internal RunData SetEndLocalVarsDefine(bool endLocalVarsDefine)
    {
        this.EndLocalVarsDefine = endLocalVarsDefine;
        return this;
    }
}
