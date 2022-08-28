#pragma once
#include "DaocStructs.h"
class TargetInfo
{
private:
    //Current process id
    int pid;


    //player info shared memory
    targetInfo_t* pShmTargetInfo;
    void* hMapFile;
    std::wstring targetInfommf_name;

    //Set target shared memory
    // will wait until value is greater than -1 and set target to that ent offset
    void* hSetTargFile;
    int* pShmSetTarget;
    std::wstring setTargmmf_name;

    //Target info - target object id is at +0x4 of entity offset
    uintptr_t ptrCurrTargetOffset;
    //color is at +0x4 of hp
    uintptr_t ptrTargHp;
    //Has target is at +0x200 of targetName - '\0' is no target
    uintptr_t ptrTargName;

public:
    TargetInfo();
    ~TargetInfo();

    //Player Hp/pow/endu
    bool GetTargetInfo();

    void SetTarget();
};

