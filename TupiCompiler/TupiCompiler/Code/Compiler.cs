using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal class Compiler
{
    internal event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    internal event EventHandler<CompilerArgs>? CompilerEvent;
    private readonly string tupiCode = string.Empty;
    private readonly ReadOnlyData readonlyData;
    private readonly RunData runData;
    private readonly CodeData codeData;
    private readonly bool isHeader;

    internal Compiler(string tupiCodePath, bool isHeader = false)
    {
        tupiCode = File.ReadAllText(tupiCodePath);
        readonlyData = new ReadOnlyData();
        runData = new RunData();
        codeData = new CodeData();
        this.isHeader = isHeader;
    }

    internal string Start()
    {
        string[] tupiCodeLines;
        tupiCodeLines = PreCompilerCode(tupiCode);
        return CompilerCode(tupiCodeLines);
    }

    private string[] PreCompilerCode(string code)
    {
        PreCompilerArgs preCompilerArgs = new PreCompilerArgs(code);
        PreCompilerEvent?.Invoke(this, preCompilerArgs);

        var codeLines = preCompilerArgs.Code.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
        return codeLines;
    }

    private string CompilerCode(string[] tupiCodeLines)
    {
        CompilerArgs compilerArgs = new CompilerArgs(tupiCodeLines, runData, codeData, readonlyData);
        CompilerEvent?.Invoke(this, compilerArgs);
        return codeData.CreateAsmCode(isHeader);
    }
}
