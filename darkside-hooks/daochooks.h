#pragma once
#include "daocgame.h"

namespace daoc {


    uintptr_t GetEntityPointer(int entityOffset);
    //typedef uintptr_t(__fastcall* _GetEntityPointer)(int entityOffset);
    //_GetEntityPointer GetEntityPointer = (_GetEntityPointer)funcGetEntityPointer_x;

    //Sanity checker before calling GetEntityPointer
    bool EntityPtrSanityCheck(int entOffset);
    //typedef bool(__fastcall* _EntityPtrSanityCheck)(int entityListOffset);
    //_EntityPtrSanityCheck EntityPtrSanityCheck = (_EntityPtrSanityCheck)funcEntityPtrSanityCheck_x;

   
    //// (c) 2022 atom0s [atom0s@live.com]
    //use __stdcall to make stack setup/cleanup simpler
    //void GetEntityName(int table_idx, int entity_idx, char* Destination, size_t Count);
    //table index should always be 3, count should be 48
    void __stdcall GetEntityName(int table_idx, int entity_idx, char* Destination, size_t Count);

    //canCastSpell should always be 1
    void UseSpell(int spellSlot, int canCastSpell);
    //Use spell and use skill functions
    //typedef void(__cdecl* _UseSpell)(int canCastSpell, int spellSlot);
    //_UseSpell UseSpell = (_UseSpell)funcUseSpell_x;

    //Has Skill flag should always be 1
    void UseSkill(int skillSlot, int hasSkillFlag);
    //Use skill function
    //typedef void(__cdecl* _UseSkill)(int skillSlot, int hasSkillFlag);
    //_UseSkill UseSkill = (_UseSkill)funcUseSkill_x;

    //Set target func
    //hasTarget should always be 0
    void SetTarget(int entIdx, bool hasTarget);
    //typedef void(__cdecl* _SetTarget)(int entIdx, bool hasTarget);
    //_SetTarget SetTarget;// = (_SetTarget)0x42b2e3;
    // 
    //This function updates the UI with current target set via SetTarget
    void SetTargetUI();
    //typedef void(__cdecl* _SetTargetUI)();
    //_SetTargetUI SetTargetUI; //= (_SetTargetUI)0x48f194;

    //pet window packet function
    //aggroState // 1-Aggressive, 2-Deffensive, 3-Passive
    //walkState // 1-Follow, 2-Stay, 3-GoTarg, 4-Here
    //command // 1-Attack, 2-Release
    void UsePetCommand(char aggroState, char walkState, char command);
    //typedef void(__cdecl* _PetWindow)(char aggroState, char walkState, char command);
    //_PetWindow PetWindow;
    //Address of signature = game.dll + 0x0002AD78
    //const char* funcPetWindowPattern = "\x55\x8B\xEC\x51\x83\x3D\x00\x82\x99\x00\x00\x75\x00\x8A\x45\x00\x88\x45\x00\x8A\x45\x00\x88\x45\x00\x8A\x45";
    //const char* funcPetWindowMask = "xxxxxxxxxx?x?xx?xx?xx?xx?xx";
    //"55 8B EC 51 83 3D 00 82 99 00 ? 75 ? 8A 45 ? 88 45 ? 8A 45 ? 88 45 ? 8A 45"

    //Send command function
    //(c) 2022 atom0s [atom0s@live.com]
    //     enum command_mode : int32_t
    //{
    //    typed = 0,
    //        macro = 1,
    //        system = 2,
    //};
    //enum input_mode : int32_t
    //{
    //    normal = 0,
    //    slash = 1,
    //    debug = 2,
    //};
    // 
    // 
    //Commands prefixed with & (not /)
    void SendCommand(int cmdMode, int iMode, const char* cmdBuffer);
    //typedef void(__cdecl* _SendCommand)(const char* cmdBuffer);
    //_SendCommand SendCommand;// = (_GetEntityPointer)0x43589f;
    ////Address of signature = game.dll + 0x0002BC08 0x42bc08
    //const char* sendCmdPattern = "\x83\x3D\x00\x82\x99\x00\x00\x0F\x85\x00\x00\x00\x00\x56";
    //const char* sendCmdMask = "xxxxxx?xx????x";
    //"83 3D 00 82 99 00 ? 0F 85 ? ? ? ? 56"

    //Move item Function
    //Slot reference: https://github.com/Dawn-of-Light/DOLSharp/blob/9af87af011497c3fda852559b01a269c889b162e/GameServer/gameutils/IGameInventory.cs
    //Ground = 1
    //Backpack = 40-79
    //count = 0 for non stacks
    void MoveItem(int toSlot, int fromSlot, int count);
    //typedef void(__cdecl* _MoveItem)(int toSlot, int fromSlot, int count);
    //_MoveItem MoveItem; //  = (_MoveItem)0x42a976;

    int SendPacket(const char* packetBuffer, DWORD packetHeader, DWORD packetLen, DWORD unknown);
    //_SendPacket Send;// = (_SendPacket)0x4281df;
    //typedef int(__cdecl* _SendPacket)(const char* packetBuffer, DWORD packetHeader, DWORD packetLen, DWORD unknown);
    //_SendPacket SendPacket;

    void InteractRequest(uint16_t objId);
    //Object interact request
    //typedef void(__cdecl* _InteractRequest)(int objId);
    //_InteractRequest InteractRequest;
    //Address of signature = game.dll + 0x0002AE06 0x42ae06
}
