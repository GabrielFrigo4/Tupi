#include "pch.h"
#include "framework.h"

extern "C" double pow(double x, double y);

extern "C" double __internal__tupi__operate__math__pow(double x, double y) {
	return pow(x, y);
}

extern "C" double __internal__tupi__operate__math__root(double x, double y) {
	return pow(x, 1/y);
}