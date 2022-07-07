#pragma once

#include "tupiWin.h"
#include "mem.h"

EXTC BOOL startProcess(char* appName, char* command, WORD nCmdShow, BOOL waitEndProc);
EXTC BOOL startCommand(char* command, char* args, WORD nCmdShow, BOOL waitEndProc);
EXTC void exitProcess(BOOL waitEndProc);