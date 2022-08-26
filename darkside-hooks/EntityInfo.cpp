#include "pch.h"
#include "EntityInfo.h"
#include "daochooks.h"

EntityInfo::EntityInfo() {
    //Initialize daocgame pointers
    ptrEntityListMax = ptrEntityListMax_x;
#ifdef _DEBUG
    std::cout << "ptrEntityListMax: " << std::hex << (int)ptrEntityListMax << std::endl;
#endif

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring

    //Setup the Ent Info mmf
    entInfommf_name = std::to_wstring(pid) + L"_EntInfo";
    //Set the size for 8 party members
    std::size_t fileSize = sizeof(entity_t) * 2000;

    //Create a handle to memory mapped file
    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        entInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (hMapFile != 0) {
        //Map a view of the memory mapped file from above
        ptrShmEntities = (entity_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    //Setup the EntName mmf
    entNamemmf_name = std::to_wstring(pid) + L"_EntName";
    //Set the size for 8 party members
    std::size_t namefileSize = sizeof(entName_t) * 2000;

    //Create a handle to memory mapped file
    auto entNameFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        namefileSize,                // maximum object size (low-order DWORD)
        entNamemmf_name.c_str());                 // name of mapping object

    if (entNameFile == NULL)
    {
        _tprintf(TEXT("EntityInfo Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (entNameFile != 0) {
        //Map a view of the memory mapped file from above
        ptrShmEntNames = (entName_t*)MapViewOfFile(entNameFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (ptrShmEntities != NULL && ptrShmEntNames != NULL) {
        //Create a new pointer and cast as unsigned char to be able to offset by single byte values
        //Stores entity pointer from game function
        uintptr_t ptrEntInfoBytePtr;
        //Recast in order to offset the shared memory pointer
        unsigned char* ptrShmBytePtr = reinterpret_cast<unsigned char*>(ptrShmEntities);
        unsigned char* ptrEntNameShmBytePtr = reinterpret_cast<unsigned char*>(ptrShmEntNames);
        //Offsets for the shared memory
        int entoffset = 0;
        int nameoffset = 0;
        //current max of the entity list
        entityListMax = *(int*)ptrEntityListMax;
        //update the zone offset values
        zoneYoffset = *(float*)ptrZoneYoffset_x;
        zoneXoffset = *(float*)ptrZoneXoffset_x;
        //Copy the entity_t structures into the shared memory
        for (int i = 0; i < entityListMax; i++) {
            //reset the entity pointer
            ptrEntInfoBytePtr = 0;
            //use game function to check if there is a valid entity at this offset
            if (daoc::EntityPtrSanityCheck(i)) {
                //Get the entity pointer for this offset from game function
                ptrEntInfoBytePtr = daoc::GetEntityPointer(i);
                if (ptrEntInfoBytePtr) {
                    //copy the entity to shared memory
                    memcpy(ptrShmBytePtr + entoffset, (entity_t*)ptrEntInfoBytePtr, sizeof(entity_t));
                    //Use game function to write the entity name to shared memory
                    daoc::GetEntityName(3, i, reinterpret_cast<entName_t*>((ptrEntNameShmBytePtr + nameoffset))->name, 48);
                    unsigned char* tempPtr = reinterpret_cast<unsigned char*>((ptrShmBytePtr + entoffset));
                    const auto type = *(uint8_t*)(tempPtr + 0x28e);
                    const auto objId = *(uint16_t*)(tempPtr + 0x23c);
                    const auto level = ((*(uint32_t*)(tempPtr + 0x60) ^ 0xCB96) / 74) - 23;//unencode level: ((*(uint32_t*)(tempPtr + 0x60) ^ 0xCB96)/74) - 23
                    const auto health = (*(uint32_t*)(tempPtr + 0x228) ^ 0xbe00) / 0x22 - 0x23;//unencode health: (*(uint32_t*)(tempPtr + 0x228) ^ 0xbe00) / 0x22 - 0x23
                    const auto posx = *(float*)(tempPtr + 0x48) - zoneXoffset;
                    const auto posy = *(float*)(tempPtr + 0x370) - zoneYoffset;
                    const auto posz = *(float*)(tempPtr + 0xE7C);
                    const auto heading = ((((*(uint16_t*)(tempPtr + 0xcb6) + 0x800) * 0x168) / 0x1000) % 0x168); //from 0x41948d //from 0x41948d
                    std::printf("%d : 0x%x - Type: %d - %d - %s - Lvl: %d | hp: %d | %.0f %.0f %.0f %d\n", i, (ptrShmBytePtr + entoffset), type, objId, reinterpret_cast<entName_t*>((ptrEntNameShmBytePtr + nameoffset))->name, level, health, posx, posy, posz, heading);
                }
            }
            else {
                //if no valid entity, memset that part of shared memory to 0
                memset(ptrShmBytePtr + entoffset, 0, sizeof(entity_t));
                memset(ptrEntNameShmBytePtr + nameoffset, 0, sizeof(entName_t));
            }
            //increase offsets for pointer
            entoffset += sizeof(entity_t);
            nameoffset += sizeof(entName_t);
        }
    }//Todo add exception
}

EntityInfo::~EntityInfo() {
    UnmapViewOfFile(ptrShmEntities);
    CloseHandle(hMapFile);
    UnmapViewOfFile(ptrShmEntNames);
    CloseHandle(entNameFile);
}

bool EntityInfo::GetEntityInfo() {

    if (ptrShmEntities != NULL && ptrShmEntNames != NULL) {
        //Create a new pointer and cast as unsigned char to be able to offset by single byte values
        //Stores entity pointer from game function
        uintptr_t ptrEntInfoBytePtr;
        //Recast in order to offset the shared memory pointer
        unsigned char* ptrShmBytePtr = reinterpret_cast<unsigned char*>(ptrShmEntities);
        unsigned char* ptrEntNameShmBytePtr = reinterpret_cast<unsigned char*>(ptrShmEntNames);
        //Offsets for the shared memory
        int entoffset = 0;
        int nameoffset = 0;
        //current max of the entity list
        entityListMax = *(int*)ptrEntityListMax;
        //update the zone offset values
        zoneYoffset = *(float*)ptrZoneYoffset_x;
        zoneXoffset = *(float*)ptrZoneXoffset_x;
        //Copy the entity_t structures into the shared memory
        for (int i = 0; i < entityListMax; i++) {
            //reset the entity pointer
            ptrEntInfoBytePtr = 0;
            //use game function to check if there is a valid entity at this offset
            if (daoc::EntityPtrSanityCheck(i)) {
                //Get the entity pointer for this offset from game function
                ptrEntInfoBytePtr = daoc::GetEntityPointer(i);
                if (ptrEntInfoBytePtr) {
                    //copy the entity to shared memory
                    memcpy(ptrShmBytePtr + entoffset, (entity_t*)ptrEntInfoBytePtr, sizeof(entity_t));
                    //Use game function to write the entity name to shared memory
                    daoc::GetEntityName(3, i, reinterpret_cast<entName_t*>((ptrEntNameShmBytePtr + nameoffset))->name, 48);                }
            }
            else {
                //if no valid entity, memset that part of shared memory to 0
                memset(ptrShmBytePtr + entoffset, 0, sizeof(entity_t));
                memset(ptrEntNameShmBytePtr + nameoffset, 0, sizeof(entName_t));
            }
            //increase offsets for pointer
            entoffset += sizeof(entity_t);
            nameoffset += sizeof(entName_t);
        }
    }//Todo add exception
    else {
        return false;
    }//Todo add exception
    
}
