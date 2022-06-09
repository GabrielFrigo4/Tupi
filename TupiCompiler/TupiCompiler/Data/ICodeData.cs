namespace TupiCompiler.Data;

internal interface ICodeData
{
    List<FuncData> Funcs { get; }
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
    Dictionary<string, VarData> GlobalVars { get; }
}
