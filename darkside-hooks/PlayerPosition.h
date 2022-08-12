#pragma once
#include "DaocStructs.h"

extern "C" class __declspec(dllexport) PlayerPosition {
private:
    //Current process id
    int pid;

    //Player Position Info
    playerpos_t* playerPositionInfo;
    uintptr_t ptrPlayerPosition;
    playerpos_t* pPlayerPos;
    void* hMapFile;
    std::wstring posInfommf_name;

    //Player autorun toggle
    void* arunMapFile;
    std::wstring arunmmf_name;
    BYTE valAutorunToggle;
    BYTE* shmAutorunToggle;
    BYTE* ptrAutorunToggle;

public:
    PlayerPosition();
    ~PlayerPosition();

    //Player Info
    bool GetPlayerPosition();

    void SetAutorun();
};



//moving this out temporarily
//std::size_t fileSize = sizeof(playerpos_t);
//TCHAR szName[] = TEXT("pid_mmf");
//
//
//
//auto hMapFile = CreateFileMapping(
//    INVALID_HANDLE_VALUE,    // use paging file
//    NULL,                    // default security
//    PAGE_READWRITE,          // read/write access
//    0,                       // maximum object size (high-order DWORD)
//    fileSize,                // maximum object size (low-order DWORD)
//    szName);                 // name of mapping object
//
//if (hMapFile == NULL)
//{
//    _tprintf(TEXT("Could not create file mapping object (%d).\n"),
//        GetLastError());
//    return 1;
//}
//
//playerpos_t* pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
//
//*pPlayerPos = *playerPositionInfo;
//
//UnmapViewOfFile(pPlayerPos);
//
//CloseHandle(hMapFile);