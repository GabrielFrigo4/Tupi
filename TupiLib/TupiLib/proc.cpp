#include "pch.h"
#include "proc.h"
#include "str.h"

EXTC BOOL startProcess(char* appName, char* commad, WORD nCmdShow, BOOL waitEndProc) {
	STARTUPINFOA info = { sizeof(info) };
	info.dwFlags = STARTF_USESHOWWINDOW;
	info.wShowWindow = nCmdShow;
	PROCESS_INFORMATION processInfo;

	BOOL ret = CreateProcessA((LPSTR)appName, (LPSTR)commad, NULL, NULL, TRUE, 0, NULL, NULL, &info, &processInfo);
	if (ret)
	{
		if (waitEndProc == TRUE)
			WaitForSingleObject(processInfo.hProcess, INFINITE);
		else
			WaitForSingleObject(processInfo.hProcess, 0);
		CloseHandle(processInfo.hProcess);
		CloseHandle(processInfo.hThread);
	}
	return ret;
}

EXTC BOOL startCommand(char* command, char* args, WORD nCmdShow, BOOL waitEndProc) {
	STARTUPINFOA info = { sizeof(info) };
	info.dwFlags = STARTF_USESHOWWINDOW;
	info.wShowWindow = nCmdShow;
	PROCESS_INFORMATION processInfo;

	char* cmd = joinStrWithSpace(command, args);

	BOOL ret = CreateProcessA(NULL, (LPSTR)cmd, NULL, NULL, TRUE, 0, NULL, NULL, &info, &processInfo);
	if (ret)
	{
		if (waitEndProc == TRUE)
			WaitForSingleObject(processInfo.hProcess, INFINITE);
		else
			WaitForSingleObject(processInfo.hProcess, 0);
		CloseHandle(processInfo.hProcess);
		CloseHandle(processInfo.hThread);
	}
	deleteMem(cmd);
	return ret;
}

EXTC void exitProcess(BOOL ret) {
	ExitProcess(ret);
}