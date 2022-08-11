#include "DarksideAPI.h"
#include "simple-inject.h"
#include <format>

struct playerpos_t {
    float pos_x;
    short heading;
    unsigned char unknown[68];
    float pos_y;
    unsigned char unknown2[8];
    float pos_z;
    char unknown4;
};

extern "C" VOID __cdecl MainThread();

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
static HANDLE hMapFile;
static LPVOID lpMemFile;

DarksideAPI::DarksideAPI() {}

DarksideAPI::~DarksideAPI() {}

void DarksideAPI::InjectPid(int pid) {
    simpleInject("C:\\Users\\ritzgames\\Desktop\\daoc\\darkside\\darkside-api\\DarksideGUI\\bin\\Debug\\net6.0-windows\\darkside-hooks.dll", (DWORD)pid);
    std::wstring msg = std::format(L"Injected {}\n", pid);
    MessageBox(0, msg.c_str(), L"Hi", MB_ICONINFORMATION);
    this->pidHandle = pid;
    std::string str = std::to_string(pid) + "_tshmem";

    //get process handle
    HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, 0, pid);
    str = std::to_string(pid) + "_tshmem";

    // Get a handle to our file map
    hMapFile = CreateFileMappingA(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, SHMEMSIZE, str.c_str());
    if (hMapFile == nullptr) {
        MessageBoxA(nullptr, "API Failed to create file mapping!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
    }
    else {
        // Get our shared memory pointer
        lpMemFile = MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, 0);
        if (lpMemFile == nullptr) {
            MessageBoxA(nullptr, "API Failed to map shared memory!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
        }
        else {
            // Copy from shared memory
            TSharedData data;
            memcpy(&data, lpMemFile, SHMEMSIZE);
            // Clean up
            UnmapViewOfFile(lpMemFile);
            CloseHandle(hMapFile);
            // Call the remote function
            DWORD dwThreadId = 0;
            auto hThread = CreateRemoteThread(hProc, nullptr, 0, LPTHREAD_START_ROUTINE(data.lpInit), nullptr, 0, &dwThreadId);
            ResumeThread(hThread);
        }
    }
}

bool DarksideAPI::GetPlayerInfo(LPVOID lpBuffer) {
   

    std::size_t fileSize = sizeof(playerpos_t);

    TCHAR szName[] = TEXT("pid_mmf");



    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        szName);                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        return 1;
    }


    playerpos_t* pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    playerpos_t sPlayerPos = *pPlayerPos;

    memcpy(lpBuffer, &sPlayerPos, sizeof(playerpos_t));

    UnmapViewOfFile(pPlayerPos);

    CloseHandle(hMapFile);

    return true;
}