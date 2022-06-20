#pragma once

#include "mem.h"

EXTC BOOL consoleWrite(const VOID* lpBuffer, DWORD nNumberOfCharsToWrite, LPDWORD lpNumberOfCharsWritten);
EXTC BOOL consoleWriteStr(const char* str);
EXTC BOOL consoleWriteInt(long long int number);
EXTC BOOL consoleWriteFloat(double number);
EXTC const char* iToStr(long long int number);
EXTC const char* fToStr(double number);