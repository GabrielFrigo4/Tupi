#include "pch.h"
#include "framework.h"
#include "heapapi.h"

LPVOID MemAlloc(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_ZERO_MEMORY, size);
}

LPVOID MemReAlloc(LPVOID plMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_ZERO_MEMORY, plMem, size);
}

BOOL MemFree(LPVOID plMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, plMem);
}