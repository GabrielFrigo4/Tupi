#include "pch.h"
#include "framework.h"
#include "cmd.h"
#include "math.h"

EXTC BOOL consoleWrite(const VOID * lpBuffer, DWORD nNumberOfCharsToWrite, LPDWORD lpNumberOfCharsWritten) {
    HANDLE hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);
    return WriteConsoleA(hConsoleOutput, lpBuffer, nNumberOfCharsToWrite, lpNumberOfCharsWritten, NULL);
}

EXTC BOOL consoleWriteStr(const char* str) {
    DWORD length = 0;
    while (str[length] != '\0')
    {
        length++;
    }
    return consoleWrite(str, length, NULL);
}

EXTC BOOL consoleWriteInt(long long int number) {
    const char* strInt = iToStr(number);
    BOOL ret = consoleWriteStr(strInt);
    deleteMem((char*)strInt);
    return ret;
}

EXTC BOOL consoleWriteFloat(double number, int decimalPlaces) {
    const char* strFloat = fToStr(number, decimalPlaces);
    BOOL ret = consoleWriteStr(strFloat);
    deleteMem((char*)strFloat);
    return ret;
}

EXTC const char* iToStr(long long int number) {
    bool isNegative = false;
    char* strInt = (char*)createMem(1);
    char* strSignIntIvert = (char*)createMem(2);
    DWORD length = 1;

    //end of string
    if (strSignIntIvert != NULL) {
        strSignIntIvert[0] = '\0';
    }

    //sign
    if (number < 0) {
        if (strInt != NULL) {
            strInt[0] = '-';
        }
        number *= -1;
        isNegative = true;
    }

    //body number
    if (number == 0) {
        length++;
        char* strInt2 = (char*)recreateMem(strSignIntIvert, static_cast<size_t>(length));
        strSignIntIvert = strInt2;
        if (strSignIntIvert != NULL) {
            strSignIntIvert[length - 1] = '0';
        }
    }
    else {
        while (number > 0)
        {
            length++;
            char* strInt2 = (char*)recreateMem(strSignIntIvert, static_cast<size_t>(length));
            strSignIntIvert = strInt2;
            if (strSignIntIvert != NULL) {
                strSignIntIvert[length - 1] = (char)(number % 10 + '0');
            }
            number /= 10;
        }
    }

    if (strInt != NULL) {
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

    deleteMem(strSignIntIvert);
    return strInt;
}

EXTC const char* fToStr(double number, int decimalPlaces) {
    long long int IntPart = mathFloor(mathAbs(number));
    double DecimalPart = mathAbs(number) - IntPart;
    bool isNegative = false;
    char* strFloat = (char*)createMem(1);
    char* strSignFloatIvert = (char*)createMem(1);
    DWORD length = 0;

    //sign
    if (number < 0) {
        if (strFloat != NULL) {
            strFloat[0] = '-';
        }
        number *= -1;
        isNegative = true;
    }

    //integer part
    if (IntPart == 0) {
        length++;
        char* strFloar2 = (char*)recreateMem(strSignFloatIvert, static_cast<size_t>(length));
        strSignFloatIvert = strFloar2;
        if (strSignFloatIvert != NULL) {
            strSignFloatIvert[length - 1] = '0';
        }
    }
    else {
        while (IntPart > 0)
        {
            length++;
            char* strFloar2 = (char*)recreateMem(strSignFloatIvert, static_cast<size_t>(length));
            strSignFloatIvert = strFloar2;
            if (strSignFloatIvert != NULL) {
                strSignFloatIvert[length - 1] = (char)(IntPart % 10 + '0');
            }
            IntPart /= 10;
        }
    }

    if (strFloat != NULL) {
        char* strFloat2 = (char*)recreateMem(strFloat, static_cast<size_t>(length) + isNegative);
        strFloat = strFloat2;
    }

    int invertPos = length;
    length += isNegative;
    do
    {
        if (strFloat != NULL && strSignFloatIvert != NULL) {
            strFloat[length - invertPos] = strSignFloatIvert[invertPos - 1];
        }
        invertPos--;
    } while (invertPos > 0);

    //decimal part
    if (DecimalPart > 0 && decimalPlaces != 0) {
        length++;
        if (strFloat != NULL) {
            char* strFloat2 = (char*)recreateMem(strFloat, length);
            strFloat = strFloat2;
        }
        if (strFloat != NULL) {
            strFloat[length - 1] = '.';
        }
    }
    while (DecimalPart > 0)
    {
        if (decimalPlaces == 0) {
            DecimalPart = 0;
            break;
        }
        decimalPlaces--;

        int numb = mathFloor(DecimalPart * 10);
        DecimalPart = DecimalPart * 10 - numb;
        length++;
        if (strFloat != NULL) {
            char* strFloat2 = (char*)recreateMem(strFloat, length);
            strFloat = strFloat2;
        }
        if (strFloat != NULL) {
            strFloat[length - 1] = (char)(numb + '0');
        }
    }

    //end of string
    if (strFloat != NULL) {
        char* strFloat2 = (char*)recreateMem(strFloat, static_cast<size_t>(length) + 1);
        strFloat = strFloat2;
    }
    if (strFloat != NULL) {
        strFloat[length] = '\0';
    }

    deleteMem(strSignFloatIvert);
    return strFloat;
}
