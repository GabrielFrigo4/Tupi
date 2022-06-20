#include "pch.h"
#include "framework.h"
#include "tupiWin.h"

EXTC LPVOID createMem(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_NO_SERIALIZE, size);
}

EXTC LPVOID recreateMem(LPVOID lpMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_NO_SERIALIZE, lpMem, size);
}

EXTC BOOL deleteMem(LPVOID lpMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, lpMem);
}