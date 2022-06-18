namespace TupiCompiler.Data;
internal class TypedefData
{
    internal string Name { get; private set; }
    internal int Size { get; private set; }

    internal TypedefData(string name, int size)
    {
        Name = name;
        Size = size;
    }
}
