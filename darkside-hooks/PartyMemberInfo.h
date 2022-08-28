#pragma once

#include "daocgame.h"
#include "DaocStructs.h"

class PartyMemberInfo
{
private:
    //Current process id
    int pid;

    //Party Member Info
    partymemberinfo_t* partyMemberInfo;
    //Pointer to in game memory
    uintptr_t ptrPartyMemberInfo;
    //Shared memory for party member array
    partymembers_t* ptrShmPartyMembers;
    //Shared memory variables
    void* hMapFile;
    std::wstring partyInfommf_name;


public:
    PartyMemberInfo();
    ~PartyMemberInfo();

    //Copy game memory to shared memory
    bool GetPartyMembers();
};

