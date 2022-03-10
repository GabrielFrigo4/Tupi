using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal class Compiler
{
    internal event EventHandler<PreCompilerArgs>? PreCompilerEvent;
    internal event EventHandler<CompilerArgs>? CompilerEvent;
    private readonly string tupiCode = string.Empty;
    private readonly ReadOnlyData readonlyData;
    private readonly RunData runData;

    internal Compiler(string tupiCodePath)
    {
        tupiCode = File.ReadAllText(tupiCodePath);
        readonlyData = new ReadOnlyData();
        runData = new RunData();
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

        var codeLines = preCompilerArgs.Code.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return codeLines;
    }

    private string CompilerCode(string[] tupiCodeLines)
    {
        string asmCode = string.Empty;
        runData.Vars.Add(string.Empty, new Dictionary<string, string>());

        for (int l = 0; l < tupiCodeLines.Length; l++)
        {
            CompilerArgs compilerLinesArgs = new CompilerArgs(tupiCodeLines, tupiCodeLines[l], l, runData, readonlyData);
            CompilerEvent?.Invoke(this, compilerLinesArgs);

            if (runData.DotCode && l == compilerLinesArgs.Lines.Length - 1)
            {
                compilerLinesArgs.Line += "\nEnd";
            }

            tupiCodeLines[l] = compilerLinesArgs.Line;
            tupiCodeLines[l] += "\n";
        }

        foreach (string line in tupiCodeLines)
        {
            asmCode += line;
        }

        return asmCode;
    }
}
