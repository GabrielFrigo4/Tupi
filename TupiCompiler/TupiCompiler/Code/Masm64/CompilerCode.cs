using TupiCompiler.Data;

namespace TupiCompiler.Code.Masm64;

internal class CompilerCode : CompilerAbstract, ICompilerCode
{
    internal CompilerCode(string tupiCodePath)
    {
        TupiCode = File.ReadAllText(tupiCodePath);
        ReadonlyData = new(Architecture.X86_64);
        RunData = new();
        CompiledCode = new(false);
    }

    public void SetCompilerFunc(ICompilerCodeFunc compilerCodeFunc)
    {
        ICompilerCode compiler = this;
        compilerCodeFunc.PreCompilerEventAdd(ref compiler);
        compilerCodeFunc.CompilerEventAdd(ref compiler);
    }
}
