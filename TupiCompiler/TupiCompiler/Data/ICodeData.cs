namespace TupiCompiler.Data;

internal interface ICodeData
{
    List<TypedefData> Typedef { get; }
    List<FuncData> Funcs { get; }
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
    Dictionary<string, VarData> GlobalVars { get; }
    Dictionary<string, string> Macros { get; }
}
