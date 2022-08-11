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
    
    //poc to pass struct to c#
    //playerpos_t test = { 30000, 300, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 50000, "bbbbbbb", 6000, 'c' };
    //memcpy_s(lpBuffer, sizeof(playerpos_t), &test, sizeof(playerpos_t));
    // 
    
    ////POC to pass struct from inject to C#
    //
    //std::size_t fileSize = sizeof(playerpos_t);
    //const char* fileName = "pid_mmf";
    //
    //boost::interprocess::file_mapping m_file(fileName, boost::interprocess::read_only);
    //
    ////map the whole file with read only
    //boost::interprocess::mapped_region region(m_file, boost::interprocess::read_only);
    //
    ////get address of mapped region
    //void* addr = region.get_address();
    //std::size_t size = region.get_size();
    //
    ////Copy the struct value to lpBuffer
    //memcpy_s(lpBuffer, sizeof(playerpos_t), addr, sizeof(playerpos_t));

#define BUF_SIZE 256
    std::size_t fileSize = sizeof(playerpos_t);
    //const char* fileName = "pid_mmf";
    TCHAR szName[] = TEXT("pid_mmf");

    //HANDLE hMapFile;
    //LPVOID pBuf;

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
    //pBuf = (LPVOID)MapViewOfFile(hMapFile,   // handle to map object
    //    FILE_MAP_ALL_ACCESS, // read/write permission
    //    0,
    //    0,
    //    256);
    //
    //if (pBuf == NULL)
    //{
    //    _tprintf(TEXT("Could not map view of file (%d).\n"),
    //        GetLastError());
    //
    //    CloseHandle(hMapFile);
    //
    //    return 1;
    //}
    //CopyMemory((PVOID)lpBuffer, &pBuf, sizeof(playerpos_t));

    playerpos_t* pPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    playerpos_t sPlayerPos = *pPlayerPos;

    memcpy(lpBuffer, &sPlayerPos, sizeof(playerpos_t));

    UnmapViewOfFile(pPlayerPos);

    CloseHandle(hMapFile);

    return true;
}