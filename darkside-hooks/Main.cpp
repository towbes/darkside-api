#include "pch.h"
#include "mem.h"
#include "gh_d3d9.h"
#include "Globals.h"
#include "PlayerPosition.h"
#include "PartyMemberInfo.h"
#include "PlayerInfo.h"
#include "EntityInfo.h"
#include "TargetInfo.h"
#include "ChatManager.h"
#include "daochooks.h"

bool bInit = false;

tPresent oPresent = nullptr;
tReset oReset = nullptr;
LPDIRECT3DDEVICE9 pD3DDevice = nullptr;
static WNDPROC origWndProc = nullptr;
static WNDPROC oWndProc = nullptr;
void* d3d9Device[119];
HRESULT APIENTRY hkPresent(LPDIRECT3DDEVICE9 pDevice, const RECT* pSourceRect, const RECT* pDestRect, HWND hDestWindowOverride, const RGNDATA* pDirtyRegion);
HRESULT APIENTRY hkReset(LPDIRECT3DDEVICE9 pDevice, D3DPRESENT_PARAMETERS* pPresentationParameters);

void* ptrPresent = NULL;
void* ptrReset = NULL;
char oPresBytes[11];
char oResetBytes[11];

//chat hook
uintptr_t ptrPrintChat = funcPrintChat_x;
void GrabChat(const char* buffer);
char chatBuf[2048];
std::mutex chatBufMutex;
bool newMsg = false;

//Chat Hooks
//Incoming chat hook
void GrabChat(const char* buffer) {
    //::OutputDebugStringA(std::format("Text: {}", buffer).c_str());
    std::scoped_lock<std::mutex> lg(chatBufMutex);
    newMsg = true;
    strcpy_s(chatBuf, buffer);
}

__declspec(naked) void __stdcall PrintChat() {
    const char* ptrBuff;
    //save the registers/flags;
    _asm pushad;
    _asm pushfd;
    //prologue;
    _asm push ebp;
    _asm mov ebp, esp;
    _asm sub esp, __LOCAL_SIZE;

    _asm mov ptrBuff, ebx;

    ptrBuff += 1;
    GrabChat(ptrBuff);

    //epilogue
    _asm mov esp, ebp;
    _asm pop ebp;
    //restore registers/flags
    _asm popfd;
    _asm popad;

    //instruction we overwrote
    _asm jmp ptrPrintChat
}


DWORD WINAPI Init(HMODULE hModule);

PlayerPosition* posInfo = NULL;
PartyMemberInfo* pMemInfo = NULL;
PlayerInfo* plyrInfo = NULL;
EntityInfo* entInfo = NULL;
TargetInfo* targInfo = NULL;
ChatManager* chatMan = NULL;

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


    //Get Process ID
    int currpid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring
    std::string str = std::to_string(currpid) + "_unloadFlag";
    std::size_t fsize = sizeof(int);

    // Get a handle to our file map
    auto hMapFile = CreateFileMappingA(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, fsize, str.c_str());
    if (hMapFile == nullptr) {
        MessageBoxA(nullptr, "Failed to create file mapping!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
    }
    // Get our shared memory pointer
    int* lpMemFile = (int*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, 0);
    if (lpMemFile == nullptr) {
        MessageBoxA(nullptr, "Failed to map shared memory!", "DLL_PROCESS_ATTACH", MB_OK | MB_ICONERROR);
        CloseHandle(hMapFile);
    }

    *lpMemFile = 0;

    //wait for unload command from shared memory
    while (true) {
        //break when user presses end
        if (*lpMemFile == 1 || GetAsyncKeyState(VK_RCONTROL) & 1) {
            break;
        }
        Sleep(100);
    }
#ifdef _DEBUG
    if (f != 0) {
        fclose(f);
    }
    FreeConsole();
#endif

    //Remove all hooks

    if (ptrPresent != NULL && ptrReset != NULL) {
        DetourTransactionBegin();
        DetourUpdateThread(GetCurrentThread());
        DetourDetach(&(PVOID&)ptrPresent, hkPresent);
        long result = DetourTransactionCommit();
        if (result != NO_ERROR)
        {

        }

        DetourTransactionBegin();
        DetourUpdateThread(GetCurrentThread());
        DetourDetach(&(PVOID&)ptrReset, hkReset);
        result = DetourTransactionCommit();
        if (result != NO_ERROR)
        {

        }

        DetourTransactionBegin();
        DetourUpdateThread(GetCurrentThread());
        DetourDetach(&(PVOID&)ptrPrintChat, PrintChat);
        result = DetourTransactionCommit();
        if (result != NO_ERROR)
        {

        }


        bInit = false;
    }

    //Sleep to give reset a time to run?
    Sleep(200);

    FreeLibraryAndExitThread(ghModule, 0);
}

HRESULT APIENTRY hkPresent(LPDIRECT3DDEVICE9 pDevice, const RECT* pSourceRect, const RECT* pDestRect, HWND hDestWindowOverride, const RGNDATA* pDirtyRegion)
{

    if (!bInit) {
        posInfo = new PlayerPosition();
        pMemInfo = new PartyMemberInfo();
        plyrInfo = new PlayerInfo();
        entInfo = new EntityInfo();
        targInfo = new TargetInfo();
        chatMan = new ChatManager();
        bInit = true;
    }

    //These functions will run every frame
    //They are used to get/set memory
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
        plyrInfo->QueueSkill();
        plyrInfo->QueueSpell();
        plyrInfo->QueuePetCmd();
        plyrInfo->QueueMoveItem();
    }
    if (entInfo != NULL) {
        entInfo->GetEntityInfo();
    }
    if (targInfo != NULL) {
        targInfo->GetTargetInfo();
        targInfo->SetTarget();
        targInfo->InteractRequest();
    }
    if (chatMan != NULL) {
        if (newMsg) {
            std::scoped_lock<std::mutex> lg(chatBufMutex);
            chatMan->CopyChat(chatBuf);
            newMsg = false;
        }
        chatMan->QueueCommand();
        chatMan->QueueSendPacket();

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

        if (ptrPresent != NULL && ptrReset != NULL) {

            //D3D9 Present hook
            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(&(PVOID&)ptrPresent, hkPresent);
            long result = DetourTransactionCommit();
            if (result != NO_ERROR)
            {

            }
            //D3D9 Reset hook
            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(&(PVOID&)ptrReset, hkReset);
            result = DetourTransactionCommit();
            if (result != NO_ERROR)
            {

            }

            oPresent = (tPresent)ptrPresent;
            oReset = (tReset)ptrReset;

            //Print to Chat detour (messages that don't require server side validation are not part of this function, so some messages will be missed)
            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(&(PVOID&)ptrPrintChat, PrintChat);
            result = DetourTransactionCommit();
            if (result != NO_ERROR)
            {

            }


        }
    }
    return 0;
}

