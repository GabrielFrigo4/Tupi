namespace TupiCompiler.Data;
internal class FuncData
{
    internal string Name { get; private set; }
    internal int ShadowSpace { get; set; }
    internal List<VarData> LocalVars { get; private set; }
    internal List<VarData> Args { get; private set; }

    internal FuncData(string name)
    {
        Name = name;
        ShadowSpace = 32;
        LocalVars = new List<VarData>();
        Args = new List<VarData>();
    }

    internal VarData? GetLocalVarByName(string name)
    {
        foreach(var v in LocalVars)
        {
            if(v.Name == name)
                return v;
        }
        return null;
    }

    internal VarData? GetArgByName(string name)
    {
        foreach (var v in Args)
        {
            if (v.Name == name)
                return v;
        }
        return null;
    }
}
