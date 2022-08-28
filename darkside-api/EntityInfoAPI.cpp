#include "pch.h"
#include "DarksideAPI.h"


bool DarksideAPI::GetEntityInfo(int entIndex, LPVOID lpBuffer) {


    //Setup zone offset shm for loc calcs
    //Zone offset mmf
    std::wstring zoneOffommf_name = std::to_wstring(pidHandle) + L"_ZoneOffsets";
    //Set the size for 8 party members
    std::size_t zoneOffsetfileSize = sizeof(zoneoffset_t);

    //Create a handle to memory mapped file
    auto zoneOffsetFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        zoneOffsetfileSize,                // maximum object size (low-order DWORD)
        zoneOffommf_name.c_str());                 // name of mapping object

    if (zoneOffsetFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        if (zoneOffsetFile != NULL) {
            CloseHandle(zoneOffsetFile);
        }
        memset(lpBuffer, 0, sizeof(entityInfoAPI_t));
        return false;
    }

    //Todo add exception
    //Copy the offsets into shm
    zoneoffset_t* ptrZoneOffsets = (zoneoffset_t*)MapViewOfFile(zoneOffsetFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    //Ent Info shm name and size
    std::wstring entInfommf_name = std::to_wstring(pidHandle) + L"_EntInfo";
    std::size_t fileSize = sizeof(entity_t) * 2000;
    entity_t* ptrEntList = NULL;
    entName_t* ptrEntName = NULL;

    //Create a handle to memory mapped file
    auto entListFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        entInfommf_name.c_str());                 // name of mapping object

    if (entListFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        if (ptrEntList != NULL) {
            UnmapViewOfFile(ptrEntList);
        }
        if (entListFile != NULL) {
            CloseHandle(entListFile);
        }
        if (ptrEntName != NULL) {
            UnmapViewOfFile(ptrEntName);
        }
        if (ptrZoneOffsets != NULL) {
            UnmapViewOfFile(ptrZoneOffsets);
        }
        if (zoneOffsetFile != NULL) {
            CloseHandle(zoneOffsetFile);
        }
        memset(lpBuffer, 0, sizeof(entityInfoAPI_t));
        return false;
    }

    if (entListFile != 0) {
        //Map a view of the memory mapped file from above
        ptrEntList = (entity_t*)MapViewOfFile(entListFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    if (ptrEntList == NULL) {
        _tprintf(TEXT("Could not create entList map view object (%d).\n"),
            GetLastError());
        if (ptrEntList != NULL) {
            UnmapViewOfFile(ptrEntList);
        }
        if (entListFile != NULL) {
            CloseHandle(entListFile);
        }
        if (ptrEntName != NULL) {
            UnmapViewOfFile(ptrEntName);
        }
        if (ptrZoneOffsets != NULL) {
            UnmapViewOfFile(ptrZoneOffsets);
        }
        if (zoneOffsetFile != NULL) {
            CloseHandle(zoneOffsetFile);
        }
        memset(lpBuffer, 0, sizeof(entityInfoAPI_t));
        return false;
    }

    //Setup the EntName mmf
    std::wstring entNamemmf_name = std::to_wstring(pidHandle) + L"_EntName";
    //Set the size for 2000 entities
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
        if (ptrEntList != NULL) {
            UnmapViewOfFile(ptrEntList);
        }
        if (entListFile != NULL) {
            CloseHandle(entListFile);
        }
        if (ptrEntName != NULL) {
            UnmapViewOfFile(ptrEntName);
        }
        if (entNameFile != NULL) {
            CloseHandle(entNameFile);
        }
        if (ptrZoneOffsets != NULL) {
            UnmapViewOfFile(ptrZoneOffsets);
        }
        if (zoneOffsetFile != NULL) {
            CloseHandle(zoneOffsetFile);
        }
        memset(lpBuffer, 0, sizeof(entityInfoAPI_t));
        return false;
    }

    if (entNameFile != 0) {
        //Map a view of the memory mapped file from above
        ptrEntName = (entName_t*)MapViewOfFile(entNameFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception


    if (ptrEntName == NULL) {
        _tprintf(TEXT("Could not create entName map view object (%d).\n"),
            GetLastError());
        if (ptrEntList != NULL) {
            UnmapViewOfFile(ptrEntList);
        }
        if (entListFile != NULL) {
            CloseHandle(entListFile);
        }
        if (ptrEntName != NULL) {
            UnmapViewOfFile(ptrEntName);
        }
        if (entNameFile != NULL) {
            CloseHandle(entNameFile);
        }
        if (ptrZoneOffsets != NULL) {
            UnmapViewOfFile(ptrZoneOffsets);
        }
        if (zoneOffsetFile != NULL) {
            CloseHandle(zoneOffsetFile);
        }
        memset(lpBuffer, 0, sizeof(entityInfoAPI_t));
        return false;
    }

    //Recast in order to offset the shared memory pointer
    unsigned char* ptrShmBytePtr = reinterpret_cast<unsigned char*>(ptrEntList);
    unsigned char* ptrEntNameShmBytePtr = reinterpret_cast<unsigned char*>(ptrEntName);
    //Offsets for the shared memory
    int entoffset = sizeof(entity_t) * entIndex;
    int nameoffset = sizeof(entName_t) * entIndex;
    ptrShmBytePtr += entoffset;
    ptrEntNameShmBytePtr += nameoffset;

    const auto type = *(uint8_t*)(ptrShmBytePtr + 0x28e);
    const auto objId = *(uint16_t*)(ptrShmBytePtr + 0x23c);
    //if objId is 0 this offset was empty
    if (objId == 0) {
        //zero out the buffer to prevent dirty memory going to c#
        memset(lpBuffer, 0, sizeof(entityInfoAPI_t));
        if (ptrEntList != NULL) {
            UnmapViewOfFile(ptrEntList);
        }
        if (entListFile != NULL) {
            CloseHandle(entListFile);
        }
        if (ptrEntName != NULL) {
            UnmapViewOfFile(ptrEntName);
        }
        if (entNameFile != NULL) {
            CloseHandle(entNameFile);
        }
        if (ptrZoneOffsets != NULL) {
            UnmapViewOfFile(ptrZoneOffsets);
        }
        if (zoneOffsetFile != NULL) {
            CloseHandle(zoneOffsetFile);
        }
        return false;
    }
    const auto level = ((*(uint32_t*)(ptrShmBytePtr + 0x60) ^ 0xCB96) / 74) - 23;//unencode level: ((*(uint32_t*)(tempPtr + 0x60) ^ 0xCB96)/74) - 23
    const auto health = (*(uint32_t*)(ptrShmBytePtr + 0x228) ^ 0xbe00) / 0x22 - 0x23;//unencode health: (*(uint32_t*)(tempPtr + 0x228) ^ 0xbe00) / 0x22 - 0x23
    const auto posx = *(float*)(ptrShmBytePtr + 0x48) - ptrZoneOffsets->zoneXoffset;
    const auto posy = *(float*)(ptrShmBytePtr + 0x370) - ptrZoneOffsets->zoneYoffset;
    const auto posz = *(float*)(ptrShmBytePtr + 0xE7C);
    const auto heading = ((((*(uint16_t*)(ptrShmBytePtr + 0xcb6) + 0x800) * 0x168) / 0x1000) % 0x168); //from 0x41948d //from 0x41948d
    //Create struct to hold the entity data for API
    entityInfoAPI_t* tempEnt = new entityInfoAPI_t();
    memcpy(tempEnt->name, ptrEntNameShmBytePtr, sizeof(entName_t));
    tempEnt->health = health;
    tempEnt->type = type;
    tempEnt->objectId = objId;
    tempEnt->level = level;
    tempEnt->pos_x = posx;
    tempEnt->pos_y = posy;
    tempEnt->pos_z = posz;
    tempEnt->heading = heading;
    //Copy the entityInfoAPI_t to buffer
    memcpy(lpBuffer, tempEnt, sizeof(entityInfoAPI_t));

    delete tempEnt;
    if (ptrEntList != NULL) {
        UnmapViewOfFile(ptrEntList);
    }
    if (entListFile != NULL) {
        CloseHandle(entListFile);
    }
    if (ptrEntName != NULL) {
        UnmapViewOfFile(ptrEntName);
    }
    if (entNameFile != NULL) {
        CloseHandle(entNameFile);
    }
    if (ptrZoneOffsets != NULL) {
        UnmapViewOfFile(ptrZoneOffsets);
    }
    if (zoneOffsetFile != NULL) {
        CloseHandle(zoneOffsetFile);
    }
    //CleanupEntity();

    return true;
}
