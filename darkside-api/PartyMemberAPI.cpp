#include "pch.h"
#include "DarksideAPI.h"

bool DarksideAPI::GetPartyMember(int memberIndex, LPVOID lpBuffer) {
    //Setup the PlayerInfo mmf
    std::wstring partyInfommf_name = std::to_wstring(pidHandle) + L"_pMemInfo";
    //Set the size for 8 party members
    std::size_t fileSize = sizeof(partymembers_t);
    partymembers_t* ptrPartyMembers = NULL;

    //reopen the shared memory
    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        partyInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        memset(lpBuffer, 0, sizeof(partymemberinfo_t));
        return false;
    }

    if (hMapFile != 0) {
        ptrPartyMembers = (partymembers_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (ptrPartyMembers == NULL) {
        _tprintf(TEXT("Could not create map view object (%d).\n"),
            GetLastError());
        memset(lpBuffer, 0, sizeof(partymemberinfo_t));
        CloseHandle(hMapFile);
        return false;
    }
    //cast as unsigned char* to be able to use single byte offsets to move the pointer
    unsigned char* ptrShmBytePtr = reinterpret_cast<unsigned char*>(ptrPartyMembers);

    ptrShmBytePtr += sizeof(partymemberinfo_t) * memberIndex;
    //dereference the partymember_info data at memberIndex offset into a new object
    partymemberinfo_t sPartyMemberInfo = *(partymemberinfo_t*)ptrShmBytePtr;
    //copy that object to the buffer
    memcpy(lpBuffer, &sPartyMemberInfo, sizeof(partymemberinfo_t));
    //close the handles since we don't need them until the next call
    UnmapViewOfFile(ptrPartyMembers);
    CloseHandle(hMapFile);

    return true;
}