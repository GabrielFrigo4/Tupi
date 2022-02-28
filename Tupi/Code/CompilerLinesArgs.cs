using Tupi.Data;

namespace Tupi.Code;

internal class CompilerLinesArgs : EventArgs
{
    internal string Line { get; set; }
    internal string[] Lines { get; private set; }
    internal int LinePos { get; private set; }
    internal string[] Terms { get; private set; }
    internal RunData RunData { get; set; }
    internal ReadOnlyData ReadOnlyData { get; private set; }

    internal CompilerLinesArgs(string[] lines, string line, int linePos, RunData runData, ReadOnlyData readOnlyData)
    {
        this.Lines = lines;
        this.Line = line;
        this.LinePos = linePos;
        this.Terms = line.Split(new char[] { '\r', '\t', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        this.RunData = runData;
        this.ReadOnlyData = readOnlyData;
    }
}
