#include "pch.h"
#include "framework.h"

extern "C" LPVOID memAlloc(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_NO_SERIALIZE, size);
}

extern "C" LPVOID memReAlloc(LPVOID plMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_NO_SERIALIZE, plMem, size);
}

extern "C" BOOL memFree(LPVOID plMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, plMem);
}