#include "pch.h"
#include "framework.h"
#include "tupiWin.h"

extern "C" LPVOID createMem(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_NO_SERIALIZE, size);
}

extern "C" LPVOID recreateMem(LPVOID lpMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_NO_SERIALIZE, lpMem, size);
}

extern "C" BOOL deleteMem(LPVOID lpMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, lpMem);
}