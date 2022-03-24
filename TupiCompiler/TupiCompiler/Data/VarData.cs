namespace TupiCompiler.Data;
internal class VarData
{
    internal string Name { get; private set; }
    internal string Type { get; private set; }
    internal int Size { get; private set; }
    internal string Def { get; private set; }

    internal VarData(string name, string type, int size)
    {
        Name = name;
        Type = type;
        Size = size;
        Def = string.Empty;
    }

    internal VarData(string name, string type, int size, string def)
    {
        Name = name;
        Type = type;
        Size = size;
        Def = def;
    }
}

