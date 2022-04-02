#include "pch.h"
#include "framework.h"
#define HEAP_NO_SERIALIZE 0x00000001
#define HEAP_GENERATE_EXCEPTIONS 0x00000004
#define HEAP_ZERO_MEMORY 0x00000008
#define HEAP_REALLOC_IN_PLACE_ONLY 0x00000010
typedef void* LPVOID;
typedef void* HANDLE;
typedef int BOOL;
typedef int DWORD;
typedef size_t SIZE_T;
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

extern "C" LPVOID createMem(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_NO_SERIALIZE, size);
}

extern "C" LPVOID recreateMem(LPVOID plMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_NO_SERIALIZE, plMem, size);
}

extern "C" BOOL deleteMem(LPVOID plMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, plMem);
}