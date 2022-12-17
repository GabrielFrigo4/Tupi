using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        linkPath = EXE_PATH + "/.TupiCore/llvm/lld-link.exe",
        arPath = EXE_PATH + "/.TupiCore/llvm/llvm-ar.exe",
        nasmPath = EXE_PATH + "/.TupiCore/nasm/nasm.exe",
        x64Path = EXE_PATH + "/.TupiCore/tupi/x64/",
        x86Path = EXE_PATH + "/.TupiCore/tupi/x86/",
        thPath = EXE_PATH + "/.TupiCore/tupi/header/",
        tpPath = EXE_PATH + "/.TupiCore/tupi/code/",
        pathDir = "./build";

    internal static string pathCompile = string.Empty;

    internal static Config Config { get; set; }

    static int Main(string[] args)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build();
        Config = deserializer.Deserialize<Config>(File.ReadAllText("./Config.yaml"));
#if DEBUG
        List<string> listArgs = new();
        listArgs.Add("-s");
        listArgs.Add("TupiCode/mycode.tp");
        //listArgs.Add("-s");
        //listArgs.Add("TupiCode/exemples/tupiexe.tp");
        //listArgs.Add("-s");
        //listArgs.Add("TupiCode/exemples/tupilib.tp");
        listArgs.Add("-o");
        listArgs.Add("mycode.exe");
        listArgs.Add("-e");
        args = listArgs.ToArray();
#endif
        if (args.Length > 0)
        {
            Action<string[]?, bool, bool, bool, string?> action = CompileTupiProj;
            Option<string[]?> source = new(new[] { "--source", "-s" }, "source for tupi compile");
            Option<bool> exe = new(new[] { "--exe", "-e" }, "is a exe output file");
            Option<bool> dll = new(new[] { "--dll", "-d" }, "is a dll output file");
            Option<bool> lib = new(new[] { "--lib", "-l" }, "is a lib output file");
            Option<string?> output = new(new[] { "--output", "-o" }, "is a output file name");
            RootCommand cmd = new()
            {
                source,
                exe,
                dll,
                lib,
                output,
            };
            cmd.SetHandler(action, source, exe, dll, lib, output);
            return cmd.Invoke(args);
        }
        else
        {
            CompilerFunc.ShowErrors("falta o comando ou argumento...");
            return 0;
        }
    }

    internal static void CompileTupiProj(string[]? filesTupi, bool isExe, bool isDll, bool isLib, string? output)
    {
        List<string> nameFiles = new();
        if(filesTupi is null)
        {
            CompilerFunc.ShowErrors("need a source file to compile");
            return;
        }

        OutputType outputType = OutputType.Exe;
        if(isExe)
            outputType = OutputType.Exe;
        else if (isDll)
            outputType = OutputType.Dll;
        else if (isLib)
            outputType = OutputType.Lib;

        foreach(string pathTupi in filesTupi)
        {
            string? _pathCompile = Path.GetDirectoryName(Path.GetFullPath(pathTupi));
            if (_pathCompile is not null)
                pathCompile = Path.GetFullPath(_pathCompile);

            string tupiFileName = Path.GetFileNameWithoutExtension(pathTupi);
            nameFiles.Add(tupiFileName);
            Console.WriteLine($"compile tupi file: {pathTupi}");
            Console.WriteLine("tranform tupi code to assembly(masm)");

            Directory.CreateDirectory(pathDir);
            StreamWriter write = File.CreateText(pathDir + $"\\{tupiFileName}.asm");
            write.Write(CompileTupiCodeFile(pathTupi, out mainCompiler, true));
            write.Close();
        }

        if (!Directory.Exists(pathDir + "\\header"))
            Directory.CreateDirectory(pathDir + "\\header");
        if (File.Exists(pathDir + "\\header\\std_tupi_def.inc"))
            File.Delete(pathDir + "\\header\\std_tupi_def.inc");
        File.Copy($"{x64Path}std_tupi_def.inc", pathDir + "\\header\\std_tupi_def.inc");

        CompileAsm(pathDir, nameFiles, MainCompiler.LinkLibs, MainCompiler.FnExport, outputType, output);
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
        List<string> fnExport, OutputType output, string? outputName, bool run = false, bool assembler_warning = true)
    {
        Console.WriteLine("tranform assembly to binary file");
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            CreateNoWindow = !assembler_warning,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/C cd \"{path_dir_asm}\" && call \"{Config.Vspath}VC\\Auxiliary\\Build\\vcvarsall.bat\" {Config.Arch}  &&"
        };
        foreach (string asmFile in nameFiles)
        {
            startInfo.Arguments += $" ml64 {asmFile}.asm /c &&";
        }

        if (output == OutputType.Lib)
        {
            string libCommand = " lib";
            foreach (string objFile in nameFiles)
                libCommand += $" {objFile}.obj";

            var libFilesDistinct = libFiles.Distinct();
            foreach (string libFile in libFilesDistinct)
            {
                libCommand += $" /nodefaultlib:{libFile}";
            }

            var fnExportDistinct = fnExport.Distinct();
            foreach (string fnExp in fnExportDistinct)
            {
                libCommand += $" /export:{fnExp}";
            }

            if(outputName is not null)
            {
                libCommand += $" /out:{outputName}";
            }

            startInfo.Arguments += libCommand;
        }
        else
        {
            string linkCommand = " link";
            foreach (string objFile in nameFiles)
                linkCommand += $" {objFile}.obj";

            if (output == OutputType.Exe)
                linkCommand += $" /entry:main /subsystem:console";
            else if (output == OutputType.Dll)
                linkCommand += $" /DLL /entry:DllMain";

            var libFilesDistinct = libFiles.Distinct();
            foreach (string libFile in libFilesDistinct)
            {
                linkCommand += $" /defaultlib:{libFile}";
            }

            var fnExportDistinct = fnExport.Distinct();
            foreach (string fnExp in fnExportDistinct)
            {
                linkCommand += $" /export:{fnExp}";
            }

            if (outputName is not null)
            {
                linkCommand += $" /out:{outputName}";
            }

            startInfo.Arguments += linkCommand;
            if (run)
            {
                startInfo.Arguments += " && main";
            }
        }

        StreamWriter write = File.CreateText(pathDir + $"\\build.bat");
        write.Write(startInfo.Arguments.Remove(0, $"/C cd \"{path_dir_asm}\" && ".Length));
        write.Close();

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        Console.WriteLine("compile finished!!");
    }
}

struct Config
{
    public string Vspath { get; private set; }
    public string Arch { get; private set; }
}

enum OutputType
{
    Exe,
    Dll,
    Lib,
}