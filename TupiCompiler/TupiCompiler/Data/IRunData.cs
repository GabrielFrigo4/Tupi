namespace TupiCompiler.Data;

internal interface IHeaderData
{
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
    List<TypedefData> Typedef { get; }
    List<string> Const { get; }
    Dictionary<string, string> Macros { get; }
}

internal interface ICodeData
{
    List<FuncData> Funcs { get; }
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
    List<TypedefData> Typedef { get; }
    Dictionary<string, VarData> GlobalVars { get; }
    List<string> Const { get; }
    Dictionary<string, string> Macros { get; }
}
