#pragma once
#include "daocgame.h"
#include "DaocStructs.h"

class PlayerInfo
{
private:
    //Current process id
    int pid;

    //player info shared memory
    plyrinfo_t* pShmPlayerInfo;
    void* hMapFile;
    std::wstring plyrInfommf_name;

    //Player Health/Pow/Endu Info
    void* ptrPlayerHp;
    void* ptrUseSkills;
    void* ptrUseSpells;
    void* ptrBuffs;
    void* ptrInventory;

    //Use Skill and usespell Shared memory
    //Function will check this every frame
    //Cast if values present, then reset to 0

    int pShmUseSkill;
    void* skillMapFile;
    std::wstring skillmmf_name;
    int pShmUseSpell;
    void* spellMapFile;
    std::wstring spellmmf_name;

public:
    PlayerInfo();
    ~PlayerInfo();

    //Player Hp/pow/endu
    void GetPlayerInfo();

    void QueueSkill();
    void QueueSpell();


};

