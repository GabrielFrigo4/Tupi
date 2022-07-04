namespace TupiCompiler.Code;
internal class CompiledCode
{
    internal List<string> UseTh { get; private set; }
    internal List<string> UseTp { get; private set; }
    internal List<string> UseFn { get; private set; }
    internal List<string> Struct { get; private set; }
    internal List<string> Union { get; private set; }
    internal List<string> Typedef { get; private set; }
    internal List<string> GlobalVar { get; private set; }
    internal List<string> Func { get; private set; }

    internal CompiledCode()
    {
        UseFn = new();
        UseTp = new();
        UseTh = new();
        Struct = new();
        Union = new();
        Typedef = new();
        GlobalVar = new();
        Func = new();
    }

    internal string CreateAsmCode(bool isMainFile)
    {
        string code = string.Empty;
        foreach (var useth in UseTh)
        {
            code += useth + "\n";
        }
        foreach (var usetp in UseTp)
        {
            code += usetp + "\n";
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
        code += ".code\n";
        foreach (var func in Func)
        {
            code += func + "\n";
        }
        if(isMainFile)
            code += "end";
        return code;
    }

    internal string CreateIncCode()
    {
        string code = string.Empty;
        foreach (var useth in UseTh)
        {
            code += useth + "\n";
        }
        foreach (var usetp in UseTp)
        {
            code += usetp + "\n";
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
        return code;
    }
}
