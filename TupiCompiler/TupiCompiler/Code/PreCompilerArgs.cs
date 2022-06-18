namespace TupiCompiler.Code
{
    internal class PreCompilerArgs : EventArgs
    {
        internal string Code { get; set; }
        internal Dictionary<string, string> Macros { get; private set; }

        internal PreCompilerArgs(string code)
        {
            this.Code = code;
            Macros = new();
        }
    }
}
