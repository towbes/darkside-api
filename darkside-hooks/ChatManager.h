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

    //SendPacket Shared memory
        //Send command shared memory
    sendPacket_t* pShmSendPkt;
    void* sendPktFile;
    std::wstring sendPktmmf_name;
    std::queue<std::string> sendPktQueue;

public:
    ChatManager();
    ~ChatManager();

    void CopyChat(const char* buffer);
    void QueueCommand();
    void QueueSendPacket();
};