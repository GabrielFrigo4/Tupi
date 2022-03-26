namespace TupiCompiler.Data;
internal class StructData
{
    internal string Name { get; private set; }
    internal List<VarData> Vars { get; private set; }

    internal StructData(string name)
    {
        Name = name;
        Vars = new List<VarData>();
    }
}