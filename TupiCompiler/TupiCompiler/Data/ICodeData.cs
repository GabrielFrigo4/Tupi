namespace TupiCompiler.Data;

internal interface ICodeData
{
    List<FuncData> Funcs { get; }
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
    List<TypedefData> Typedef { get; }
    Dictionary<string, VarData> GlobalVars { get; }
    Dictionary<string, string> Macros { get; }
}
