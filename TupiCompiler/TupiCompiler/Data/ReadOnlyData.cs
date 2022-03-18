namespace TupiCompiler.Data;

internal class ReadOnlyData
{
    internal string[] Registors8i { get; private set; }
    internal string[] Registors16i { get; private set; }
    internal string[] Registors32i { get; private set; }
    internal string[] Registors64i { get; private set; }
    internal string[][] RegistorsAll { get; private set; }
    internal string[] AsmTypes { get; private set; }
    internal string[] TupiTypes { get; private set; }
    internal int[] TupiTypeSize { get; private set; }

    public ReadOnlyData()
    {
        Registors8i = new string[] { "cl", "dl", "r8b", "r9b" };
        Registors16i = new string[] { "cx", "dx", "r8w", "r9w" };
        Registors32i = new string[] { "ecx", "edx", "r8d", "r9d" };
        Registors64i = new string[] { "rcx", "rdx", "r8", "r9" };
        RegistorsAll = new string[][] { Registors8i, Registors16i, Registors32i, Registors64i };
        AsmTypes = new string[] { "byte", "word", "dword", "qword", "real4", "real8" };
        TupiTypes = new string[] { "i8", "i16", "i32", "i64", "f32", "f64" };
        TupiTypeSize = new int[] { 1, 2, 4, 8, 4, 8 };
    }
}
