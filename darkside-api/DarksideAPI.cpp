#include "DarksideAPI.h"
#include "simple-inject.h"
#include <format>

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
    //Get the current directory
    CHAR buffer[MAX_PATH] = { 0 };
    GetModuleFileNameA(NULL, buffer, MAX_PATH);
    std::wstring::size_type pos = std::string(buffer).find_last_of("\\/");
    std::string currPath = std::string(buffer).substr(0, pos);
    //append darkside-hooks.dll
    currPath = currPath + "\\darkside-hooks.dll";
    simpleInject(currPath.c_str(), (DWORD)pid);
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
            if (hThread != 0) {
                ResumeThread(hThread);
            }
            else {
                _tprintf(TEXT("Could not create remote thread (%d).\n"),
                    GetLastError());
            }
           
        }
    }
}

bool DarksideAPI::GetPlayerPosition(LPVOID lpBuffer) {
   

    std::size_t fileSize = sizeof(playerpos_t);
    std::wstring mmf_name = std::to_wstring(pidHandle) + L"_pInfo";


    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        mmf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        return false;
    }


    playerpos_t* pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (pPlayerPos == NULL) {
        _tprintf(TEXT("Could not find pPlayerPos (%d).\n"),
            GetLastError());
        return false;
    }
    
    playerpos_t sPlayerPos = *pPlayerPos;

    memcpy(lpBuffer, &sPlayerPos, sizeof(playerpos_t));
    UnmapViewOfFile(pPlayerPos);
    CloseHandle(hMapFile);

    return true;
}

bool DarksideAPI::SetPlayerHeading(bool changeHeading, short newHeading) {
    //Setup the MMF
        //setup the heading overwrite flag mmf
    std::wstring headingmmf_name = std::to_wstring(pidHandle) + L"_heading";
    std::size_t headingFileSize = sizeof(headingupdate_t);


    auto headingMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        headingFileSize,                // maximum object size (low-order DWORD)
        headingmmf_name.c_str());                 // name of mapping object

    if (headingMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        return false;
    }

    headingupdate_t* headingUpdate = (headingupdate_t*)MapViewOfFile(headingMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    //Todo add exception

    if (headingUpdate == NULL) {
        _tprintf(TEXT("Could not create map view object (%d).\n"),
            GetLastError());
        return false;
    }

    headingUpdate->changeHeading = changeHeading;
    headingUpdate->heading = newHeading;

    //clean up the shared mem
    UnmapViewOfFile(headingUpdate);
    CloseHandle(headingMapFile);

    return true;
}

bool DarksideAPI::SetAutorun(bool autorun) {
    std::wstring posInfommf_name = std::to_wstring(pidHandle) + L"_arun";
    std::size_t fileSize = sizeof(BYTE);

    //open the shared memory
    auto arunMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        posInfommf_name.c_str());                 // name of mapping object

    if (arunMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (arunMapFile == 0) {
        //Todo add exception
        return false;
    }
    BYTE* shmAutorunToggle = (BYTE*)MapViewOfFile(arunMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (shmAutorunToggle == NULL) {
        return false;
    }

    //Set autorun to 1 if true and 0 if false
    if (autorun) {
        *(BYTE*)shmAutorunToggle = 0x1;
    }
    else {
        *(BYTE*)shmAutorunToggle = 0x0;
    }
    
    //clean up the shared mem
    UnmapViewOfFile(shmAutorunToggle);
    CloseHandle(arunMapFile);
}

bool DarksideAPI::GetPartyMember(int memberIndex, LPVOID lpBuffer) {
    //Setup the PlayerInfo mmf
    std::wstring partyInfommf_name = std::to_wstring(pidHandle) + L"_pMemInfo";
    //Set the size for 8 party members
    std::size_t fileSize = sizeof(partymembers_t);
    partymembers_t* ptrPartyMembers = NULL;

    //reopen the shared memory
    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        partyInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        return false;
    }

    if (hMapFile != 0) {
        ptrPartyMembers = (partymembers_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (ptrPartyMembers == NULL) {
        _tprintf(TEXT("Could not create map view object (%d).\n"),
            GetLastError());
        return false;
    }
    //cast as unsigned char* to be able to use single byte offsets to move the pointer
    unsigned char* ptrShmBytePtr = reinterpret_cast<unsigned char*>(ptrPartyMembers);

    ptrShmBytePtr += sizeof(partymemberinfo_t) * memberIndex;
    //dereference the partymember_info data at memberIndex offset into a new object
    partymemberinfo_t sPartyMemberInfo = *(partymemberinfo_t*)ptrShmBytePtr;
    //copy that object to the buffer
    memcpy(lpBuffer, &sPartyMemberInfo, sizeof(partymemberinfo_t));
    //close the handles since we don't need them until the next call
    UnmapViewOfFile(ptrPartyMembers);
    CloseHandle(hMapFile);

    return true;
}