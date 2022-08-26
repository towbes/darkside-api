#include "pch.h"
#include "Globals.h"
#include "PlayerPosition.h"
#include "PartyMemberInfo.h"
#include "PlayerInfo.h"


extern "C" __declspec(dllexport) void __cdecl MainThread() {
#ifdef _DEBUG
    //Create Console
    AllocConsole();
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    std::cout << "DLL got injected!!" << std::endl;
#endif 

    PlayerPosition* posInfo = new PlayerPosition();
    PartyMemberInfo* pMemInfo = new PartyMemberInfo();
    PlayerInfo* plyrInfo = new PlayerInfo();

    //wait for user input
    while (true) {
        posInfo->GetPlayerPosition();
        posInfo->SetHeading();
        posInfo->SetAutorun();
        pMemInfo->GetPartyMembers();
        plyrInfo->GetPlayerInfo();
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