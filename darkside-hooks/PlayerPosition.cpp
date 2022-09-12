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
        //Create new playerPositionInfo in order to update x/y offsets
        zoneYoffset = *(float*)ptrZoneYoffset_x;
        zoneXoffset = *(float*)ptrZoneXoffset_x;
        playerpos_t tempPos = *playerPositionInfo;
        tempPos.pos_x = tempPos.pos_x - zoneXoffset;
        tempPos.pos_y = tempPos.pos_y - zoneYoffset;
        tempPos.heading = (((((tempPos.heading + 0xcb6) + 0x800) * 0x168) / 0x1000) % 0x168);
        *pShmPlayerPos = tempPos;
    }//Todo add exception

    //setup the heading overwrite flag mmf
    posUpdateMmf_name = std::to_wstring(pid) + L"_posUpdate";
    std::size_t headingFileSize = sizeof(positionUpdate_t);


    auto posUpdateMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        headingFileSize,                // maximum object size (low-order DWORD)
        posUpdateMmf_name.c_str());                 // name of mapping object

    if (posUpdateMapFile == NULL)
    {
        _tprintf(TEXT("Heading Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (posUpdateMapFile != 0) {
        posUpdate = (positionUpdate_t*)MapViewOfFile(posUpdateMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    posUpdate->changeHeading = false;
    posUpdate->changeFwd = false;
    posUpdate->changeStrafe = false;
    posUpdate->newHeading = 0;
    posUpdate->newFwd = 0;
    posUpdate->newStrafe = 0;
    
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
    UnmapViewOfFile(posUpdate);
    CloseHandle(posUpdateMapFile);
}

bool PlayerPosition::GetPlayerPosition() {
    //Lock the pointer when we read it
    //std::scoped_lock<std::mutex> lg(posUpdateMutex);
    zoneYoffset = *(float*)ptrZoneYoffset_x;
    zoneXoffset = *(float*)ptrZoneXoffset_x;
    playerpos_t tempPos = *playerPositionInfo;
    tempPos.pos_x = tempPos.pos_x - zoneXoffset;
    tempPos.pos_y = tempPos.pos_y - zoneYoffset;
    tempPos.heading = (((((tempPos.heading + 0xcb6) + 0x800) * 0x168) / 0x1000) % 0x168);
    *pShmPlayerPos = tempPos;
    return true;
}

void PlayerPosition::SetHeading() {
    if ((bool)posUpdate->changeHeading) {
        //lock the pointer when we write it
        //std::scoped_lock<std::mutex> lg(posUpdateMutex);
        //convert radian to heading
        //https://github.com/Dawn-of-Light/DOLSharp/blob/9af87af011497c3fda852559b01a269c889b162e/GameServer/world/Point2D.cs
        //short oHeading = posUpdate->heading * (4096.0 / 360.0);
        //S = 0/4096  E = 3072  N = 2048  W = 1024
        //posUpdate->heading * 0x1000 / 0x168 
        playerPositionInfo->heading = posUpdate->newHeading;
    }
    if ((bool)posUpdate->changeFwd) {
        playerPositionInfo->momentumFwdBackWrite = posUpdate->newFwd;
    }
    if ((bool)posUpdate->changeStrafe) {
        playerPositionInfo->momentumLeftRight = posUpdate->newStrafe;
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
        //If we're changing to 0, also set our forward momentum to 0
        *ptrAutorunToggle = *shmAutorunToggle;
        preValAutorunToggle = *(BYTE*)shmAutorunToggle;
        if (*shmAutorunToggle == 0) {
            //std::scoped_lock<std::mutex> lg(posUpdateMutex);
            playerPositionInfo->momentumFwdBackWrite = 0;
            //int prevSpeed = playerPositionInfo->playerSpeedFwd;
            //playerPositionInfo->playerSpeedFwd = 0;
            //playerPositionInfo->playerSpeedFwd = prevSpeed;
        }
        return true;
    }
    return false;
    
#ifdef _DEBUG
    //std::cout << "valAutorunToggle: " << std::hex << (int)valAutorunToggle << std::endl;
    //std::cout << "shmAutorunToggle: " << std::hex << *(int*)shmAutorunToggle << std::endl;
#endif
}

