#include "pch.h"
#include "DarksideAPI.h"


bool  DarksideAPI::GetPlayerInfo(LPVOID lpBuffer) {
    //Setup the PlayerInfo mmf
    std::wstring plyrInfommf_name = std::to_wstring(pidHandle) + L"_plyrHp";
    std::size_t fileSize = sizeof(plyrinfo_t);

    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        plyrInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("PlayerInfo Could not create file mapping object (%d).\n"),
            GetLastError());
        memset(lpBuffer, 0, sizeof(plyrinfo_t));
        CloseHandle(hMapFile);
        return false;
    }

    plyrinfo_t* pShmPlayerInfo = (plyrinfo_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);


    if (pShmPlayerInfo == NULL) {
        _tprintf(TEXT("shmPlayerInfo Could not create file mapping object (%d).\n"),
            GetLastError());
        memset(lpBuffer, 0, sizeof(plyrinfo_t));
        UnmapViewOfFile(pShmPlayerInfo);
        CloseHandle(hMapFile);
        return false;

    }
    pShmPlayerInfo->hp;
    memcpy(lpBuffer, pShmPlayerInfo, sizeof(plyrinfo_t));
    UnmapViewOfFile(pShmPlayerInfo);
    CloseHandle(hMapFile);
    return true;
}
