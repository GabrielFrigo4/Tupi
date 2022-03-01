namespace TupiCompiler.Code
{
    internal class PreCompilerArgs : EventArgs
    {
        internal string Code { get; set; }

        internal PreCompilerArgs(string code)
        {
            this.Code = code;
        }
    }
}
