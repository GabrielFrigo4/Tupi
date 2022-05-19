#include "pch.h"
#include "framework.h"
#include "tupiWin.h"

LPVOID __internal__tupi__operate__memory__new(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_NO_SERIALIZE, size);
}

BOOL __internal__tupi__operate__memory__delete(LPVOID lpMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, lpMem);
}