#include "pch.h"
#include "framework.h"
#include "math.h"

EXTC int mathSign(double val) {
	return (0 < val) - (val < 0);
}

EXTC double mathFloor(const double num) {
	return (unsigned long long) num;
}

EXTC double mathAbs(double a) {
	return a < 0 ? -a : a;
}