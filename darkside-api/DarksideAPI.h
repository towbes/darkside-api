#pragma once
#include "stdafx.h"

//https://stackoverflow.com/questions/315051/using-a-class-defined-in-a-c-dll-in-c-sharp-code

class __declspec(dllexport) DarksideAPI {
private:
    int pidHandle;
    void setPid(int pid);
public:
    DarksideAPI();
    ~DarksideAPI();
    void InjectPid(int pid);
};
