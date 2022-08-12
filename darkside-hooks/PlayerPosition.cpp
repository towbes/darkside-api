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
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (hMapFile != 0) {
        pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception
    
    if (pPlayerPos != NULL) {
        *pPlayerPos = *playerPositionInfo;
    }//Todo add exception
    
    //Setup the autorun toggle mmf
    ptrAutorunToggle = (BYTE*)ptrAutorunToggle2_x;
    valAutorunToggle = *(BYTE*)ptrAutorunToggle;
#ifdef _DEBUG
    std::cout << "ptrAutorunToggle: " << std::hex << (int)ptrAutorunToggle << std::endl;
    std::cout << "valAutorunToggle: " << std::hex << (int)valAutorunToggle << std::endl;
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
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (arunMapFile != 0) {
        shmAutorunToggle = (BYTE*)MapViewOfFile(arunMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (shmAutorunToggle != NULL) {
        *shmAutorunToggle = valAutorunToggle;
    }//Todo add exception
}

PlayerPosition::~PlayerPosition() {
    UnmapViewOfFile(pPlayerPos);
    CloseHandle(hMapFile);
    UnmapViewOfFile(shmAutorunToggle);
    CloseHandle(arunMapFile);
}

bool PlayerPosition::GetPlayerPosition() {

    *pPlayerPos = *playerPositionInfo;

    return true;
}

void PlayerPosition::SetAutorun() {
    valAutorunToggle = *(BYTE*)shmAutorunToggle;
    *ptrAutorunToggle = *shmAutorunToggle;
#ifdef _DEBUG
    //std::cout << "valAutorunToggle: " << std::hex << (int)valAutorunToggle << std::endl;
    //std::cout << "shmAutorunToggle: " << std::hex << *(int*)shmAutorunToggle << std::endl;
#endif
}