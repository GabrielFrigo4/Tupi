namespace TupiCompiler.Code;
internal class CodeCompiled
{
    internal List<string> UseFn { get; private set; }
    internal List<string> UseTh { get; private set; }
    internal List<string> Struct { get; private set; }
    internal List<string> Union { get; private set; }
    internal List<string> GlobalVar { get; private set; }
    internal List<string> Func { get; private set; }

    internal CodeCompiled()
    {
        UseFn = new List<string>();
        UseTh = new List<string>();
        Struct = new List<string>();
        Union = new List<string>();
        GlobalVar = new List<string>();
        Func = new List<string>();
    }

    internal string CreateAsmCode(bool isHeader = false)
    {
        string code = string.Empty;
        foreach(var usefn in UseFn)
        {
            code += usefn + "\n";
        }
        foreach (var useth in UseTh)
        {
            code += useth + "\n";
        }
        code += ".data\n";
        foreach (var @struct in Struct)
        {
            code += @struct + "\n";
        }
        foreach (var union in Union)
        {
            code += union + "\n";
        }
        foreach (var globalVar in GlobalVar)
        {
            code += globalVar + "\n";
        }
        if (!isHeader)
        {
            code += ".code\n";
            foreach (var func in Func)
            {
                code += func + "\n";
            }
            code += "End";
        }
        return code;
    }
}
