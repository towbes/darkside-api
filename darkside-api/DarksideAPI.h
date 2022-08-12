#pragma once
#include "pch.h"
#include "DaocStructs.h"

//Code reference
//https://stackoverflow.com/questions/315051/using-a-class-defined-in-a-c-dll-in-c-sharp-code

class __declspec(dllexport) DarksideAPI {
private:
    int pidHandle;
    void setPid(int pid);
public:
    DarksideAPI();
    ~DarksideAPI();

    //Injector
    void InjectPid(int pid);

    //Player Position
    bool GetPlayerPosition(LPVOID lpBuffer);
    bool SetAutorun(bool autorun);

    //Party Members
    bool GetPartyMember(int memberIndex, LPVOID lpBuffer);

};
