#include "DarksideAPI.h"
#include "simple-inject.h"


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

DarksideAPI::~DarksideAPI() {
    if (chatThread != nullptr) {
        chatThread->join();
    }
}

bool DarksideAPI::InjectPid(int pid) {
    //Get the current directory
    CHAR buffer[MAX_PATH] = { 0 };
    GetModuleFileNameA(NULL, buffer, MAX_PATH);
    std::wstring::size_type pos = std::string(buffer).find_last_of("\\/");
    std::string currPath = std::string(buffer).substr(0, pos);
    //append darkside-hooks.dll
    currPath = currPath + "\\darkside-hooks.dll";
    simpleInject(currPath.c_str(), (DWORD)pid);
    std::wstring msg = std::format(L"Injected {}\n", pid);
    //MessageBox(0, msg.c_str(), L"Hi", MB_ICONINFORMATION);
    //Short sleep is needed to prevent a crash
    Sleep(100);
    this->pidHandle = pid;
    std::string str = std::to_string(pid) + "_tshmem";

    //get process handle
    HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, 0, pid);
    str = std::to_string(pid) + "_tshmem";

    // Get a handle to our file map
    hMapFile = CreateFileMappingA(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, SHMEMSIZE, str.c_str());
    if (hMapFile == nullptr) {
        MessageBoxA(nullptr, "API Failed to create file mapping!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
        return false;
    }
    else {
        // Get our shared memory pointer
        lpMemFile = MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, 0);
        if (lpMemFile == nullptr) {
            MessageBoxA(nullptr, "API Failed to map shared memory!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
            return false;
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
            if (hThread != 0) {
                ResumeThread(hThread);
            }
            else {
                _tprintf(TEXT("Could not create remote thread (%d).\n"),
                    GetLastError());
                return false;
            }
            Sleep(100);
           //Start the chat listener
            if (chatThread == nullptr) {
                chatThread = new std::thread(&DarksideAPI::ChatListener, this);
            }
            
        }
        return true;
    }
}

int DarksideAPI::GetPid() {
    return pidHandle;
}