#include "pch.h"
#include "framework.h"

extern "C" int sign(double val) {
	return (0 < val) - (val < 0);
}