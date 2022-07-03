using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal interface ICompilerCodeFunc
{
    void PreCompilerEventAdd(ref ICompilerCode compiler);

    void CompilerEventAdd(ref ICompilerCode compiler);
}

internal interface ICompilerHeaderFunc
{
    void PreCompilerEventAdd(ref ICompilerHeader compiler);

    void CompilerEventAdd(ref ICompilerHeader compiler);
}