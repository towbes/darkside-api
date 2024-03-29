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

DarksideAPI::DarksideAPI() {
    this->pidHandle = 0;
    injected = false;
    chatThread = nullptr;
}

DarksideAPI::~DarksideAPI() {
    //if (chatThread != nullptr) {
    //    chatThread->join();
    //    chatThread = nullptr;
    //}
}

bool DarksideAPI::InjectPid(int pid) {

    //Check shared memory to see if this pid is injected already, if not, inject it
    std::string str = std::to_string(pid) + "_tshmem";

    //get process handle
    // Get a handle to our file map
    hMapFile = OpenFileMappingA(PAGE_READWRITE, 0, str.c_str());
    //If it's a nullptr process hasn't been injected
    if (hMapFile == nullptr) {
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
                if (hThread != NULL) {
                    if (ResumeThread(hThread) != -1) {
                        injected = true;
                        Sleep(100);
                        //Start the chat listener
                        if (chatThread == nullptr) {
                            chatThread = new std::thread(&DarksideAPI::ChatListener, this);
                        }
                    }
                    else {
                        ::OutputDebugStringA(std::format("Resumethreaderror: {}", GetLastError()).c_str());
                        return false;
                    }

                }
                else {
                    _tprintf(TEXT("Could not create remote thread (%d).\n"),
                        GetLastError());
                    return false;
                }
            }
            return true;
        }
    }
    //If it's not null, the process has already been injected
    else {
        this->pidHandle = pid;
        this->injected = true;
        //Start the chat listener
        if (chatThread == nullptr) {
            chatThread = new std::thread(&DarksideAPI::ChatListener, this);
        }
        return true;
    }

}

int DarksideAPI::GetPid() {
    return pidHandle;
}

bool DarksideAPI::Unload(int pid) {

    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring
    std::string str = std::to_string(pidHandle) + "_unloadFlag";
    std::size_t fsize = sizeof(int);

    // Get a handle to our file map
    auto hMapFile = CreateFileMappingA(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, fsize, str.c_str());
    if (hMapFile == nullptr) {
        MessageBoxA(nullptr, "Failed to create file mapping!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
        return false;
    }
    // Get our shared memory pointer
    int* lpMemFile = (int*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, 0);
    if (lpMemFile == nullptr) {
        MessageBoxA(nullptr, "Failed to map shared memory!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
        UnmapViewOfFile(hMapFile);
        return false;
    }
    injected = false;
    //Give the chat thread time to finish
    Sleep(200);

    *lpMemFile = 1;

    pidHandle = 0;
    UnmapViewOfFile(lpMemFile);
    CloseHandle(hMapFile);

    return true;
}