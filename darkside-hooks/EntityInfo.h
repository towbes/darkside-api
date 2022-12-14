#pragma once
#include "daocgame.h"
#include "DaocStructs.h"


class EntityInfo
{
private:
    //Current process id
    int pid;

    //current max of entity list
    int ptrEntityListMax;
    int entityListMax;
    float zoneYoffset;
    float zoneXoffset;

    //ptrEntity Info
    entity_t* entityInfo;
    //Pointer to in game memory
    uintptr_t ptrEntityInfo;
    //Shared memory for entity list
    entity_t* ptrShmEntities;
    void* hMapFile;
    std::wstring entInfommf_name;
    //shared memory for entity names
    entName_t* ptrShmEntNames;
    void* entNameFile;
    std::wstring entNamemmf_name;

    //Shared memory for zone offsets
    zoneoffset_t* ptrZoneOffsets;
    void* zoneOffsetFile;
    std::wstring zoneOffommf_name;

public:
    EntityInfo();
    ~EntityInfo();

    //Player Hp/pow/endu
    bool GetEntityInfo();


};

