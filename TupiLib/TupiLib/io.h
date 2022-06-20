#pragma once

#include "tupiWin.h"

EXTC HANDLE createFile(
	LPCSTR	lpFileName,
	DWORD	dwDesiredAccess,
	DWORD	dwShareMode,
	DWORD	dwCreationDisposition
);

EXTC BOOL deleteFile(LPCSTR	lpFileName);