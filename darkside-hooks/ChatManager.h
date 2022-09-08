#pragma once
#include "DaocStructs.h"


class ChatManager
{
private:
    //Current process id
    int pid;

    //Incoming Chat message shared memory
    chatManager_t* pShmChatmanager;
    void* hMapFile;
    std::wstring chatManagermmf_name;

    char tmpBuff[512];

    //Send command shared memory
    sendCmd_t* pShmSendCmd;
    void* sendCmdFile;
    std::wstring sendCmdmmf_name;
    std::queue<std::string> sendCmdQueue;
    char recvCmdBuf[512];
    char cmdBuf[512];

public:
    ChatManager();
    ~ChatManager();

    void CopyChat(const char* buffer);
    void QueueCommand();
};