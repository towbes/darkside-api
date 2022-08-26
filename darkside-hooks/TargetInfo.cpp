#include "pch.h"
#include "TargetInfo.h"
#include "daochooks.h"

TargetInfo::TargetInfo() {
    //Initialize daocgame pointers
    ptrCurrTargetOffset = ptrCurrentTargetEntOffset_x;

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


#ifdef _DEBUG

#endif
    }//Todo add exception

    //set up skill and spell casting function shaerd memory
    setTargmmf_name = std::to_wstring(pid) + L"_SetTarg";
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
    }

    if (hSetTargFile != 0) {
        pShmSetTarget = (int)MapViewOfFile(hSetTargFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    //Set to -1 while waiting for target
    pShmSetTarget = -1;

}

TargetInfo::~TargetInfo() {
    UnmapViewOfFile(pShmTargetInfo);
    CloseHandle(hMapFile);
    UnmapViewOfFile((LPCVOID)pShmSetTarget);
    CloseHandle(hSetTargFile);
}