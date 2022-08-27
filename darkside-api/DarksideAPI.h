#pragma once
#include "pch.h"
#include "DaocStructs.h"

//Code reference
//https://stackoverflow.com/questions/315051/using-a-class-defined-in-a-c-dll-in-c-sharp-code

struct entityInfoAPI_t {
    char name[48];
    uint8_t type;
    short objectId;
    int level;
    int health;
    float pos_x;
    float pos_y;
    float pos_z;
    short heading;
};

class __declspec(dllexport) DarksideAPI {
private:
    int pidHandle;
    void setPid(int pid);
public:
    DarksideAPI();
    ~DarksideAPI();

    //Injector
    void InjectPid(int pid);

    //Entity
    bool GetEntityInfo(int entIndex, LPVOID lpBuffer);

    //Player Position
    bool GetPlayerPosition(LPVOID lpBuffer);
    bool SetPlayerHeading(bool changeHeading, short newHeading);
    bool SetAutorun(bool autorun);

    //Party Members
    bool GetPartyMember(int memberIndex, LPVOID lpBuffer);

};
