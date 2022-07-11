using System.CommandLine;
using System.Diagnostics;
using System.Reflection;

namespace TupiCompiler.Code;
internal static class Program
{
    internal static ICompilerCode? mainCompiler;

    internal static ICompilerCode MainCompiler
    {
        get
        {
            if (mainCompiler is null)
            {
                throw new Exception("MainCompiler not create");
            }
            else
            {
                return mainCompiler;
            }
        }
    }

    internal static readonly string? exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
    internal static string EXE_PATH
    {
        get
        {
            if (exePath is null)
            {
                return string.Empty;
            }
            else
            {
                return exePath;
            }
        }
    }

    internal readonly static string
        x64Path = EXE_PATH + "/_tupi/x64/",
        x86Path = EXE_PATH + "/_tupi/x86/",
        thPath = EXE_PATH + "/_tupi/header/",
        tpPath = EXE_PATH + "/_tupi/code/",
        pathDir = "./build";

    internal static string pathCompile = string.Empty;

    static int Main(string[] args)
    {
#if DEBUG
        args = new string[1];
        args[0] = "TupiCode/mycode.tp";
#endif
        if (args.Length > 0)
        {
            string? _pathCompile = Path.GetDirectoryName(Path.GetFullPath(args[0]));
            if (_pathCompile is not null)
                pathCompile = Path.GetFullPath(_pathCompile);

            Action<string, bool, bool, bool> action = CompileTupiProj;
            Argument<string> source = new("source", "source for tupi compile");
            Option<bool> exe = new(new[] { "--exe", "-e" }, "is a exe output file");
            Option<bool> dll = new(new[] { "--dll", "-d" }, "is a dll output file");
            Option<bool> lib = new(new[] { "--lib", "-l" }, "is a lib output file");
            RootCommand cmd = new()
            {
                source,
                exe,
                dll,
                lib,
            };
            cmd.SetHandler(action, source, exe, dll, lib);
            return cmd.Invoke(args);
        }
        else
        {
            Console.WriteLine("falta o comando ou argumento...");
            return 0;
        }
    }

    internal static void CompileTupiProj(string pathTupi, bool isExe, bool isDll, bool isLib)
    {
        OutputType outputType = OutputType.Exe;
        if(isExe)
            outputType = OutputType.Exe;
        else if (isDll)
            outputType = OutputType.Dll;
        else if (isLib)
            outputType = OutputType.Lib;
        string tupiFileName = Path.GetFileNameWithoutExtension(pathTupi);
        Console.WriteLine("compile tupi proj:");
        Console.WriteLine("tranform tupi code to assembly(masm)");

        Directory.CreateDirectory(pathDir);
        StreamWriter write = File.CreateText(pathDir + $"\\{tupiFileName}.asm");
        write.Write(CompileTupiCodeFile(pathTupi, out mainCompiler, true));
        write.Close();

        List<string> files = new();
        files.Add(tupiFileName);
        if (!Directory.Exists(pathDir + "\\header"))
            Directory.CreateDirectory(pathDir + "\\header");
        if (File.Exists(pathDir + "\\header\\std_tupi_def.inc"))
            File.Delete(pathDir + "\\header\\std_tupi_def.inc");
        File.Copy($"{x64Path}std_tupi_def.inc", pathDir + "\\header\\std_tupi_def.inc");
        CompileAsm(pathDir, files, MainCompiler.LinkLibs, MainCompiler.FnExport, outputType);
    }

    internal static string CompileTupiCodeFile(string pathTupiCode, out ICompilerCode compiler, bool isMainFile)
    {
        compiler = new Masm64.CompilerCode(pathTupiCode, isMainFile);
        compiler.SetCompilerFunc(new CompilerFunc());
        return compiler.Start();
    }

    internal static string CompileTupiHeaderFile(string pathTupiCode, out ICompilerHeader compiler)
    {
        compiler = new Masm64.CompilerHeader(pathTupiCode);
        compiler.SetCompilerFunc(new CompilerFunc());
        return compiler.Start();
    }

    internal static void CompileAsm(string path_dir_asm, List<string> nameFiles, List<string> libFiles, 
        List<string> fnExport, OutputType output, bool run = false, bool assembler_warning = true)
    {
        Console.WriteLine("tranform assembly to binary file");
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            CreateNoWindow = !assembler_warning,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/C cd \"{path_dir_asm}\" && call \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvarsall.bat\" x64  &&"
        };
        foreach (string asmFile in nameFiles)
        {
            startInfo.Arguments += $" ml64 {asmFile}.asm /c &&";
        }

        if (output == OutputType.Lib)
        {
            string libCommand = " lib";
            foreach (string objFile in nameFiles)
            {
                libCommand += $" {objFile}.obj";

                {
                    var libFilesDistinct = libFiles.Distinct();
                    foreach (string libFile in libFilesDistinct)
                    {
                        libCommand += $" /nodefaultlib:{libFile}";
                    }
                }

                {
                    var fnExportDistinct = fnExport.Distinct();
                    foreach (string fnExp in fnExportDistinct)
                    {
                        libCommand += $" /export:{fnExp}";
                    }
                }

                startInfo.Arguments += libCommand;
            }
        }
        else
        {
            string linkCommand = " link";
            foreach (string objFile in nameFiles)
            {
                linkCommand += $" {objFile}.obj";
            }

            if (output == OutputType.Exe)
                linkCommand += $" /entry:main /subsystem:console";
            else if (output == OutputType.Dll)
                linkCommand += $" /DLL /entry:DllMain";

            {
                var libFilesDistinct = libFiles.Distinct();
                foreach (string libFile in libFilesDistinct)
                {
                    linkCommand += $" /defaultlib:{libFile}";
                }
            }

            {
                var fnExportDistinct = fnExport.Distinct();
                foreach (string fnExp in fnExportDistinct)
                {
                    linkCommand += $" /export:{fnExp}";
                }
            }

            startInfo.Arguments += linkCommand;
            if (run)
            {
                startInfo.Arguments += " && main";
            }
        }

        Console.WriteLine(startInfo.Arguments);
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        Console.WriteLine("compile finished!!");
    }
}

enum OutputType
{
    Exe,
    Dll,
    Lib,
}