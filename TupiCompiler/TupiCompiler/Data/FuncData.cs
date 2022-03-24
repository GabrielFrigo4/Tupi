namespace TupiCompiler.Data;
internal class FuncData
{
    internal string Name { get; private set; }
    internal int ShadowSpace { get; set; }
    internal List<VarData> LocalVar { get; private set; }
    internal List<VarData> Args { get; private set; }

    internal FuncData(string name)
    {
        Name = name;
        ShadowSpace = 32;
        LocalVar = new List<VarData>();
        Args = new List<VarData>();
    }

    internal VarData? GetDataByName(string name)
    {
        foreach(var v in LocalVar)
        {
            if(v.Name == name)
                return v;
        }
        return null;
    }
}
