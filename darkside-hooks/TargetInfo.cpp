#include "pch.h"
#include "TargetInfo.h"
#include "daochooks.h"

TargetInfo::TargetInfo() {
    //Initialize daocgame pointers
    ptrCurrTargetOffset = ptrCurrentTargetEntOffset_x;
    ptrTargHp = ptrCurrentTargetHp_x;
    ptrTargName = ptrCurrentTargetName_x;

#ifdef _DEBUG
    std::cout << "ptrCurrTargetOffset: " << std::hex << (int)ptrCurrTargetOffset << std::endl;
#endif

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring

    //Setup the PlayerInfo mmf
    targetInfommf_name = std::to_wstring(pid) + L"_targInfo";
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
    }

    if (hMapFile != 0) {
        pShmTargetInfo = (targetInfo_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception


    if (pShmTargetInfo != NULL) {
        pShmTargetInfo->entOffset = *(int*)ptrCurrTargetOffset;
        unsigned char* tempPtr = reinterpret_cast<unsigned char*>(ptrTargHp);
        pShmTargetInfo->tarHp = *(int*)tempPtr;
        pShmTargetInfo->tarColor = *(int*)(tempPtr + 0x4);
        tempPtr = reinterpret_cast<unsigned char*>(ptrTargName);
        memcpy(pShmTargetInfo->tarName, (tempPtr + 0x400), 47);
        //If the name was longer and the last character in the array isn't a null terminator, terminate it
        if (pShmTargetInfo->tarName[47] != '\0') {
            memset((char*)pShmTargetInfo->tarName[47], 0, 1);
        }
        memcpy(pShmTargetInfo->hasTarget, tempPtr + 0x600, 4);
        


#ifdef _DEBUG
        std::cout << "targOffset: " << std::dec << pShmTargetInfo->entOffset << std::endl;
        std::cout << "targHp: " << std::dec << pShmTargetInfo->tarHp << std::endl;
        std::cout << "targColor: " << std::dec << pShmTargetInfo->tarColor << std::endl;
        std::cout << "targName: " << std::dec << pShmTargetInfo->tarName << std::endl;
        std::cout << "hasTarget: " << std::dec << pShmTargetInfo->hasTarget << std::endl;

#endif
    }//Todo add exception

    //set up skill and spell casting function shaerd memory
    setTargmmf_name = std::to_wstring(pid) + L"_SetTarg";
    std::size_t setTargFileSize = sizeof(int);

    auto hSetTargFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        setTargFileSize,                // maximum object size (low-order DWORD)
        setTargmmf_name.c_str());                 // name of mapping object

    if (hSetTargFile == NULL)
    {
        _tprintf(TEXT("Set Target Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (hSetTargFile != 0) {
        pShmSetTarget = (int*)MapViewOfFile(hSetTargFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    //Set to -1 while waiting for target
    *pShmSetTarget = -1;

}

TargetInfo::~TargetInfo() {
    UnmapViewOfFile(pShmTargetInfo);
    CloseHandle(hMapFile);
    UnmapViewOfFile(pShmSetTarget);
    CloseHandle(hSetTargFile);
}

bool TargetInfo::GetTargetInfo() {
    if (pShmTargetInfo) {
        pShmTargetInfo->entOffset = *(int*)ptrCurrTargetOffset;
        unsigned char* tempPtr = reinterpret_cast<unsigned char*>(ptrTargHp);
        pShmTargetInfo->tarHp = *(int*)tempPtr;
        pShmTargetInfo->tarColor = *(int*)(tempPtr + 0x4);
        tempPtr = reinterpret_cast<unsigned char*>(ptrTargName);
        memcpy(pShmTargetInfo->tarName, (tempPtr + 0x400), 47);
        //If the name was longer and the last character in the array isn't a null terminator, terminate it
        if (pShmTargetInfo->tarName[47] != '\0') {
            memset((char*)pShmTargetInfo->tarName[47], 0, 1);
        }
        memcpy(pShmTargetInfo->hasTarget, tempPtr + 0x600, 4);
        return true;
    }
    else {
        return false;
    }
}

void TargetInfo::SetTarget() {
    if (*pShmSetTarget >= 0) {
        daoc::SetTarget(*pShmSetTarget, 0);
        daoc::SetTargetUI();
        *pShmSetTarget = -1;
    }
}