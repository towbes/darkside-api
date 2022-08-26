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
    int pShmSetTarget;
    std::wstring setTargmmf_name;

    uintptr_t ptrCurrTargetOffset;

public:
    TargetInfo();
    ~TargetInfo();

    //Player Hp/pow/endu
    void GetTargetInfo();

    void SetTarget();
};

