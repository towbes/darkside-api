#include "pch.h"
#include "DarksideAPI.h"



bool DarksideAPI::GetPlayerPosition(LPVOID lpBuffer) {

    std::size_t fileSize = sizeof(playerpos_t);
    std::wstring mmf_name = std::to_wstring(pidHandle) + L"_pInfo";


    auto hPlayerPosFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        mmf_name.c_str());                 // name of mapping object

    if (hPlayerPosFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        if (hPlayerPosFile != NULL) {
            CloseHandle(hPlayerPosFile);
        }
        memset(lpBuffer, 0, sizeof(playerpos_t));
        return false;
    }


    playerpos_t* pPlayerPos = (playerpos_t*)MapViewOfFile(hPlayerPosFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (pPlayerPos == NULL) {
        _tprintf(TEXT("Could not find pPlayerPos (%d).\n"),
            GetLastError());
        if (pPlayerPos != NULL) {
            UnmapViewOfFile(pPlayerPos);
        }
        if (hPlayerPosFile != NULL) {
            CloseHandle(hPlayerPosFile);
        }
        memset(lpBuffer, 0, sizeof(playerpos_t));
        return false;
    }

    playerpos_t sPlayerPos = *pPlayerPos;

    memcpy(lpBuffer, &sPlayerPos, sizeof(playerpos_t));
    if (pPlayerPos != NULL) {
        UnmapViewOfFile(pPlayerPos);
    }
    if (hPlayerPosFile != NULL) {
        CloseHandle(hPlayerPosFile);
    }

    return true;
}

bool DarksideAPI::SetPlayerHeading(bool changeHeading, short newHeading) {

    //Setup the MMF
        //setup the heading overwrite flag mmf
    std::wstring posUpdateMmf_name = std::to_wstring(pidHandle) + L"_posUpdate";
    std::size_t posUpdateFilesize = sizeof(positionUpdate_t);


    auto posUpdateMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        posUpdateFilesize,                // maximum object size (low-order DWORD)
        posUpdateMmf_name.c_str());                 // name of mapping object

    if (posUpdateMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        if (posUpdateMapFile != NULL) {
            CloseHandle(posUpdateMapFile);
        }

        return false;
    }

    positionUpdate_t* posUpdate = (positionUpdate_t*)MapViewOfFile(posUpdateMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    //Todo add exception

    if (posUpdate == NULL) {
        _tprintf(TEXT("Could not create map view object (%d).\n"),
            GetLastError());
        if (posUpdate != NULL) {
            UnmapViewOfFile(posUpdate);
        }
        if (posUpdateMapFile != NULL) {
            CloseHandle(posUpdateMapFile);
        }
        return false;
    }

    if (posUpdate->changeHeading == false) {
        posUpdate->changeHeading = changeHeading;
        posUpdate->newHeading = newHeading;
    }

    //clean up the shared mem
    if (posUpdate != NULL) {
        UnmapViewOfFile(posUpdate);
    }
    if (posUpdateMapFile != NULL) {
        CloseHandle(posUpdateMapFile);
    }

    return true;
}

bool DarksideAPI::SetPlayerFwdSpeed(bool changeSpeed, float newSpeed) {

    //Setup the MMF
        //setup the heading overwrite flag mmf
    std::wstring posUpdateMmf_name = std::to_wstring(pidHandle) + L"_posUpdate";
    std::size_t posUpdateFilesize = sizeof(positionUpdate_t);


    auto posUpdateMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        posUpdateFilesize,                // maximum object size (low-order DWORD)
        posUpdateMmf_name.c_str());                 // name of mapping object

    if (posUpdateMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        if (posUpdateMapFile != NULL) {
            CloseHandle(posUpdateMapFile);
        }

        return false;
    }

    positionUpdate_t* posUpdate = (positionUpdate_t*)MapViewOfFile(posUpdateMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    //Todo add exception

    if (posUpdate == NULL) {
        _tprintf(TEXT("Could not create map view object (%d).\n"),
            GetLastError());
        if (posUpdate != NULL) {
            UnmapViewOfFile(posUpdate);
        }
        if (posUpdateMapFile != NULL) {
            CloseHandle(posUpdateMapFile);
        }
        return false;
    }

    if (posUpdate->changeFwd == false) {
        posUpdate->changeFwd = changeSpeed;
        posUpdate->newFwd = newSpeed;
    }



    //clean up the shared mem
    if (posUpdate != NULL) {
        UnmapViewOfFile(posUpdate);
    }
    if (posUpdateMapFile != NULL) {
        CloseHandle(posUpdateMapFile);
    }

    return true;
}

bool DarksideAPI::SetPlayerStrafeSpeed(bool changeSpeed, float newSpeed) {

    //Setup the MMF
        //setup the heading overwrite flag mmf
    std::wstring posUpdateMmf_name = std::to_wstring(pidHandle) + L"_posUpdate";
    std::size_t posUpdateFilesize = sizeof(positionUpdate_t);


    auto posUpdateMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        posUpdateFilesize,                // maximum object size (low-order DWORD)
        posUpdateMmf_name.c_str());                 // name of mapping object

    if (posUpdateMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        if (posUpdateMapFile != NULL) {
            CloseHandle(posUpdateMapFile);
        }

        return false;
    }

    positionUpdate_t* posUpdate = (positionUpdate_t*)MapViewOfFile(posUpdateMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    //Todo add exception

    if (posUpdate == NULL) {
        _tprintf(TEXT("Could not create map view object (%d).\n"),
            GetLastError());
        if (posUpdate != NULL) {
            UnmapViewOfFile(posUpdate);
        }
        if (posUpdateMapFile != NULL) {
            CloseHandle(posUpdateMapFile);
        }
        return false;
    }

    if (posUpdate->changeStrafe == false) {
        posUpdate->changeStrafe = changeSpeed;
        posUpdate->newStrafe = newSpeed;
    }


    //clean up the shared mem
    if (posUpdate != NULL) {
        UnmapViewOfFile(posUpdate);
    }
    if (posUpdateMapFile != NULL) {
        CloseHandle(posUpdateMapFile);
    }

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
        if (arunMapFile != NULL) {
            CloseHandle(arunMapFile);
        }
        return false;
    }

    BYTE* shmAutorunToggle = (BYTE*)MapViewOfFile(arunMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (shmAutorunToggle == NULL) {
        if (shmAutorunToggle != NULL) {
            UnmapViewOfFile(shmAutorunToggle);
        }
        if (arunMapFile != NULL) {
            CloseHandle(arunMapFile);
        }
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
    if (shmAutorunToggle != NULL) {
        UnmapViewOfFile(shmAutorunToggle);
    }
    if (arunMapFile != NULL) {
        CloseHandle(arunMapFile);
    }
}
