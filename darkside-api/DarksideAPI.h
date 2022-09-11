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

struct entityListAPI_t {
    entityInfoAPI_t entList[2000];
};

class __declspec(dllexport) DarksideAPI {
private:
    int pidHandle;
    bool injected;

    //Queue to hold the chatLog, does not need to be accessed from C#
    std::queue<std::string> chatLog;
    //New thread for chat listener
    std::thread* chatThread = nullptr;

public:
    DarksideAPI();
    ~DarksideAPI();

    //Injector
    bool InjectPid(int pid);
    bool Unload(int pid);
    int GetPid();
    
    //Entity
    bool GetEntityInfo(int entIndex, LPVOID lpBuffer);
    bool GetEntityList(LPVOID lpBuffer);

    //Player Position
    bool GetPlayerPosition(LPVOID lpBuffer);
    bool SetPlayerHeading(bool changeHeading, short newHeading);
    bool SetAutorun(bool autorun);

    //Player Info
    bool GetPlayerInfo(LPVOID lpBuffer);
    bool UseSkill(int skillOffset);
    bool UseSpell(int spellCategory, int spellLevel);
    bool UsePetCmd(int aggState, int walkState, int petCmd);
    bool MoveItem(int fromSlot, int toSlot, int count);

    //Party Members
    bool GetPartyMember(int memberIndex, LPVOID lpBuffer);

    //Target Info
    bool GetTargetInfo(LPVOID lpBuffer);
    bool SetTarget(int entIndex);
    bool InteractRequest(uint16_t objId);

    //Chat manager, listener will start in a new thread
    void ChatListener();
    bool GetChatline(LPVOID lpBuffer);
    bool SendCommand(int cmdMode, int iMode, LPVOID lpBuffer);
    bool SendPacket(LPVOID packetBuffer);

};
