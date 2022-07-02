namespace TupiCompiler.Data;
internal class VarData
{
    internal string Name { get; private set; }
    internal string Type { get; private set; }
    internal int Size { get; private set; }
    internal string Def { get; private set; }
    internal bool Ref { get; private set; }

    internal VarData(string name, string type, int size, bool @ref)
    {
        Name = name;
        Type = type;
        Size = size;
        Def = string.Empty;
        Ref = @ref;
    }

    internal VarData(string name, string type, int size, string def, bool @ref)
    {
        Name = name;
        Type = type;
        Size = size;
        Def = def;
        Ref = @ref;
    }
}

