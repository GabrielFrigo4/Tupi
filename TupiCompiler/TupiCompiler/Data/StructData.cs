namespace TupiCompiler.Data;
internal class StructData
{
    internal string Name { get; private set; }
    internal List<VarData> VarData { get; private set; }

    internal StructData(string name)
    {
        Name = name;
        VarData = new List<VarData>();
    }
}