﻿namespace TupiCompiler.Data;
internal class StructData
{
    internal string Name { get; private set; }
    internal int Size { get; set; }
    internal List<VarData> Vars { get; private set; }

    internal StructData(string name)
    {
        Name = name;
        Size = 0;
        Vars = new List<VarData>();
    }

    internal StructData(string name, int size)
    {
        Name = name;
        Size = size;
        Vars = new List<VarData>();
    }
}