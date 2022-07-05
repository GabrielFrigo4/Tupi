using TupiCompiler.Code;

namespace TupiCompiler.Data;
internal class ReadOnlyData
{
    internal string[] Registors8i { get; private set; }
    internal string[] Registors16i { get; private set; }
    internal string[] Registors32i { get; private set; }
    internal string[] Registors64i { get; private set; }
    internal string[] RegistorsRealX { get; private set; }
    internal string[] RegistorsRealY { get; private set; }
    internal string[] RegistorsB { get; private set; }
    internal string[] RegistorsA { get; private set; }
    internal string[][] RegistorsAll { get; private set; }
    internal string[] AsmTypes { get; private set; }
    internal string[] DefAsmTypes { get; private set; }
    internal string[] TupiTypes { get; private set; }
    internal int[] TypeSize { get; private set; }

    public ReadOnlyData(Architecture architecture)
    {
        Registors8i = new string[] { "cl", "dl", "r8b", "r9b" };
        Registors16i = new string[] { "cx", "dx", "r8w", "r9w" };
        Registors32i = new string[] { "ecx", "edx", "r8d", "r9d" };
        Registors64i = new string[] { "rcx", "rdx", "r8", "r9" };
        RegistorsRealX = new string[] { "xmm0", "xmm1", "xmm2", "xmm3" };
        RegistorsRealY = new string[] { "ymm0", "ymm1", "ymm2", "ymm3" };
        RegistorsB = new string[] { "bl", "bx", "ebx", "rbx" };
        RegistorsA = new string[] { "al", "ax", "eax", "rax" };
        RegistorsAll = new string[][] { Registors8i, Registors16i, Registors32i, Registors64i };
        AsmTypes = new string[] { "byte", "word", "dword", "qword", "real4", "real8" };
        DefAsmTypes = new string[] { "db", "dw", "dd", "dq", "dd", "dq" };
        TupiTypes = new string[] { "i8", "i16", "i32", "i64", "f32", "f64", "iptr" };

        int iptrSize = 8;
        switch (architecture)
        {
            case Architecture.X86_64:
                iptrSize = 8;
                break;
            case Architecture.X86:
                iptrSize = 4;
                break;
            case Architecture.X86_16:
                iptrSize = 2;
                break;
        }
        TypeSize = new int[] { 1, 2, 4, 8, 4, 8, iptrSize };
    }
}
