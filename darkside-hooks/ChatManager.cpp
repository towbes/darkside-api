#include "pch.h"
#include "ChatManager.h"
#include "daochooks.h"

ChatManager::ChatManager() {
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

    //Setup the Send Command  mmf
    sendCmdmmf_name = std::to_wstring(pid) + L"_SendCmd";
    std::size_t cmdFileSize = sizeof(sendCmd_t);
    //
    auto sendCmdFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        cmdFileSize,                // maximum object size (low-order DWORD)
        sendCmdmmf_name.c_str());                 // name of mapping object
    //
    if (sendCmdFile == NULL)
    {
        _tprintf(TEXT("Plyr Pos Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (sendCmdFile != 0) {
        pShmSendCmd = (sendCmd_t*)MapViewOfFile(sendCmdFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception
    //
    if (pShmSendCmd != NULL) {
        //initialize a new chat maanager struct
        sendCmd_t sendCmd;
        //Flag that current client is ready to receive a command from API
        sendCmd.rdyRecv = true;
        //copy that struct to shared memory
        memcpy(pShmSendCmd, &sendCmd, sizeof(sendCmd_t));
    }//Todo add exception

        //Setup the Send Packet  mmf
    sendPktmmf_name = std::to_wstring(pid) + L"_SendPkt";
    std::size_t pktFileSize = sizeof(sendPacket_t);
    //
    auto sendPktFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        pktFileSize,                // maximum object size (low-order DWORD)
        sendPktmmf_name.c_str());                 // name of mapping object
    //
    if (sendPktFile == NULL)
    {
        _tprintf(TEXT("Plyr Pos Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (sendPktFile != 0) {
        pShmSendPkt = (sendPacket_t*)MapViewOfFile(sendPktFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception
    //
    if (pShmSendPkt != NULL) {
        //initialize a new chat maanager struct
        sendPacket_t sendPkt;
        //Flag that current client is ready to receive a command from API
        sendPkt.rdySendPkt = true;
        //copy that struct to shared memory
        memcpy(pShmSendPkt, &sendPkt, sizeof(sendPacket_t));
    }//Todo add exception

}

ChatManager::~ChatManager() {
    UnmapViewOfFile(pShmChatmanager);
    CloseHandle(hMapFile);
    UnmapViewOfFile(pShmSendCmd);
    CloseHandle(sendCmdFile);
    UnmapViewOfFile(pShmSendPkt);
    CloseHandle(sendPktFile);
}

void ChatManager::CopyChat(const char* buffer) {
    if (pShmChatmanager->rdySend == true) {
        //std::scoped_lock<std::mutex> lg(pShmChatmanager->cmMutex);
        strcpy_s(pShmChatmanager->buffer, buffer);
        pShmChatmanager->rdySend = false;
    }
}

void ChatManager::QueueCommand() {

    
    if (pShmSendCmd->rdyRecv == false) {
        //std::scoped_lock<std::mutex> lg(pShmSendCmd->cmdMutex);
        //std::string tmpString = std::string(pShmSendCmd->buffer);
        //sendCmdQueue.push(tmpString);
        daoc::SendCommand(pShmSendCmd->cmdMode, pShmSendCmd->iMode, pShmSendCmd->buffer);
        pShmSendCmd->rdyRecv = true;
    }
    //if (!sendCmdQueue.empty()) {
    //    strcpy_s(cmdBuf, sendCmdQueue.front().c_str());
    //    sendCmdQueue.pop();
    //    daoc::SendCommand(cmdBuf);
    //}
}

void ChatManager::QueueSendPacket() {

    
    if (pShmSendPkt->rdySendPkt == false) {
        //std::scoped_lock<std::mutex> lg(pShmSendPkt->pktMutex);
        //static char pktBuf[2048];
        //memcpy(pktBuf, pShmSendPkt->packetBuffer, pShmSendPkt->packetLen);
        daoc::SendPacket((const char*)(pShmSendPkt->packetBuffer), pShmSendPkt->packetHeader, pShmSendPkt->packetLen, pShmSendPkt->unknown);
        pShmSendPkt->rdySendPkt = true;
    }

}