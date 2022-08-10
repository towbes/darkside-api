// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"


DWORD WINAPI MainThread(HMODULE hModule) {
#ifdef _DEBUG
    //Create Console
    AllocConsole();
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    std::cout << "DLL got injected!" << std::endl;
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

    FreeLibraryAndExitThread(hModule, 0);
    return 0;


    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        HANDLE ThreadHandle = CreateThread(0, NULL, (LPTHREAD_START_ROUTINE)MainThread, hModule, NULL, NULL);

        if (ThreadHandle != NULL) {
            CloseHandle(ThreadHandle);
        }
        break;
    }
    return TRUE;
}

