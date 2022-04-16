#pragma once

#define NULL 0
#define VOID void
#define WINAPI __stdcall
#define HEAP_NO_SERIALIZE 0x00000001
#define HEAP_GENERATE_EXCEPTIONS 0x00000004
#define HEAP_ZERO_MEMORY 0x00000008
#define HEAP_REALLOC_IN_PLACE_ONLY 0x00000010
#define STD_INPUT_HANDLE ((DWORD)-10)
#define STD_OUTPUT_HANDLE ((DWORD)-11)
#define STD_ERROR_HANDLE ((DWORD)-12)
typedef int BOOL;
typedef int DWORD;
typedef size_t SIZE_T;
typedef void* LPVOID;
typedef void* HANDLE;
typedef DWORD* LPDWORD;
extern "C" HANDLE GetProcessHeap();
extern "C" LPVOID HeapAlloc(
	HANDLE hHeap,
	DWORD  dwFlags,
	SIZE_T dwBytes
);
extern "C" LPVOID HeapReAlloc(
	HANDLE	hHeap,
	DWORD	dwFlags,
	LPVOID	lpMem,
	SIZE_T	dwBytes
);
extern "C" BOOL HeapFree(
	HANDLE	hHeap,
	DWORD	dwFlags,
	LPVOID	lpMem
);
extern "C" HANDLE WINAPI GetStdHandle(
	DWORD nStdHandle
);
extern "C" BOOL WINAPI WriteConsoleA(
	HANDLE  hConsoleOutput,
	const VOID* lpBuffer,
	DWORD   nNumberOfCharsToWrite,
	LPDWORD lpNumberOfCharsWritten,
	LPVOID  lpReserved
);