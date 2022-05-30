namespace TupiCompiler.Data;
internal class UnionData
{
    internal string Name { get; private set; }
    internal int Size { get; set; }
    internal List<VarData> Vars { get; private set; }

    internal UnionData(string name)
    {
        Name = name;
        Size = 0;
        Vars = new List<VarData>();
    }

    internal UnionData(string name, int size)
    {
        Name = name;
        Size = size;
        Vars = new List<VarData>();
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