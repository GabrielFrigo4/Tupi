#include "pch.h"
#include "io.h"

EXTC HANDLE createFile(
	LPCSTR	lpFileName,
	DWORD	dwDesiredAccess,
	DWORD	dwShareMode,
	DWORD	dwCreationDisposition
) {
	return CreateFileA(lpFileName, dwDesiredAccess, dwShareMode, NULL, dwCreationDisposition, (0x80), NULL);
}

EXTC BOOL deleteFile(LPCSTR	lpFileName) {
	return DeleteFileA(lpFileName);
}