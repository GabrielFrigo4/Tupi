#include "pch.h"
#include "framework.h"
#include "stdio.h"

extern "C" int print(const char* format, ...) {
	return printf(format);
}

extern "C" int input(const char* format, ...) {
	return scanf_s(format);
}