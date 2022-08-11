#include "DarksideAPI.h"
#include "simple-inject.h"
#include <format>

struct playerpos_t {
    float pos_x;
    short heading;
    unsigned char unknown[68];
    float pos_y;
    unsigned char unknown2[8];
    float pos_z;
    char unknown4;
};

DarksideAPI::DarksideAPI() {}

DarksideAPI::~DarksideAPI() {}

void DarksideAPI::InjectPid(int pid) {
    simpleInject("C:\\Users\\ritzgames\\Desktop\\daoc\\darkside\\darkside-api\\DarksideGUI\\bin\\Debug\\net6.0-windows\\darkside-hooks.dll", (DWORD)pid);
    std::wstring msg = std::format(L"Injected {}\n", pid);
    MessageBox(0, msg.c_str(), L"Hi", MB_ICONINFORMATION);
    this->pidHandle = pid;
}

bool DarksideAPI::GetPlayerInfo(LPVOID lpBuffer) {
    std::wstring msg = std::format(L"GetPlayerInfo {}\n", this->pidHandle);
    MessageBox(0, msg.c_str(), L"Hi", MB_ICONINFORMATION);
    

#define BUF_SIZE 256
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
        return 1;
    }


    playerpos_t* pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    playerpos_t sPlayerPos = *pPlayerPos;

    memcpy(lpBuffer, &sPlayerPos, sizeof(playerpos_t));

    UnmapViewOfFile(pPlayerPos);

    CloseHandle(hMapFile);

    return true;
}