#include "pch.h"
#include "PlayerPosition.h"
#include "Globals.h"

PlayerPosition::PlayerPosition() {
    //Initialize daocgame pointers
    ptrPlayerPosition = ptrPlayerPosition_x;
    playerPositionInfo = *(playerpos_t**)ptrPlayerPosition;
#ifdef _DEBUG
    std::cout << "PlayerPositionPtr: " << std::hex << (int)ptrPlayerPosition << std::endl;
    std::cout << "Player Position Z: " << std::fixed << std::setprecision(0) << playerPositionInfo->pos_z << std::endl;
#endif

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring
    
    //Setup the PlayerInfo mmf
    posInfommf_name = std::to_wstring(pid) + L"_pInfo";
    std::size_t fileSize = sizeof(playerpos_t);

    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        posInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Plyr Pos Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (hMapFile != 0) {
        pShmPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception
    
    if (pShmPlayerPos != NULL) {
        *pShmPlayerPos = *playerPositionInfo;
    }//Todo add exception

    //setup the heading overwrite flag mmf
    headingmmf_name = std::to_wstring(pid) + L"_heading";
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
        _tprintf(TEXT("Heading Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (headingMapFile != 0) {
        headingUpdate = (headingupdate_t*)MapViewOfFile(headingMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception


    headingUpdate->changeHeading = false;
    headingUpdate->heading = 0;
    
    //Setup the autorun toggle mmf
    ptrAutorunToggle = (BYTE*)ptrAutorunToggle2_x;
    preValAutorunToggle = *(BYTE*)ptrAutorunToggle;
#ifdef _DEBUG
    std::cout << "ptrAutorunToggle: " << std::hex << (int)ptrAutorunToggle << std::endl;
    std::cout << "valAutorunToggle: " << std::hex << (int)preValAutorunToggle << std::endl;
#endif
    posInfommf_name = std::to_wstring(pid) + L"_arun";
    std::size_t arunfileSize = sizeof(BYTE);

    auto arunMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        arunfileSize,                // maximum object size (low-order DWORD)
        posInfommf_name.c_str());                 // name of mapping object

    if (arunMapFile == NULL)
    {
        _tprintf(TEXT("Autorun Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (arunMapFile != 0) {
        shmAutorunToggle = (BYTE*)MapViewOfFile(arunMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (shmAutorunToggle != NULL) {
        *shmAutorunToggle = preValAutorunToggle;
    }//Todo add exception

}

PlayerPosition::~PlayerPosition() {
    UnmapViewOfFile(pShmPlayerPos);
    CloseHandle(hMapFile);
    UnmapViewOfFile(shmAutorunToggle);
    CloseHandle(arunMapFile);
    UnmapViewOfFile(headingUpdate);
    CloseHandle(headingMapFile);
}

bool PlayerPosition::GetPlayerPosition() {
    //Lock the pointer when we read it
    std::lock_guard<std::mutex> lg(posUpdateMutex);
    *pShmPlayerPos = *playerPositionInfo;
    return true;
}

void PlayerPosition::SetHeading() {
    if ((bool)headingUpdate->changeHeading) {
        //lock the pointer when we write it
        std::lock_guard<std::mutex> lg(posUpdateMutex);
        playerPositionInfo->heading = headingUpdate->heading;
    }
}

void PlayerPosition::GetAutorun() {
    *shmAutorunToggle = *ptrAutorunToggle;
}



bool PlayerPosition::SetAutorun() {
    //Only change the autorun value if the shared memory changed
    //This lets the player still use in game autorun
    if ((BYTE)preValAutorunToggle == *(BYTE*)shmAutorunToggle) {
        //do nothing
        return false;
    }
    else if (preValAutorunToggle != *shmAutorunToggle) {
        *ptrAutorunToggle = *shmAutorunToggle;
        preValAutorunToggle = *(BYTE*)shmAutorunToggle;
        return true;
    }
    return false;
    
#ifdef _DEBUG
    //std::cout << "valAutorunToggle: " << std::hex << (int)valAutorunToggle << std::endl;
    //std::cout << "shmAutorunToggle: " << std::hex << *(int*)shmAutorunToggle << std::endl;
#endif
}

