#include "pch.h"
#include "framework.h"

extern "C" LPVOID MemAlloc(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_ZERO_MEMORY, size);
}

extern "C" LPVOID MemReAlloc(LPVOID plMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_ZERO_MEMORY, plMem, size);
}

extern "C" BOOL MemFree(LPVOID plMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, plMem);
}