#include "pch.h"
#include "PartyMemberInfo.h"
#include "Globals.h"

PartyMemberInfo::PartyMemberInfo() {
    //Initialize daocgame pointers
    ptrPartyMemberInfo = ptrPartyMemberInfo_x;
    //partyMemberInfo = *(partymemberinfo_t**)ptrPartyMemberInfo;
#ifdef _DEBUG
    std::cout << "ptrPartyMemberInfo: " << std::hex << (int)ptrPartyMemberInfo << std::endl;
#endif

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring

    //Setup the PlayerInfo mmf
    partyInfommf_name = std::to_wstring(pid) + L"_pMemInfo";
    //Set the size for 8 party members
    std::size_t fileSize = sizeof(partymembers_t);

    //Create a handle to memory mapped file
    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        partyInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Party Info Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (hMapFile != 0) {
        //Map a view of the memory mapped file from above
        ptrShmPartyMembers = (partymembers_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (ptrShmPartyMembers != NULL) {
        //Create a new pointer and cast as unsigned char to be able to offset by single byte values
        unsigned char* ptrShmBytePtr = reinterpret_cast<unsigned char*>(ptrShmPartyMembers);
        unsigned char* ptrPInfoBytePtr = reinterpret_cast<unsigned char*>(ptrPartyMemberInfo);
        //Copy the 8 partymemberinfo_t structures into the shared memory
        for (int i = 0; i < 8; i++) {
            //copy party member info
            *(partymemberinfo_t*)ptrShmBytePtr = *(partymemberinfo_t*)ptrPInfoBytePtr;
            //Move offset to next party member
            ptrShmBytePtr += sizeof(partymemberinfo_t);
            ptrPInfoBytePtr += sizeof(partymemberinfo_t);
        }
    } else {
    }//Todo add exception
}

PartyMemberInfo::~PartyMemberInfo() {
    UnmapViewOfFile(ptrShmPartyMembers);
    CloseHandle(hMapFile);
}

bool PartyMemberInfo::GetPartyMembers() {

    if (ptrShmPartyMembers != NULL) {
        //Create a new pointer and cast as unsigned char to be able to offset by single byte values
        unsigned char* ptrShmBytePtr = reinterpret_cast<unsigned char*>(ptrShmPartyMembers);
        unsigned char* ptrPInfoBytePtr = reinterpret_cast<unsigned char*>(ptrPartyMemberInfo);
        //Copy the 8 partymemberinfo_t structures into the shared memory
        for (int i = 0; i < 8; i++) {
            //copy party member info
            *(partymemberinfo_t*)ptrShmBytePtr = *(partymemberinfo_t*)ptrPInfoBytePtr;
            //Move offset to next party member
            ptrShmBytePtr += sizeof(partymemberinfo_t);
            ptrPInfoBytePtr += sizeof(partymemberinfo_t);
        }
    }//Todo add exception
#ifdef _DEBUG
    //unsigned char* ptrShmBytePtr2 = reinterpret_cast<unsigned char*>(ptrShmPartyMembers);
    //ptrShmBytePtr2 += sizeof(partymemberinfo_t);
    //std::cout << "partymemberHP: " << std::hex << *(int*)ptrShmBytePtr2 << std::endl;
#endif
    return true;
}
