#include "pch.h"
#include "PlayerPosition.h"
#include "Globals.h"

PlayerPosition::PlayerPosition() {
    //Initialize daocgame pointers
    ptrPlayerPosition = ptrPlayerPosition_x;
    playerPositionInfo = *(playerpos_t**)ptrPlayerPosition;
#ifdef _DEBUG
    std::cout << "PlayerPositionPtr: " << std::hex << (int)ptrPlayerPosition << std::endl;
    std::cout << "Player Position X: " << std::fixed << std::setprecision(0) << playerPositionInfo->pos_z << std::endl;
#endif

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring
    mmf_name = std::to_wstring(pid) + L"_pInfo";
    std::size_t fileSize = sizeof(playerpos_t);

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
    }

    if (hMapFile != 0) {
        pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception
    
    if (pPlayerPos != NULL) {
        *pPlayerPos = *playerPositionInfo;
    }//Todo add exception
    
}

PlayerPosition::~PlayerPosition() {
    UnmapViewOfFile(pPlayerPos);
    CloseHandle(hMapFile);
}

bool PlayerPosition::GetPlayerPosition() {

    *pPlayerPos = *playerPositionInfo;

    return true;
}
