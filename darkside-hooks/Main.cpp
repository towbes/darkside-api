#include "pch.h"
#include "mem.h"
#include "gh_d3d9.h"
#include "Globals.h"
#include "PlayerPosition.h"
#include "PartyMemberInfo.h"
#include "PlayerInfo.h"
#include "EntityInfo.h"

bool bInit = false;

tPresent oPresent = nullptr;
tReset oReset = nullptr;
LPDIRECT3DDEVICE9 pD3DDevice = nullptr;
static WNDPROC origWndProc = nullptr;
static WNDPROC oWndProc = nullptr;
void* d3d9Device[119];

void* ptrPresent = NULL;
void* ptrReset = NULL;
char oPresBytes[11];
char oResetBytes[11];

bool done = false;

HHOOK mouseHook = NULL;

DWORD WINAPI Init(HMODULE hModule);

PlayerPosition* posInfo = NULL;
PartyMemberInfo* pMemInfo = NULL;
PlayerInfo* plyrInfo = NULL;
EntityInfo* entInfo = NULL;

extern "C" __declspec(dllexport) void __cdecl MainThread() {
#ifdef _DEBUG
    //Create Console
    AllocConsole();
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    std::cout << "DLL got injected!!" << std::endl;
#endif 
    //Start d3d9 hook
    Init(ghModule);

    //PlayerPosition* posInfo = new PlayerPosition();
    //PartyMemberInfo* pMemInfo = new PartyMemberInfo();
    //PlayerInfo* plyrInfo = new PlayerInfo();

    //wait for user input
    while (true) {
        //posInfo->GetPlayerPosition();
        //posInfo->SetHeading();
        //posInfo->SetAutorun();
        //pMemInfo->GetPartyMembers();
        //plyrInfo->GetPlayerInfo();
        //break when user presses end
        if (GetAsyncKeyState(VK_END) & 1) {
            break;
        }
    }
#ifdef _DEBUG
    if (f != 0) {
        fclose(f);
    }
    FreeConsole();
#endif

    WriteMem((char*)ptrPresent, oPresBytes, 5);
    WriteMem((char*)ptrReset, oResetBytes, 5);

    FreeLibraryAndExitThread(ghModule, 0);
}



void cleanupImgui() {
    ///* Delete imgui to avoid errors */
    //ImGui_ImplDX9_Shutdown();
    //ImGui_ImplWin32_Shutdown();
    //ImGui::DestroyContext();
}

void InitImGui(IDirect3DDevice9* pDevice) {
    //D3DDEVICE_CREATION_PARAMETERS CP;
    //pDevice->GetCreationParameters(&CP);
    //window = CP.hFocusWindow;
    //ImGui::CreateContext();
    //ImGuiIO& io = ImGui::GetIO(); (void)io;
    //io.IniFilename = NULL;
    //io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
    //io.Fonts->AddFontDefault();
    //
    //ImGui_ImplWin32_Init(window);
    //ImGui_ImplDX9_Init(pDevice);
    //bInit = true;
    return;
}
//extern IMGUI_IMPL_API LRESULT ImGui_ImplWin32_WndProcHandler(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);
LRESULT WINAPI WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{

    //if (ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam)) {
    //    return 1;
    //}


    //ImGuiIO& io = ImGui::GetIO(); (void)io;
    //// Check if ImGui wants to handle the keyboard..
    //if (msg >= WM_KEYFIRST && msg <= WM_KEYLAST)
    //{
    //    if (io.WantTextInput || io.WantCaptureKeyboard || ImGui::IsAnyItemActive())
    //        return 1;
    //}
    //
    //// Check if ImGui wants to handle the mouse..
    //if (msg >= WM_MOUSEFIRST && msg <= WM_MOUSELAST)
    //{
    //    if (io.WantCaptureMouse)
    //        return 1;
    //}

    return ::CallWindowProcA(oWndProc, hWnd, msg, wParam, lParam);
}

