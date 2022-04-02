#include "pch.h"
#include "framework.h"
#include "stdio.h"

extern "C" int consoleWrite(const char* format, ...) {
	return printf(format);
}

extern "C" int consoleRead(const char* format, ...) {
	return scanf_s(format);
}