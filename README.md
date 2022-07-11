# Tupi
Tupi is a compiled programming language that uses masm(microsoft assembly), so tupi only works for windows, the compiler uses oc# that translates the code to assembly and then uses the microsoft c++ sdk assembler to transform the assembly code in an .exe or .dll or .lib; This language is at an early stage of development, the intention of me starting the project was to train and learn, but who knows, maybe one day it will end up being good, useful and functional.

# hello world in tupi 1:
    useth <std.th>
    ref i8 msg "hello world", 0

    i64 main(){
        consoleWriteStr(msg)
        return 0
    }

# hello world in tupi 2:
    useth <std.th>
    ref i8 msg "hello world"
    const msgSize size msg

    i64 main(){
        consoleWrite(msg, msgSize, NULL)
        return 0
    }
