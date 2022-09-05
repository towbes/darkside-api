#pragma once
#include "DaocStructs.h"


class ChatManager
{
private:
    //Current process id
    int pid;

    //player info shared memory
    uintptr_t ptrPrintChat;
    chatManager_t* pShmChatmanager;
    void* hMapFile;
    std::wstring chatManagermmf_name;

    char tmpBuff[512];

public:
    ChatManager();
    ~ChatManager();

    uintptr_t GetPtrPrintChat();
    void CopyChat(const char* buffer);
};