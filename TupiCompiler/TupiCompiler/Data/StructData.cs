namespace TupiCompiler.Data;
internal class StructData
{
    internal string Name { get; private set; }
    internal int Size { get; set; }
    internal bool IsCStruct { get; private set; }
    internal List<Tuple<int, int>> CStructSpaces { get; private set; }
    internal List<VarData> Vars { get; private set; }

    internal StructData(string name, bool isCStruct)
    {
        Name = name;
        Size = 0;
        Vars = new();
        IsCStruct = isCStruct;
        CStructSpaces = new();
    }

    internal StructData(string name, int size, bool isCStruct)
    {
        Name = name;
        Size = size;
        Vars = new();
        IsCStruct= isCStruct;
        CStructSpaces = new();
    }

    internal VarData? GetVarByName(string name)
    {
        foreach (var v in Vars)
        {
            if (v.Name == name)
                return v;
        }
        return null;
    }
}