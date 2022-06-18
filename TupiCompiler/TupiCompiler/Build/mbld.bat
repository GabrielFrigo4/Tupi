@echo on

if not defined DevEnvDir (
  call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"
)

ml64 mycode.asm /link /subsystem:console /defaultlib:kernel32.lib /defaultlib:C:\Users\gabri\source\projs\Tupi\TupiCompiler\TupiCompiler\_tupi\x64\lib\TupiLib.lib && pause