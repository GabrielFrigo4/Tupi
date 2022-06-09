namespace TupiCompiler.Data;

internal interface IHeaderData
{
    List<StructData> Structs { get; }
    List<UnionData> Unions { get; }
}
