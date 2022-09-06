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

    //Use skill MMF
    int* pShmUseSkill;
    void* skillMapFile;
    std::wstring skillmmf_name;
    //Use spell MMF
    int* pShmUseSpell;
    void* spellMapFile;
    std::wstring spellmmf_name;
    //Pet cmd MMF
    petCmd_t* pShmPetCmd;
    void* petCmdMapFile;
    std::wstring petCmdmmf_name;
    //Skill Queue
    std::queue<int> skillQueue;
    int currSkill;
    //Spell Queue
    std::queue<int> spellQueue;
    int currSpell;
    //Pet Cmd Qeueue
    std::queue<petCmd_t> petCmdQueue;
    petCmd_t currPetCmd;

public:
    PlayerInfo();
    ~PlayerInfo();

    //Player Hp/pow/endu
    void GetPlayerInfo();

    void QueueSkill();
    void QueueSpell();
    void QueuePetCmd();


};

