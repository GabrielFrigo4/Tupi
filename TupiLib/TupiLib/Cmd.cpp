#include "pch.h"
#include "framework.h"

extern "C" int CmdPrint(const char* format, ...) {
	return printf(format);
}

extern "C" int CmdScan(const char* format, ...) {
	return scanf_s(format);
}