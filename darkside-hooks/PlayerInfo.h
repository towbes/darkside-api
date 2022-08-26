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


public:
    PlayerInfo();
    ~PlayerInfo();

    //Player Hp/pow/endu
    void GetPlayerInfo();


};

