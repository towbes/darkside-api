#include "pch.h"
#include "DarksideAPI.h"


bool DarksideAPI::GetTargetInfo(LPVOID lpBuffer) {
    //Setup the PlayerInfo mmf
    std::wstring targetInfommf_name = std::to_wstring(pidHandle) + L"_targInfo";
    std::size_t fileSize = sizeof(targetInfo_t);

    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        targetInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("TargetInfo Could not create file mapping object (%d).\n"),
            GetLastError());
        memset(lpBuffer, 0, sizeof(targetInfo_t));
        return false;
    }

    targetInfo_t* pShmTargetInfo = (targetInfo_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);


    if (pShmTargetInfo == NULL) {

        _tprintf(TEXT("TargetInfo Could not map file (%d).\n"),
            GetLastError());
        memset(lpBuffer, 0, sizeof(targetInfo_t));
        CloseHandle(hMapFile);
        return false;
    }
        //struct targetInfo_t {
        //    int entOffset;
        //    int tarHp;
        //    int tarColor;
        //    char tarName[48];
        //    char hasTarget[4];
        //};
    if (pShmTargetInfo->entOffset > -1) {
        memcpy(lpBuffer, pShmTargetInfo, sizeof(targetInfo_t));
    }
    else {
        memset(lpBuffer, 0, sizeof(targetInfo_t));
    }
    
    UnmapViewOfFile(pShmTargetInfo);
    CloseHandle(hMapFile);
    return true;
}

bool DarksideAPI::SetTarget(int entIndex) {
    //set up skill and spell casting function shaerd memory
    std::wstring setTargmmf_name = std::to_wstring(pidHandle) + L"_SetTarg";
    std::size_t useSkillfileSize = sizeof(int);

    auto hSetTargFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        useSkillfileSize,                // maximum object size (low-order DWORD)
        setTargmmf_name.c_str());                 // name of mapping object

    if (hSetTargFile == NULL)
    {
        _tprintf(TEXT("Set Target Could not create file mapping object (%d).\n"),
            GetLastError());
        return false;
    }

    int* pShmSetTarget = (int*)MapViewOfFile(hSetTargFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (*pShmSetTarget == -1) {
        *pShmSetTarget = entIndex;
        UnmapViewOfFile(pShmSetTarget);
        CloseHandle(hSetTargFile);
        return true;
    }
    UnmapViewOfFile(pShmSetTarget);
    CloseHandle(hSetTargFile);
    return false;
}