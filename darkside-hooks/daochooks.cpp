#include "pch.h"
#include "daochooks.h"

uintptr_t wGetEntityName = funcGetEntityName_x;
uintptr_t ptrChatiMode = ptrChatiMode_x;
uintptr_t oCmdHandler = funcCmdHandler_x;

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

    // (c) 2022 atom0s [atom0s@live.com]
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

    void UsePetCommand(char aggroState, char walkState, char command) {
        typedef void(__cdecl* _PetWindow)(char aggroState, char walkState, char command);
        _PetWindow PetWindow = (_PetWindow)funcPetWindow_x;

        return PetWindow(aggroState, walkState, command);
        //Address of signature = game.dll + 0x0002AD78
        //const char* funcPetWindowPattern = "\x55\x8B\xEC\x51\x83\x3D\x00\x82\x99\x00\x00\x75\x00\x8A\x45\x00\x88\x45\x00\x8A\x45\x00\x88\x45\x00\x8A\x45";
        //const char* funcPetWindowMask = "xxxxxxxxxx?x?xx?xx?xx?xx?xx";
        //"55 8B EC 51 83 3D 00 82 99 00 ? 75 ? 8A 45 ? 88 45 ? 8A 45 ? 88 45 ? 8A 45"
    }

    void __declspec(naked) SendCommand(int cmdMode, int iMode, const char* cmdBuffer) {
        //typedef void(__cdecl* _SendCommand)(const char* cmdBuffer);
        //_SendCommand SendCommand = (_SendCommand)funcSendCmd_x;
        //return SendCommand(cmdBuffer);
        ////Address of signature = game.dll + 0x0002BC08 0x42bc08
        //const char* sendCmdPattern = "\x83\x3D\x00\x82\x99\x00\x00\x0F\x85\x00\x00\x00\x00\x56";
        //const char* sendCmdMask = "xxxxxx?xx????x";
        //"83 3D 00 82 99 00 ? 0F 85 ? ? ? ? 56"
            //void SendCommand(int cmdMode, int iMode, const char* cmdBuffer) {
                //prepare stackframe
        _asm push ebp;
        _asm mov ebp, esp;
        _asm sub esp, __LOCAL_SIZE;

        *(int*)ptrChatiMode = iMode;
        _asm push cmdMode;
        _asm mov edx, cmdBuffer

        _asm call oCmdHandler;
        //wrapCmdHandler();

        //epilogue
        _asm mov esp, ebp;
        _asm pop ebp;
        //
        _asm ret;



    }

}