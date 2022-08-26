// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

HMODULE ghModule;

//Start main thread remotely: https://stackoverflow.com/questions/13428881/calling-a-function-in-an-injected-dll/29897128

// Data struct to be shared between processes
struct TSharedData
{
    DWORD dwOffset = 0;
    HMODULE hModule = nullptr;
    LPDWORD lpInit = nullptr;
};

// Name of the exported function you wish to call from the Launcher process
#define DLL_REMOTEINIT_FUNCNAME "MainThread"
// Size (in bytes) of data to be shared
#define SHMEMSIZE sizeof(TSharedData)
// Name of the shared file map (NOTE: Global namespaces must have the SeCreateGlobalPrivilege privilege)
#define SHMEMNAME "pid_SHMEM"
static HANDLE hMapFile;
static LPVOID lpMemFile;


BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    TSharedData data;
    std::string str;
    uint32_t pid;
    ghModule = hModule;

    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hModule);

        //Get Process ID
        pid = GetCurrentProcessId();
        //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
        //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring
        str = std::to_string(pid) + "_tshmem";

        // Get a handle to our file map
        hMapFile = CreateFileMappingA(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, SHMEMSIZE, str.c_str());
        if (hMapFile == nullptr) {
            MessageBoxA(nullptr, "Failed to create file mapping!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Get our shared memory pointer
        lpMemFile = MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, 0);
        if (lpMemFile == nullptr) {
            MessageBoxA(nullptr, "Failed to map shared memory!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Set shared memory to hold what our remote process needs
        memset(lpMemFile, 0, SHMEMSIZE);
        data.hModule = hModule;
        data.lpInit = LPDWORD(GetProcAddress(hModule, DLL_REMOTEINIT_FUNCNAME));
        data.dwOffset = DWORD(data.lpInit) - DWORD(data.hModule);
        memcpy(lpMemFile, &data, sizeof(TSharedData));
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        // Tie up any loose ends
        UnmapViewOfFile(lpMemFile);
        CloseHandle(hMapFile);
        break;
    }
    return TRUE;
    UNREFERENCED_PARAMETER(lpReserved);
}
