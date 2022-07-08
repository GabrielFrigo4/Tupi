using TupiCompiler.Data;

namespace TupiCompiler.Code;

abstract internal class CompilerAbstract
{
    private Func<string>? createCode = null;
    private string? tupiCode = null;
    private ReadOnlyData? readonlyData = null;
    private RunData? runData = null;
    private CompiledCode? compiledCode = null;

    protected Func<string> CreateCode {
        get
        {
            if (createCode is not null)
                return createCode;
            else
                throw new Exception("CreateCode not created");
        }
        set
        {
            createCode = value;
        }
    }
    protected string TupiCode
    {
        get
        {
            if (tupiCode is not null)
                return tupiCode;
            else
                throw new Exception("TupiCode not created");
        }
        set
        {
            tupiCode = value;
        }
    }
    protected ReadOnlyData ReadonlyData
    {
        get
        {
            if (readonlyData is not null)
                return readonlyData;
            else
                throw new Exception("ReadonlyData not created");
        }
        set
        {
            readonlyData = value;
        }
    }
    protected RunData RunData
    {
        get
        {
            if (runData is not null)
                return runData;
            else
                throw new Exception("RunData not created");
        }
        set
        {
            runData = value;
        }
    }
    protected CompiledCode CompiledCode
    {
        get
        {
            if (compiledCode is not null)
                return compiledCode;
            else
                throw new Exception("CompiledCode not created");
        }
        set
        {
            compiledCode = value;
        }
    }

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
        return CreateCode.Invoke();
    }

    public RunData GetRunData()
    {
        return RunData;
    }
}