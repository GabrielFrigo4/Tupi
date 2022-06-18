namespace TupiCompiler.Data;

internal interface IHeaderData
{
    List<TypedefData> Typedef { get; }
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
    Dictionary<string, string> Macros { get; }
}
