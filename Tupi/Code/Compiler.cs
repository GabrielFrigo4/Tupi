using Tupi.Data;

namespace Tupi.Code;

internal class Compiler
{
    internal event EventHandler<CompilerLinesArgs>? CompilerLoopLines;
    private readonly string tupiCode = string.Empty;
    private readonly ReadOnlyData readonlyData;
    private string[] tupiCodeLines;
    private RunData runData;

    internal Compiler(string tupiCodePath)
    {
        tupiCode = File.ReadAllText(tupiCodePath);
        tupiCodeLines = tupiCode.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        readonlyData = new ReadOnlyData();
        runData = new RunData();
    }

    internal string Start()
    {
        string asmCode = string.Empty;
        runData.Vars.Add(string.Empty, new Dictionary<string, string>());

        for (int l = 0; l < tupiCodeLines.Length; l++)
        {
            CompilerLinesArgs compilerLinesArgs = new CompilerLinesArgs(tupiCodeLines, tupiCodeLines[l], l, runData, readonlyData);
            CompilerLoopLines?.Invoke(this, compilerLinesArgs);

            runData = compilerLinesArgs.RunData;

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
