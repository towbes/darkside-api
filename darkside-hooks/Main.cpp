#include "pch.h"
#include "Globals.h"

extern "C" __declspec(dllexport) void __cdecl MainThread() {
#ifdef _DEBUG
    //Create Console
    AllocConsole();
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    std::cout << "DLL got injected!!" << std::endl;
#endif 

    //Initialize daocgame pointers
    ptrPlayerPosition = ptrPlayerPosition_x;
    playerPositionInfo = *(playerpos_t**)ptrPlayerPosition;

#ifdef _DEBUG
    std::cout << "PlayerPositionPtr: " << std::hex << (int)ptrPlayerPosition << std::endl;
    std::cout << "Player Position X: " << std::fixed << std::setprecision(0) << playerPositionInfo->pos_x << std::endl;
#endif


    //wait for user input
    while (true) {
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