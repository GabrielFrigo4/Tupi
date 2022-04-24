#include <windows.h>
#include <stdio.h>
#include <cmath>
#include <iostream>

BOOL consoleWriteStr(const char* str);
BOOL consoleWriteInt(long long int number);
BOOL consoleWriteFloat(double number);
BOOL consoleNewLine();
BOOL consoleWrite(const VOID* lpBuffer, DWORD nNumberOfCharsToWrite, LPDWORD lpNumberOfCharsWritten);
BOOL consoleRead(LPVOID lpBuffer, DWORD nNumberOfCharsToRead, LPDWORD lpNumberOfCharsRead);

int main()
{
    const int READ_SIZE = 256;
    DWORD nRead;
    LPVOID ptr = malloc(READ_SIZE);
    const char* myString = "hello!!";
    consoleWriteStr("hello!!\n");
    consoleWriteInt(-1200);
    consoleNewLine();
    consoleWriteFloat(-32.5);
    consoleNewLine();
    consoleRead(ptr, READ_SIZE, &nRead);
}