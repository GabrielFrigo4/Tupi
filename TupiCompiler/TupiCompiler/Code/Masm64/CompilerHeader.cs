using TupiCompiler.Data;

namespace TupiCompiler.Code.Masm64;

internal class CompilerHeader : CompilerAbstract, ICompilerHeader
{
    internal CompilerHeader(string tupiCodePath)
    {
        TupiCode = File.ReadAllText(tupiCodePath);
        ReadonlyData = new(Architecture.X86_64);
        RunData = new();
        CodeCompiled = new(true);
    }
}
