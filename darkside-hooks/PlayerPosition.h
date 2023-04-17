#pragma once
#include "daocgame.h"
#include "DaocStructs.h"

class PlayerPosition {
private:
    //Current process id
    int pid;

    //Player Position Info
    playerpos_t* playerPositionInfo;
    uintptr_t ptrPlayerPosition;
    playerpos_t* pShmPlayerPos;
    void* hMapFile;
    std::wstring posInfommf_name;

    //Setup mmf to track heading overwrite
    void* posUpdateMapFile;
    std::wstring posUpdateMmf_name;
    //Pointer to heading update struct in shared memory
    positionUpdate_t* posUpdate;
    //Mutex for the player position pointer
    //Prevents writing/reading at the same time
    std::mutex posUpdateMutex;
    float zoneYoffset;
    float zoneXoffset;

    //Player autorun toggle
    void* arunMapFile;
    std::wstring arunmmf_name;
    BYTE* shmAutorunToggle;
    BYTE* ptrAutorunToggle;
    //Stores previous value of mmf autorun toggle
    //This keeps track of whether an autorun was sent or not
    BYTE preValAutorunToggle;
    


public:
    PlayerPosition();
    ~PlayerPosition();

    //Player Info
    bool GetPlayerPosition();
    //Heading
    void SetHeading();

    //Autorun
    void GetAutorun();
    bool SetAutorun();

};