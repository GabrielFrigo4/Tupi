using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal interface ICompilerHeader
{
    public event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    public event EventHandler<CompilerArgs>? CompilerEvent;

    string Start();
    RunData GetRunData();
}