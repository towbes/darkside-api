#pragma once
#include "daocgame.h"
#include "DaocStructs.h"
class PlayerInfo
{
private:
    //Current process id
    int pid;

    //Player Health/Pow/Endu Info
    void* ptrPlayerHp;
    plyrinfo_t* pShmPlayerInfo;
    void* hMapFile;
    std::wstring plyrInfommf_name;

    //Setup mmf to track heading overwrite
    void* headingMapFile;
    std::wstring headingmmf_name;
    //Pointer to heading update struct in shared memory
    headingupdate_t* headingUpdate;
    //Mutex for the player position pointer
    //Prevents writing/reading at the same time
    std::mutex posUpdateMutex;

    //Player autorun toggle
    void* arunMapFile;
    std::wstring arunmmf_name;
    BYTE* shmAutorunToggle;
    BYTE* ptrAutorunToggle;
    //Stores previous value of mmf autorun toggle
    //This keeps track of whether an autorun was sent or not
    BYTE preValAutorunToggle;



public:
    PlayerInfo();
    ~PlayerInfo();

    //Player Hp/pow/endu
    void GetPlayerInfo();


};

