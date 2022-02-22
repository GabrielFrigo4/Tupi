namespace Tupi
{
    internal struct CompileData
    {
        public bool dotData;
        public bool dotCode;
        public bool endCode;
        public List<string> funcs;

        public CompileData()
        {
            this.dotData = false;
            this.dotCode = false;
            this.endCode = false;
            this.funcs = new List<string>();
        }
    }
}
