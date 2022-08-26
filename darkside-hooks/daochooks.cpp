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
        
        return reinterpret_cast<uintptr_t(__fastcall*)(uint32_t)>(funcEntityPtrSanityCheck_x)(entOffset);
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

    void UseSpell(int canCastSpell, int spellSlot) {
        //Use spell and use skill functions
        typedef void(__cdecl* _UseSpell)(int canCastSpell, int spellSlot);
        _UseSpell UseSpell = (_UseSpell)funcUseSpell_x;

        return UseSpell(canCastSpell, spellSlot);
    }


    void UseSkill(int skillSlot, int hasSkillFlag) {
        //Use skill function
        typedef void(__cdecl* _UseSkill)(int skillSlot, int hasSkillFlag);
        _UseSkill UseSkill = (_UseSkill)funcUseSkill_x;

        return UseSkill(skillSlot, hasSkillFlag);
    }


}