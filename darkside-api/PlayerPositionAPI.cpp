#include "pch.h"
#include "DarksideAPI.h"


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