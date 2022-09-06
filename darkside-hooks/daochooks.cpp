#include "pch.h"
#include "daochooks.h"

uintptr_t wGetEntityName = funcGetEntityName_x;

namespace daoc {
    uintptr_t GetEntityPointer(int entityOffset)
    {
        //typedef uintptr_t(__fastcall* _GetEntityPointer)(int entityOffset);
        //_GetEntityPointer GetEntityPointer = (_GetEntityPointer)funcGetEntityPointer_x;
        //const auto pointer = pointers::instance().get("entity_is_valid");
        //if (pointer == 0)
        //    return false;

        return reinterpret_cast<uintptr_t(__fastcall*)(uint32_t)>(funcGetEntityPointer_x)(entityOffset);
    }

    bool EntityPtrSanityCheck(int entOffset) {
        //typedef bool(__fastcall* _EntityPtrSanityCheck)(int entityListOffset);
        //_EntityPtrSanityCheck EntityPtrSanityCheck = (_EntityPtrSanityCheck)funcEntityPtrSanityCheck_x;
        
        return reinterpret_cast<bool(__fastcall*)(uint32_t)>(funcEntityPtrSanityCheck_x)(entOffset);
    }

    void __declspec(naked) __stdcall GetEntityName(int table_idx, int entity_idx, char* Destination, size_t Count) {

        __asm {
            //prepare stackframe
            push ebp
            mov ebp, esp

            //save the registers/flags
            pushad
            pushfd

            //Setup the call to game function GetEntityname
            push Count
            push[Destination]
            push table_idx
            mov ecx, entity_idx
            pop eax

            //call the game function
            call wGetEntityName

            //pop the two values off stack
            pop eax
            pop eax

            //restore registers/flags
            popfd
            popad

            //restore stack frame and return
            mov esp, ebp
            pop ebp
            ret 0x10

        }
    }

    //Use spell and skill
    void UseSpell(int canCastSpell, int spellSlot) {
        //Use spell and use skill functions
        typedef void(__cdecl* _UseSpell)(int spellSlotint, int canCastSpell);
        _UseSpell UseSpell = (_UseSpell)funcUseSpell_x;

        return UseSpell(canCastSpell, spellSlot);
    }


    void UseSkill(int skillSlot, int hasSkillFlag) {
        //Use skill function
        typedef void(__cdecl* _UseSkill)(int skillSlot, int hasSkillFlag);
        _UseSkill UseSkill = (_UseSkill)funcUseSkill_x;

        return UseSkill(skillSlot, hasSkillFlag);
    }

    //Set target func
    void SetTarget(int entIdx, bool hasTarget) {
        typedef void(__cdecl* _SetTarget)(int entIdx, bool hasTarget);
        _SetTarget SetTarget = (_SetTarget)funcSetTarget_x;

        return SetTarget(entIdx, hasTarget);
    }

    void SetTargetUI() {
        //This function updates the UI with current target set via SetTarget
        typedef void(__cdecl* _SetTargetUI)();
        _SetTargetUI SetTargetUI = (_SetTargetUI)funcSetTargetUI_x;
        
        return SetTargetUI();
    }

    //Incoming chat hook
    //void grabChat(const char* buffer) {
    //    std::string strBuff = std::string(buffer);
    //}
    //
    //__declspec(naked) void __stdcall printChat() {
    //    const char* ptrBuff;
    //    //save the registers/flags;
    //    _asm pushad;
    //    _asm pushfd;
    //    //prologue;
    //    _asm push ebp;
    //    _asm mov ebp, esp;
    //    _asm sub esp, __LOCAL_SIZE;
    //
    //    _asm mov ptrBuff, ebx;
    //
    //    ptrBuff += 1;
    //    grabChat(ptrBuff);
    //
    //    //epilogue
    //    _asm mov esp, ebp;
    //    _asm pop ebp;
    //    //restore registers/flags
    //    _asm popfd;
    //    _asm popad;
    //
    //    //instruction we overwrote
    //    _asm jmp oPrintChat
    //}


}