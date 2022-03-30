// TupiLib.cpp : Define as funções da biblioteca estática.
//

#include "pch.h"
#include "framework.h"
#include "heapapi.h"

// TODO: Este é um exemplo de uma função de biblioteca
void fnTupiLib()
{
}

LPVOID MemAlloc(size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapAlloc(hHeap, HEAP_ZERO_MEMORY, size);
}

LPVOID MemReAlloc(LPVOID plMem, size_t size) {
	HANDLE hHeap = GetProcessHeap();
	return HeapReAlloc(hHeap, HEAP_ZERO_MEMORY, plMem, size);
}

bool MemFree(LPVOID plMem) {
	HANDLE hHeap = GetProcessHeap();
	return HeapFree(hHeap, HEAP_NO_SERIALIZE, plMem);
}