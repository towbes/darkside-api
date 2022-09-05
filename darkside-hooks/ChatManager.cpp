#include "pch.h"
#include "ChatManager.h"
#include "daocgame.h"

ChatManager::ChatManager() {
    //Initialize daocgame pointers
    //ptrPrintChat = funcPrintChat_x;

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring

    //Setup the Chat Manager mmf
    chatManagermmf_name = std::to_wstring(pid) + L"_ChatMan";
    std::size_t fileSize = sizeof(chatManager_t);
    //
    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        chatManagermmf_name.c_str());                 // name of mapping object
    //
    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Plyr Pos Could not create file mapping object (%d).\n"),
            GetLastError());
    }
    
    if (hMapFile != 0) {
        pShmChatmanager = (chatManager_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception
    //
    if (pShmChatmanager != NULL) {
        //initialize a new chat maanager struct
        chatManager_t chatMan;
        chatMan.rdySend = true;
        //copy that struct to shared memory
        memcpy(pShmChatmanager,&chatMan, sizeof(chatManager_t));
    }//Todo add exception
}

ChatManager::~ChatManager() {
    UnmapViewOfFile(pShmChatmanager);
    CloseHandle(hMapFile);
}

uintptr_t ChatManager::GetPtrPrintChat() {
    return ptrPrintChat;
}

void ChatManager::CopyChat(const char* buffer) {
    if (pShmChatmanager->rdySend == true) {
        std::lock_guard<std::mutex> lg(pShmChatmanager->cmMutex);
        pShmChatmanager->rdySend = false;
        strcpy_s(pShmChatmanager->buffer, buffer);
    }
}

