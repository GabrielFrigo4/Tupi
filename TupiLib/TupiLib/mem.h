#pragma once

#include "tupiWin.h"

extern "C" LPVOID createMem(size_t size);
extern "C" LPVOID recreateMem(LPVOID plMem, size_t size);
extern "C" BOOL deleteMem(LPVOID plMem);