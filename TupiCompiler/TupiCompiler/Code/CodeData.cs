namespace TupiCompiler.Code;
internal class CodeData
{
    internal List<string> UseFn { get; private set; }
    internal List<string> Struct { get; private set; }
    internal List<string> Union { get; private set; }
    internal List<string> GlobalVar { get; private set; }
    internal List<string> Func { get; private set; }

    internal CodeData()
    {
        UseFn = new List<string>();
        Struct = new List<string>();
        Union = new List<string>();
        GlobalVar = new List<string>();
        Func = new List<string>();
    }

    internal string CreateAsmCode()
    {
        string code = string.Empty;
        foreach(var usefn in UseFn)
        {
            code += usefn + "\n";
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
        code += ".code\n";
        foreach (var func in Func)
        {
            code += func + "\n";
        }
        code += "End";
        return code;
    }
}
