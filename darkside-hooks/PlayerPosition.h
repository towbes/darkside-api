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
    playerpos_t* pPlayerPos;
    void* hMapFile;
    std::wstring posInfommf_name;

    //Player autorun toggle
    void* arunMapFile;
    std::wstring arunmmf_name;
    BYTE preValAutorunToggle;
    BYTE* shmAutorunToggle;
    BYTE* ptrAutorunToggle;
    

public:
    PlayerPosition();
    ~PlayerPosition();

    //Player Info
    bool GetPlayerPosition();
    //Autorun
    void GetAutorun();
    void SetAutorun();
};