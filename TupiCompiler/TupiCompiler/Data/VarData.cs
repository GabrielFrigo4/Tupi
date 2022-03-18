namespace TupiCompiler.Data;
internal class VarData
{
    internal string Type { get; private set; }
    internal int Size { get; private set; }

    internal VarData(string type, int size)
    {
        Type = type;
        Size = size;
    }
}

