#pragma once

#include "tupiWin.h"

EXTC LPVOID createMem(size_t size);
EXTC LPVOID recreateMem(LPVOID plMem, size_t size);
EXTC BOOL deleteMem(LPVOID plMem);