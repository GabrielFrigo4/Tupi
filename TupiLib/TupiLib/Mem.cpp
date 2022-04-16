#include "pch.h"
#include "framework.h"
#include "tupiWin.h"

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