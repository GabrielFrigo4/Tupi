using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal class CompilerArgs : EventArgs
{
    internal CodeCompiled CodeCompiled { get; private set; }
    internal string[] Lines { get; private set; }
    internal RunData RunData { get; private set; }
    internal ReadOnlyData ReadOnlyData { get; private set; }

    internal CompilerArgs(string[] lines, RunData runData, CodeCompiled codeCompiled, ReadOnlyData readOnlyData)
    {
        this.CodeCompiled = codeCompiled;
        this.Lines = lines;
        this.RunData = runData;
        this.ReadOnlyData = readOnlyData;
    }

    internal string[] GetTermsLine(string line)
    {
        return line.Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
