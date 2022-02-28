using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tupi.Data;

internal struct ReadonlyData
{
    internal readonly string[] Registors8i;
    internal readonly string[] Registors16i;
    internal readonly string[] Registors32i;
    internal readonly string[] Registors64i;
    internal readonly string[][] RegistorsAll;
    internal readonly string[] AsmTypes;
    internal readonly string[] TupiTypes;

    public ReadonlyData()
    {
        Registors8i = new string[] { "cl", "dl", "r8b", "r9b" };
        Registors16i = new string[] { "cx", "dx", "r8w", "r9w" };
        Registors32i = new string[] { "ecx", "edx", "r8d", "r9d" };
        Registors64i = new string[] { "rcx", "rdx", "r8", "r9" };
        RegistorsAll = new string[][] { Registors8i, Registors16i, Registors32i, Registors64i };
        AsmTypes = new string[] { "byte", "word", "dword", "qword", "real4", "real8" };
        TupiTypes = new string[] { "i8", "i16", "i32", "i64", "f32", "f64" };
    }
}