HRESULT APIENTRY hkPresent(LPDIRECT3DDEVICE9 pDevice, const RECT* pSourceRect, const RECT* pDestRect, HWND hDestWindowOverride, const RGNDATA* pDirtyRegion)
{

    if (!bInit) {
        posInfo = new PlayerPosition();
        pMemInfo = new PartyMemberInfo();
        plyrInfo = new PlayerInfo();
        entInfo = new EntityInfo();
        bInit = true;
    }

    if (posInfo != NULL) {
        posInfo->GetPlayerPosition();
        posInfo->SetHeading();
        posInfo->SetAutorun();
    }
    if (pMemInfo != NULL) {
        pMemInfo->GetPartyMembers();
    }
    if (plyrInfo != NULL) {
        plyrInfo->GetPlayerInfo();
    }

    if (entInfo != NULL) {
        entInfo->GetEntityInfo();
    }
        
    //draw stuff here like so:
    //if (!bInit) InitImGui(pDevice);
    //else {
    //
    //    DrawGui();
    //}

    return oPresent(pDevice, pSourceRect, pDestRect, hDestWindowOverride, pDirtyRegion);
}

HRESULT APIENTRY hkReset(LPDIRECT3DDEVICE9 pDevice, D3DPRESENT_PARAMETERS* pPresentationParameters)
{

    //if (bInit) {
    //    /* Delete imgui to avoid errors */
    //    cleanupImgui();
    //}


    return oReset(pDevice, pPresentationParameters);
}



DWORD WINAPI Init(HMODULE hModule)
{


    uintptr_t moduleBase = (uintptr_t)GetModuleHandle(L"game.dll");
    char* name = (char*)(moduleBase + 0xc4a6a8);
    if (name[0] == 0x59) {
        FreeLibraryAndExitThread(hModule, 0);
        return 0;
    }

    window = GetProcessWindow();

    if (GetD3D9Device(d3d9Device, sizeof(d3d9Device)))
    {

        //Check for mojo dll
        if (!isModuleLoaded(L"mojo_remote_cp.dll", window)) {
            ptrPresent = d3d9Device[17];
            ptrReset = d3d9Device[16];
        }
        else {
            char* tempPtr = (char*)d3d9Device[17];
            tempPtr += 0x2f;
            void* ptrPresentTemp = *(void**)tempPtr;
            ptrPresent = *(void**)ptrPresentTemp;

            char* tempPtr2 = (char*)d3d9Device[16];
            tempPtr2 += 0x1E;
            void* ptrResetTemp = *(void**)tempPtr2;
            ptrReset = *(void**)ptrResetTemp;
        }

        //std::cout << "present: 0x" << std::hex << d3d9Device[17] << " reset: 0x" << d3d9Device[16] << std::endl;
        if (ptrPresent != NULL && ptrReset != NULL) {
            //write original bytes to buffer for cleanup later
            memcpy(oPresBytes, (char*)ptrPresent, 5);
            memcpy(oResetBytes, (char*)ptrReset, 5);
            //do the hooks
            oPresent = (tPresent)TrampHook((char*)ptrPresent, (char*)hkPresent, 5);
            oReset = (tReset)TrampHook((char*)ptrReset, (char*)hkReset, 5);
        }

    }

    //code for imgui
    //origWndProc = (WNDPROC)GetWindowLongPtr(window, GWL_WNDPROC);
    //oWndProc = (WNDPROC)SetWindowLongPtr(window, GWL_WNDPROC, (LONG_PTR)WndProc);

    //Daoc Addresses
    //LoadHooks();

    //while (true) {
    //    if (GetAsyncKeyState(VK_RCONTROL) & 1) {
    //        break;
    //    }
    //}

    //Restore WndProc
    //(WNDPROC)SetWindowLongPtr(window, GWL_WNDPROC, (LONG_PTR)origWndProc);
    //
    //if (ptrPresent != NULL && ptrReset != NULL) {
    //    WriteMem((char*)ptrPresent, oPresBytes, 5);
    //    WriteMem((char*)ptrReset, oResetBytes, 5);
    //    cleanupImgui();
    //    bInit = false;
    //}
    //
    //FreeLibraryAndExitThread(hModule, 0);

    return 0;
}