using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal interface ICompilerCode
{
    public event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    public event EventHandler<CompilerArgs>? CompilerEvent;

    string Start();
    RunData GetRunData();
    void SetCompilerFunc(ICompilerCodeFunc compilerCodeFunc);
}

internal interface ICompilerHeader
{
    public event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    public event EventHandler<CompilerArgs>? CompilerEvent;

    string Start();
    RunData GetRunData();
    void SetCompilerFunc(ICompilerHeaderFunc compilerCodeFunc);
}