#include "pch.h"
#include "framework.h"

extern "C" int cmdPrint(const char* format, ...) {
	return printf(format);
}

extern "C" int cmdScan(const char* format, ...) {
	return scanf_s(format);
}