using TupiCompiler.Data;

namespace TupiCompiler.Code.Masm64;

internal class CompilerCode : CompilerAbstract, ICompilerCode
{
    internal CompilerCode(string tupiCodePath, bool isMainFile)
    {
        TupiCode = File.ReadAllText(tupiCodePath);
        ReadonlyData = new(Architecture.X86_64);
        RunData = new();
        CompiledCode = new();
        IsMainFile = isMainFile;
        CreateCode = delegate
        {
            return CompiledCode.CreateAsmCode(isMainFile);
        };
    }

    public bool IsMainFile { get; private set; }

    public void SetCompilerFunc(ICompilerCodeFunc compilerCodeFunc)
    {
        ICompilerCode compiler = this;
        compilerCodeFunc.PreCompilerEventAdd(ref compiler);
        compilerCodeFunc.CompilerEventAdd(ref compiler);
    }
}
