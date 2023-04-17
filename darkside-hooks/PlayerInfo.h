#pragma once
#include "daocgame.h"
#include "DaocStructs.h"

class PlayerInfo
{
private:
    //Current process id
    int pid;

    int* afkcheck;

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
    void* ptrPlayerName;
    uintptr_t ptrPlayerEntIndex;
    void* ptrPetEntIndex;

    //Use Skill and usespell Shared memory
    //Function will check this every frame
    //Cast if values present, then reset to 0

    //Use skill MMF
    int* pShmUseSkill;
    void* skillMapFile;
    std::wstring skillmmf_name;
    //Skill Queue
    std::queue<int> skillQueue;
    int currSkill;

    //Use spell MMF
    spellQueue_t* pShmUseSpell;
    void* spellMapFile;
    std::wstring spellmmf_name;
    //Spell Queue
    std::queue<spellQueue_t> spellQueue;
    int currSpell;

    //Pet cmd MMF
    petCmd_t* pShmPetCmd;
    void* petCmdMapFile;
    std::wstring petCmdmmf_name;
    //Pet Cmd Qeueue
    std::queue<petCmd_t> petCmdQueue;
    petCmd_t currPetCmd;

    //Move Item
    moveItem_t* pShmMoveItem;
    void* moveItemMapFile;
    std::wstring moveItemmmf_name;

public:
    PlayerInfo();
    ~PlayerInfo();

    //Player Hp/pow/endu
    void GetPlayerInfo();

    void QueueSkill();
    void QueueSpell();
    void QueuePetCmd();
    void QueueMoveItem();


};

