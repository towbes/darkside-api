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

    //Queue to hold the chatLog, does not need to be accessed from C#
    std::queue<std::string> chatLog;
    //New thread for chat listener
    std::thread* chatThread = nullptr;

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

    //Player Info
    bool GetPlayerInfo(LPVOID lpBuffer);
    bool UseSkill(int skillOffset);
    bool UseSpell(int spellOffset);

    //Party Members
    bool GetPartyMember(int memberIndex, LPVOID lpBuffer);

    //Target Info
    bool GetTargetInfo(LPVOID lpBuffer);
    bool SetTarget(int entIndex);

    //Chat manager, listener will start in a new thread
    void ChatListener();
    bool GetChatline(LPVOID lpBuffer);

};
