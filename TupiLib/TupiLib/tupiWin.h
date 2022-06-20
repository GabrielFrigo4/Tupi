#pragma once

#define EXTC extern "C"

#define NULL 0
#define TRUE 1
#define FALSE 0
#define INFINITE 0xFFFFFFFF
#define SW_HIDE 0
#define SW_SHOWNORMAL 1
#define SW_NORMAL 1
#define SW_SHOWMINIMIZED 2
#define SW_SHOWMAXIMIZED 3
#define SW_MAXIMIZE 3
#define SW_SHOWNOACTIVATE 4
#define SW_SHOW 5
#define SW_MINIMIZE 6
#define SW_SHOWMINNOACTIVE 7
#define SW_SHOWNA 8
#define SW_RESTORE 9
#define SW_SHOWDEFAULT 10
#define SW_FORCEMINIMIZE 11
#define FILE_SHARE_DELETE 0x00000004
#define FILE_SHARE_READ 0x00000001
#define FILE_SHARE_WRITE 0x00000002
#define CREATE_ALWAYS 2
#define CREATE_NEW 1
#define OPEN_ALWAYS 4
#define OPEN_EXISTING 3
#define TRUNCATE_EXISTING 5
#define GENERIC_BOTH (GENERIC_READ | GENERIC_WRITE)
#define GENERIC_WRITE 30
#define GENERIC_READ 31
#define STARTF_USESHOWWINDOW 0x00000001
#define VOID void
#define WINAPI __stdcall
#define HEAP_NO_SERIALIZE 0x00000001
#define HEAP_GENERATE_EXCEPTIONS 0x00000004
#define HEAP_ZERO_MEMORY 0x00000008
#define HEAP_REALLOC_IN_PLACE_ONLY 0x00000010
#define STD_INPUT_HANDLE ((DWORD)-10)
#define STD_OUTPUT_HANDLE ((DWORD)-11)
#define STD_ERROR_HANDLE ((DWORD)-12)

typedef int BOOL;
typedef short int WORD;
typedef int DWORD;
typedef long long int QWORD;
typedef char BYTE;
typedef size_t SIZE_T;
typedef void* LPVOID;
typedef void* HANDLE;
typedef DWORD* LPDWORD;
typedef const char* LPCSTR;
typedef char* PSTR, * LPSTR;
typedef BYTE* LPBYTE;

typedef struct _SECURITY_ATTRIBUTES {
	DWORD	nLength;
	LPVOID	lpSecurityDescriptor;
	BOOL	bInheritHandle;
} SECURITY_ATTRIBUTES, * PSECURITY_ATTRIBUTES, * LPSECURITY_ATTRIBUTES;
typedef struct _STARTUPINFOA {
	DWORD  cb;
	LPSTR  lpReserved;
	LPSTR  lpDesktop;
	LPSTR  lpTitle;
	DWORD  dwX;
	DWORD  dwY;
	DWORD  dwXSize;
	DWORD  dwYSize;
	DWORD  dwXCountChars;
	DWORD  dwYCountChars;
	DWORD  dwFillAttribute;
	DWORD  dwFlags;
	WORD   wShowWindow;
	WORD   cbReserved2;
	LPBYTE lpReserved2;
	HANDLE hStdInput;
	HANDLE hStdOutput;
	HANDLE hStdError;
} STARTUPINFOA, * LPSTARTUPINFOA;
typedef struct _PROCESS_INFORMATION {
	HANDLE hProcess;
	HANDLE hThread;
	DWORD  dwProcessId;
	DWORD  dwThreadId;
} PROCESS_INFORMATION, * PPROCESS_INFORMATION, * LPPROCESS_INFORMATION;

EXTC HANDLE GetProcessHeap();
EXTC LPVOID HeapAlloc(
	HANDLE hHeap,
	DWORD  dwFlags,
	SIZE_T dwBytes
);
EXTC LPVOID HeapReAlloc(
	HANDLE	hHeap,
	DWORD	dwFlags,
	LPVOID	lpMem,
	SIZE_T	dwBytes
);
EXTC BOOL HeapFree(
	HANDLE	hHeap,
	DWORD	dwFlags,
	LPVOID	lpMem
);
EXTC HANDLE WINAPI GetStdHandle(
	DWORD nStdHandle
);
EXTC BOOL WINAPI WriteConsoleA(
	HANDLE  hConsoleOutput,
	const VOID* lpBuffer,
	DWORD   nNumberOfCharsToWrite,
	LPDWORD lpNumberOfCharsWritten,
	LPVOID  lpReserved
);
EXTC BOOL CreateProcessA(
	LPCSTR	lpApplicationName,
	LPSTR	lpCommandLine,
	LPSECURITY_ATTRIBUTES	lpProcessAttributes,
	LPSECURITY_ATTRIBUTES	lpThreadAttributes,
	BOOL	bInheritHandles,
	DWORD	dwCreationFlags,
	LPVOID	lpEnvironment,
	LPCSTR	lpCurrentDirectory,
	LPSTARTUPINFOA	lpStartupInfo,
	LPPROCESS_INFORMATION	lpProcessInformation
);
EXTC DWORD WaitForSingleObject(
	HANDLE hHandle,
	DWORD  dwMilliseconds
);
EXTC BOOL CloseHandle(
	HANDLE hObject
);
EXTC HANDLE CreateFileA(
	LPCSTR	lpFileName,
	DWORD	dwDesiredAccess,
	DWORD	dwShareMode,
	LPSECURITY_ATTRIBUTES	lpSecurityAttributes,
	DWORD	dwCreationDisposition,
	DWORD	dwFlagsAndAttributes,
	HANDLE	hTemplateFile
);
EXTC BOOL DeleteFileA(
	LPCSTR	lpFileName
);