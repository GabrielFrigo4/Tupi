namespace TupiCompiler.Data;
internal class FuncData
{
    internal string Name { get; private set; }
    internal int ShadowSpace { get; set; }
    internal List<VarData> VarData { get; private set; }

    internal FuncData(string name)
    {
        Name = name;
        ShadowSpace = 32;
        VarData = new List<VarData>();
    }
}
