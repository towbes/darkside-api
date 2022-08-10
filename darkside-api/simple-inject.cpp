#include "simple-inject.h"


int simpleInject(const char* dllPath, DWORD pid) {

	DWORD procId = pid;

	HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, 0, procId);

	if (hProc && hProc != INVALID_HANDLE_VALUE) {
		void* loc = VirtualAllocEx(hProc, 0, MAX_PATH, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

		if (loc) {
			WriteProcessMemory(hProc, loc, dllPath, strlen(dllPath) + 1, 0);
		}


		HANDLE hThread = CreateRemoteThread(hProc, 0, 0, (LPTHREAD_START_ROUTINE)LoadLibraryA, loc, 0, 0);

		if (hThread) {
			CloseHandle(hThread);
		}

	}

	if (hProc) {
		CloseHandle(hProc);
	}

	return 0;
}