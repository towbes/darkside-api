#include "pch.h"
#include "PlayerInfo.h"
#include "Globals.h"

PlayerInfo::PlayerInfo() {
    //Initialize daocgame pointers
    ptrPlayerPosition = ptrPlayerPosition_x;
    playerPositionInfo = *(playerpos_t**)ptrPlayerPosition;
#ifdef _DEBUG
    std::cout << "PlayerPositionPtr: " << std::hex << (int)ptrPlayerPosition << std::endl;
    std::cout << "Player Position X: " << std::fixed << std::setprecision(0) << playerPositionInfo->pos_z << std::endl;
#endif

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
    }

    pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    *pPlayerPos = *playerPositionInfo;
}

PlayerInfo::~PlayerInfo() {
    UnmapViewOfFile(pPlayerPos);
    CloseHandle(hMapFile);
}

bool PlayerInfo::GetPlayerInfo() {

    *pPlayerPos = *playerPositionInfo;

    return true;
}
