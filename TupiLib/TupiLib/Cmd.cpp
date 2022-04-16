#include "pch.h"
#include "framework.h"
#include "mem.h"

extern "C" BOOL consoleWrite(const VOID * lpBuffer, DWORD nNumberOfCharsToWrite, LPDWORD lpNumberOfCharsWritten) {
    HANDLE hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);
    return WriteConsoleA(hConsoleOutput, lpBuffer, nNumberOfCharsToWrite, lpNumberOfCharsWritten, NULL);
}

extern "C" BOOL consoleWriteStr(const char* str) {
    DWORD length = 0;
    while (str[length] != '\0')
    {
        length++;
    }
    return consoleWrite(str, length, NULL);
}

extern "C" BOOL consoleWriteInt(long long int number) {
    bool isNegative = false;
    char* strInt = (char*)createMem(1);
    char* strSignIntIvert = (char*)createMem(1);
    DWORD length = 0;
    if (number < 0) {
        if (strInt != NULL) {
            strInt[0] = '-';
        }
        number *= -1;
        isNegative = true;
        if (strSignIntIvert != NULL) {
            char* strInt2 = (char*)recreateMem(strSignIntIvert, length + 1);
            strSignIntIvert = strInt2;
        }
    }

    if (number == 0) {
        if (strSignIntIvert != NULL) {
            strSignIntIvert[length] = '0';
        }
        number /= 10;
        length++;
        if (strSignIntIvert != NULL) {
            char* strInt2 = (char*)recreateMem(strSignIntIvert, length + 1);
            strSignIntIvert = strInt2;
        }
    }
    else {
        while (number > 0)
        {
            if (strSignIntIvert != NULL) {
                strSignIntIvert[length] = (char)(number % 10 + '0');
            }
            number /= 10;
            length++;
            if (strSignIntIvert != NULL) {
                char* strInt2 = (char*)recreateMem(strSignIntIvert, length + 1);
                strSignIntIvert = strInt2;
            }
        }
    }

    if (strSignIntIvert != NULL) {
        strSignIntIvert[length] = '\0';
    }
    length++;
    if (strSignIntIvert != NULL) {
        char* strInt2 = (char*)recreateMem(strInt, length + isNegative);
        strInt = strInt2;
    }

    int invertPos = length;
    length += isNegative;
    do
    {
        if (strInt != NULL && strSignIntIvert != NULL) {
            strInt[length - invertPos] = strSignIntIvert[invertPos - 1];
        }
        invertPos--;
    } while (invertPos > 0);

    return consoleWrite(strInt, length, NULL);
}

//extern "C" int consoleRead(const char* format, ...) {
//	return scanf_s(format);
//}