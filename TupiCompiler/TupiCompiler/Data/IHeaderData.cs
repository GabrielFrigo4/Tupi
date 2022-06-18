namespace TupiCompiler.Data;

internal interface IHeaderData
{
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
    List<TypedefData> Typedef { get; }
    Dictionary<string, string> Macros { get; }
}
