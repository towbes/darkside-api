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

    
    //credit atom0s for this function
    //use __stdcall to make stack setup/cleanup simpler
    //void GetEntityName(int table_idx, int entity_idx, char* Destination, size_t Count);
    void __stdcall GetEntityName(int table_idx, int entity_idx, char* Destination, size_t Count);

    void UseSpell(int canCastSpell, int spellSlot);
    //Use spell and use skill functions
    //typedef void(__cdecl* _UseSpell)(int canCastSpell, int spellSlot);
    //_UseSpell UseSpell = (_UseSpell)funcUseSpell_x;

    void UseSkill(int skillSlot, int hasSkillFlag);
    //Use skill function
    //typedef void(__cdecl* _UseSkill)(int skillSlot, int hasSkillFlag);
    //_UseSkill UseSkill = (_UseSkill)funcUseSkill_x;

    //Set target func
    void SetTarget(int entIdx, bool hasTarget);
    //typedef void(__cdecl* _SetTarget)(int entIdx, bool hasTarget);
    //_SetTarget SetTarget;// = (_SetTarget)0x42b2e3;

    void SetTargetUI();
    //This function updates the UI with current target set via SetTarget
    //typedef void(__cdecl* _SetTargetUI)();
    //_SetTargetUI SetTargetUI; //= (_SetTargetUI)0x48f194;

}
