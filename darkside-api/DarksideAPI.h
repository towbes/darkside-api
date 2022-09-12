#pragma once
#include "pch.h"
#include "DaocStructs.h"

//Code reference
//https://stackoverflow.com/questions/315051/using-a-class-defined-in-a-c-dll-in-c-sharp-code

struct entityInfoAPI_t {
    char name[48];
    uint8_t type;  //       0x28e
    short objectId; //      0x23c
    int level; //           0x60
    int health; //          0x228
    float pos_x; //         0x48            
    float pos_y; //         0x370
    float pos_z; //         0xe7c
    short heading; //       0x180
    int castingCountdown; //in milliseconds     0x260
    int entState; //        0x4c 8 = idle, lots of other states
    short isDead; //        0xcf4
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

    std::mutex chatMutex;

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
