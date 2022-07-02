﻿using TupiCompiler.Data;

namespace TupiCompiler.Code.Masm64;

internal class Compiler : CompilerAbstract, ICompiler
{
    internal Compiler(string tupiCodePath)
    {
        TupiCode = File.ReadAllText(tupiCodePath);
        ReadonlyData = new(Architecture.X86_64);
        RunData = new();
        CodeCompiled = new(false);
    }
}
