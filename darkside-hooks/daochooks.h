#pragma once
#include "daocgame.h"

namespace daoc {
    typedef uintptr_t(__fastcall* _GetEntityPointer)(int entityOffset);
    _GetEntityPointer GetEntityPointer = (_GetEntityPointer)funcGetEntityPointer_x;

    //Sanity checker before calling GetEntityPointer
    typedef bool(__fastcall* _EntityPtrSanityCheck)(int entityListOffset);
    _EntityPtrSanityCheck EntityPtrSanityCheck = (_EntityPtrSanityCheck)funcEntityPtrSanityCheck_x;

    uintptr_t wGetEntityName = funcGetEntityName_x;

    //credit atom0s for this function
    //use __stdcall to make stack setup/cleanup simpler
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
}
