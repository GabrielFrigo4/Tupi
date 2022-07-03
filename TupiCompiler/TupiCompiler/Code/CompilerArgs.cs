using TupiCompiler.Data;

namespace TupiCompiler.Code;

internal class CompilerArgs : EventArgs
{
    internal CompiledCode CompiledCode { get; private set; }
    internal string[] Lines { get; private set; }
    internal RunData RunData { get; private set; }
    internal ReadOnlyData ReadOnlyData { get; private set; }
    internal bool IsHeader { get; private set; }

    internal CompilerArgs(string[] lines, RunData runData, CompiledCode compiledCode, ReadOnlyData readOnlyData, bool isHeader)
    {
        CompiledCode = compiledCode;
        Lines = lines;
        RunData = runData;
        ReadOnlyData = readOnlyData;
        IsHeader = isHeader;
    }

    internal string[] GetTermsLine(string line)
    {
        return line.Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }

    internal void SetLines(string[] newLines)
    {
        Lines = newLines;
    }
}
