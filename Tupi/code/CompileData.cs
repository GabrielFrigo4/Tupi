namespace Tupi
{
    internal struct CompileData
    {
        public bool dotData;
        public bool dotCode;
        public bool endCode;
        public List<string> funcs;

        /// <summary>
        /// string = func_name/global
        /// Dictionary<string, string> = type, name
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> vars;

        public CompileData()
        {
            this.dotData = false;
            this.dotCode = false;
            this.endCode = false;
            this.funcs = new List<string>();
            this.vars = new Dictionary<string, Dictionary<string, string>>();
        }
    }
}
