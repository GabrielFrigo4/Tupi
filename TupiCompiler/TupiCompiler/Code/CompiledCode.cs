namespace TupiCompiler.Code;
internal class CompiledCode
{
    internal List<string> UseTh { get; private set; }
    internal List<string> UseFn { get; private set; }
    internal List<string> Struct { get; private set; }
    internal List<string> Union { get; private set; }
    internal List<string> Typedef { get; private set; }
    internal List<string> GlobalVar { get; private set; }
    internal List<string> Func { get; private set; }
    private bool IsHeader { get; set; }

    internal CompiledCode(bool isHeader)
    {
        UseFn = new();
        UseTh = new();
        Struct = new();
        Union = new();
        Typedef = new();
        GlobalVar = new();
        Func = new();
        IsHeader = isHeader;
    }

    internal string CreateAsmCode()
    {
        string code = string.Empty;
        foreach (var useth in UseTh)
        {
            code += useth + "\n";
        }
        foreach (var usefn in UseFn)
        {
            code += usefn + "\n";
        }
        foreach (var @struct in Struct)
        {
            code += @struct + "\n";
        }
        foreach (var union in Union)
        {
            code += union + "\n";
        }
        foreach (var typedef in Typedef)
        {
            code += typedef + "\n";
        }
        code += ".data\n";
        foreach (var globalVar in GlobalVar)
        {
            code += globalVar + "\n";
        }
        if (!IsHeader)
        {
            code += ".code\n";
            foreach (var func in Func)
            {
                code += func + "\n";
            }
            code += "end";
        }
        return code;
    }
}
