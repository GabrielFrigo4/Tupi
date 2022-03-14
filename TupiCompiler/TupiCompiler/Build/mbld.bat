@echo on

if not defined DevEnvDir (
  call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"
)

ml64 main.asm /link /subsystem:console /defaultlib:kernel32.lib /defaultlib:user32.lib /defaultlib:libcmt.lib && main && pause