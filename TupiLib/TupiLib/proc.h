#pragma once

#include "tupiWin.h"
#include "mem.h"

EXTC BOOL startProcess(char* appName, char* command, WORD nCmdShow);
EXTC BOOL startCommand(char* command, char* args, WORD nCmdShow);