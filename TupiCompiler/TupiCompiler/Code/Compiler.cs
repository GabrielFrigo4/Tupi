using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal class Compiler
{
    internal event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    internal event EventHandler<CompilerArgs>? CompilerEvent;
    private readonly string tupiCode = string.Empty;
    private readonly ReadOnlyData readonlyData;
    private readonly RunData runData;
    private readonly CodeCompiled codeCompiled;
    private readonly bool isHeader;
    private readonly Architecture architecture;

    internal Compiler(string tupiCodePath, bool isHeader = false, Architecture architecture = Architecture.X64)
    {
        tupiCode = File.ReadAllText(tupiCodePath);
        readonlyData = new(architecture);
        runData = new();
        codeCompiled = new();
        this.isHeader = isHeader;
        this.architecture = architecture;
    }

    internal string Start()
    {
        string[] tupiCodeLines;
        tupiCodeLines = PreCompilerCode(tupiCode);
        return CompilerCode(tupiCodeLines);
    }

    private string[] PreCompilerCode(string code)
    {
        PreCompilerArgs preCompilerArgs = new(code);
        PreCompilerEvent?.Invoke(this, preCompilerArgs);

        var codeLines = preCompilerArgs.Code.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var macro in preCompilerArgs.Macros)
            runData.Macros.TryAdd(macro.Key, macro.Value);
        return codeLines;
    }

    private string CompilerCode(string[] tupiCodeLines)
    {
        CompilerArgs compilerArgs = new(tupiCodeLines, runData, codeCompiled, readonlyData, isHeader);
        CompilerEvent?.Invoke(this, compilerArgs);
        return codeCompiled.CreateAsmCode(isHeader);
    }

    internal RunData GetRunData()
    {
        return runData;
    }
}
