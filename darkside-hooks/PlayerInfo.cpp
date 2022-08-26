#include "pch.h"
#include "PlayerInfo.h"
PlayerInfo::PlayerInfo() {
    //Initialize daocgame pointers
    ptrPlayerHp = (void*)ptrPlayerHp_x;
#ifdef _DEBUG
    std::cout << "ptrPlayerHp: " << std::hex << (int)ptrPlayerHp << std::endl;
#endif

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring

    //Setup the PlayerInfo mmf
    plyrInfommf_name = std::to_wstring(pid) + L"_plyrHp";
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
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (hMapFile != 0) {
        pShmPlayerInfo = (plyrinfo_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (pShmPlayerInfo != NULL) {
        *(plyrinfo_t*)pShmPlayerInfo = *(plyrinfo_t*)ptrPlayerHp;
#ifdef _DEBUG
        std::cout << "plyrHP: " << std::dec << pShmPlayerInfo->hp << std::endl;
        std::cout << "plyrPow: " << std::dec << pShmPlayerInfo->pow << std::endl;
        std::cout << "plyrEndu: " << std::dec << pShmPlayerInfo->endu << std::endl;
#endif
    }//Todo add exception

}

PlayerInfo::~PlayerInfo() {
    UnmapViewOfFile(pShmPlayerInfo);
    CloseHandle(hMapFile);
}

void PlayerInfo::GetPlayerInfo() {
    *(plyrinfo_t*)pShmPlayerInfo = *(plyrinfo_t*)ptrPlayerHp;
}