using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal class CompilerArgs : EventArgs
{
    internal CodeData CodeData { get; private set; }
    internal string[] Lines { get; private set; }
    internal RunData RunData { get; private set; }
    internal ReadOnlyData ReadOnlyData { get; private set; }

    internal CompilerArgs(string[] lines, RunData runData, CodeData codeData, ReadOnlyData readOnlyData)
    {
        this.CodeData = codeData;
        this.Lines = lines;
        this.RunData = runData;
        this.ReadOnlyData = readOnlyData;
    }

    internal string[] GetTermsLine(string line)
    {
        return line.Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
