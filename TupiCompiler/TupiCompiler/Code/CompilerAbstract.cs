using TupiCompiler.Data;

namespace TupiCompiler.Code;

abstract internal class CompilerAbstract
{
    protected string TupiCode { get; set; }
    protected ReadOnlyData ReadonlyData { get; set; }
    protected RunData RunData { get; set; }
    protected CompiledCode CompiledCode { get; set; }

    public event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    public event EventHandler<CompilerArgs>? CompilerEvent;

    public string Start()
    {
        string[] tupiCodeLines;
        tupiCodeLines = PreCompilerCode(TupiCode);
        return CompilerCode(tupiCodeLines);
    }

    protected string[] PreCompilerCode(string code)
    {
        PreCompilerArgs preCompilerArgs = new(code);
        PreCompilerEvent?.Invoke(this, preCompilerArgs);

        var codeLines = preCompilerArgs.Code.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var macro in preCompilerArgs.Macros)
            RunData.Macros.TryAdd(macro.Key, macro.Value);
        return codeLines;
    }

    protected string CompilerCode(string[] tupiCodeLines)
    {
        CompilerArgs compilerArgs = new(tupiCodeLines, RunData, CompiledCode, ReadonlyData, false);
        CompilerEvent?.Invoke(this, compilerArgs);
        return CompiledCode.CreateAsmCode();
    }

    public RunData GetRunData()
    {
        return RunData;
    }
}