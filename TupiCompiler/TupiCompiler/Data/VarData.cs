namespace TupiCompiler.Data;
internal class VarData
{
    internal string Name { get; private set; }
    internal string Type { get; private set; }
    internal int Size { get; private set; }

    internal VarData(string type, int size)
    {
        Name = string.Empty;
        Type = type;
        Size = size;
    }

    internal VarData(string name, string type, int size)
    {
        Name = name;
        Type = type;
        Size = size;
    }
}

