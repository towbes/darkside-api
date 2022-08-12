#pragma once

#include "daocgame.h"
#include "DaocStructs.h"

class PartyMemberInfo
{
private:
    //Current process id
    int pid;

    //Player Position Info
    partymemberinfo_t* partyMemberInfo;
    uintptr_t ptrPartyMemberInfo;
    partymembers_t* ptrShmPartyMembers;
    void* hMapFile;
    std::wstring partyInfommf_name;


public:
    PartyMemberInfo();
    ~PartyMemberInfo();

    //Player Info
    bool GetPartyMembers();
};

