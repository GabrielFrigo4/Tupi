#include "pch.h"
#include "framework.h"
#include <windows.h>

extern "C" LPVOID alloc(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_NO_SERIALIZE, size);
}

extern "C" LPVOID realloc(LPVOID plMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_NO_SERIALIZE, plMem, size);
}

extern "C" BOOL free(LPVOID plMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, plMem);
}