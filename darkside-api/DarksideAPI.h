#pragma once
#include "pch.h"

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

    //Player Info
    bool GetPlayerInfo(LPVOID lpBuffer);
};
