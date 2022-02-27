using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tupi;

internal struct TupiData
{
    internal readonly string[] registors_8i;
    internal readonly string[] registors_16i;
    internal readonly string[] registors_32i;
    internal readonly string[] registors_64i;
    internal readonly string[][] registors_all;
    internal readonly string[] asm_types;
    internal readonly string[] tupi_types;

    public TupiData()
    {
        registors_8i = new string[] { "cl", "dl", "r8b", "r9b" };
        registors_16i = new string[] { "cx", "dx", "r8w", "r9w" };
        registors_32i = new string[] { "ecx", "edx", "r8d", "r9d" };
        registors_64i = new string[] { "rcx", "rdx", "r8", "r9" };
        registors_all = new string[][] { registors_8i, registors_16i, registors_32i, registors_64i };
        asm_types = new string[] { "byte", "word", "dword", "qword", "real4", "real8" };
        tupi_types = new string[] { "i8", "i16", "i32", "i64", "f32", "f64" };
    }
}
