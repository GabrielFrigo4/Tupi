using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal interface ICompiler
{
    List<string> LinkLibs { get; }
    public event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    public event EventHandler<CompilerArgs>? CompilerEvent;

    string Start();
    RunData GetRunData();
}

internal interface ICompilerCode: ICompiler
{
    bool IsMainFile { get; }
    void SetCompilerFunc(ICompilerCodeFunc compilerCodeFunc);
}

internal interface ICompilerHeader: ICompiler
{
    void SetCompilerFunc(ICompilerHeaderFunc compilerCodeFunc);
}