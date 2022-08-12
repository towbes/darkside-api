#include "pch.h"
#include "Globals.h"
#include "PlayerPosition.h"
#include "PartyMemberInfo.h"


extern "C" __declspec(dllexport) void __cdecl MainThread() {
#ifdef _DEBUG
    //Create Console
    AllocConsole();
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    std::cout << "DLL got injected!!" << std::endl;
#endif 

    PlayerPosition* pInfo = new PlayerPosition();
    PartyMemberInfo* pMemInfo = new PartyMemberInfo();

    //wait for user input
    while (true) {
        pInfo->GetPlayerPosition();
        pInfo->SetHeading();
        pInfo->SetAutorun();
        pMemInfo->GetPartyMembers();
        //break when user presses end
        if (GetAsyncKeyState(VK_END) & 1) {
            break;
        }
        Sleep(5);
    }
#ifdef _DEBUG
    if (f != 0) {
        fclose(f);
    }
    FreeConsole();
#endif

    FreeLibraryAndExitThread(ghModule, 0);
}