#include "pch.h"
#include "proc.h"

EXTC void startProcess(char* appName, char* args, WORD nCmdShow) {
	STARTUPINFOA info = { sizeof(info) };
	info.dwFlags = STARTF_USESHOWWINDOW;
	info.wShowWindow = nCmdShow;
	PROCESS_INFORMATION processInfo;

	int i, j;
	char* cmd;
	for (i = 0; appName[i] != '\0'; ++i);
	for (j = 0; args[j] != '\0'; ++j);
	cmd = (char*)createMem(i + j + 1);

	for (i = 0; appName[i] != '\0'; ++i){
		cmd[i] = appName[i];
	}
	cmd[i] = ' ';
	i++;
	for (j = 0; args[j] != '\0'; ++j, ++i){
		cmd[i] = args[j];
	}
	cmd[i] = '\0';

	if (CreateProcessA(NULL, (LPSTR)appName, NULL, NULL, TRUE, 0, NULL, NULL, &info, &processInfo))
	{
		WaitForSingleObject(processInfo.hProcess, INFINITE);
		CloseHandle(processInfo.hProcess);
		CloseHandle(processInfo.hThread);
	}
}