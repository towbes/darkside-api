#include "pch.h"
#include "DarksideAPI.h"

std::vector<char> HexToBytes(const std::string& hex) {
    std::vector<char> bytes;

    for (unsigned int i = 0; i < hex.length(); i += 2) {
        std::string byteString = hex.substr(i, 2);
        char byte = (char)strtol(byteString.c_str(), NULL, 16);
        bytes.push_back(byte);
    }

    return bytes;
}

void DarksideAPI::ChatListener() {
    //Setup the Chat Manager mmf
    std::wstring chatManagermmf_name = std::to_wstring(pidHandle) + L"_ChatMan";
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
        _tprintf(TEXT("Chatman Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    chatManager_t* pShmChatmanager = (chatManager_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (pShmChatmanager == NULL) {
        _tprintf(TEXT("Chatman could not map file (%d).\n"),
            GetLastError());
        UnmapViewOfFile(hMapFile);
    }
    //Add a sleep to give injected DLL time to set up shared memory
    Sleep(200);

    while (injected) {
        if (pShmChatmanager->rdySend == false) {
            std::string tmp = std::string(pShmChatmanager->buffer);
            if (tmp.length() > 0) {
                chatLog.push(tmp);
            }
            //::OutputDebugStringA(std::format("Text: {}", tmp).c_str());
            pShmChatmanager->rdySend = true;
        }
        Sleep(10);
    }
}

bool DarksideAPI::GetChatline(LPVOID lpBuffer) {
    if (chatLog.size() > 0) {
        //::OutputDebugStringA(std::format("Text: {}", chatLog.front()).c_str());
        ::strcpy_s((char*)lpBuffer, 2048, chatLog.front().c_str());
        chatLog.pop();
        return true;
    }
    else {
        memset(lpBuffer, 0, 2048);
    }
    return false;
}

bool DarksideAPI::SendCommand(int cmdMode, int iMode, LPVOID lpBuffer) {
    //Setup the Chat Manager mmf
    std::wstring sendCmdmmf_name = std::to_wstring(pidHandle) + L"_SendCmd";
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
        _tprintf(TEXT("SendCmd Could not create file mapping object (%d).\n"),
            GetLastError());
        return false;
    }

    sendCmd_t* pShmSendCmd = (sendCmd_t*)MapViewOfFile(sendCmdFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (pShmSendCmd == NULL) {
        _tprintf(TEXT("SendCmd Could not create map of file mapping object (%d).\n"),
            GetLastError());
        CloseHandle(sendCmdFile);
        return false;

    }
    
    
    if (pShmSendCmd->rdyRecv == true) {
        //std::scoped_lock<std::mutex> lg(pShmSendCmd->cmdMutex);
        pShmSendCmd->cmdMode = cmdMode;
        pShmSendCmd->iMode = iMode;
        strcpy_s(pShmSendCmd->buffer, sizeof(char) * 512, (const char*)lpBuffer);
        pShmSendCmd->rdyRecv = false;
    }
    else {
        memset(lpBuffer, 0, 512);
        UnmapViewOfFile(pShmSendCmd);
        CloseHandle(sendCmdFile);
        return false;
    }

    UnmapViewOfFile(pShmSendCmd);
    CloseHandle(sendCmdFile);
    return true;

}

bool DarksideAPI::SendPacket(LPVOID lpBuffer) {
    //Setup the Send Packet  mmf
    std::wstring sendPktmmf_name = std::to_wstring(pidHandle) + L"_SendPkt";
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
        _tprintf(TEXT("sendpacket Could not create file object (%d).\n"),
            GetLastError());
        return false;
    }

    sendPacket_t* pShmSendPkt = (sendPacket_t*)MapViewOfFile(sendPktFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    if (pShmSendPkt == NULL)
    {
        _tprintf(TEXT("Sendpacket Could not create file mapping object (%d).\n"),
            GetLastError());
        CloseHandle(sendPktFile);
        return false;
    }

    //Convert the incoming buffer to bytes
    //incoming buffer is in ascii hex with space between: AA BB CC DD EE FF 
    //first bytes are packet header

    std::string hexStr = std::string((const char*)lpBuffer);

    hexStr.erase(remove_if(hexStr.begin(), hexStr.end(), isspace), hexStr.end());

    std::vector<char> test = HexToBytes(hexStr);

    char header = test.at(0);

    test.erase(test.begin());

    
    if (pShmSendPkt->rdySendPkt == true) {
        //Possible race condition while debugging. Probably need to switch these to unique_locks
        //std::scoped_lock<std::mutex> lg(pShmSendPkt->pktMutex);
        memset(pShmSendPkt->packetBuffer, 0, 2048);
        memcpy(pShmSendPkt->packetBuffer, &test[0], test.size() - 1);
        pShmSendPkt->packetHeader = (DWORD)header;
        pShmSendPkt->packetLen = test.size() - 1;
        pShmSendPkt->unknown = 0;
        pShmSendPkt->rdySendPkt = false;
        //memset(lpBuffer, 0, 2048);
    }
    else {
        //memset(lpBuffer, 0, 2048);
        UnmapViewOfFile(pShmSendPkt);
        CloseHandle(sendPktFile);
        false;
    }
    UnmapViewOfFile(pShmSendPkt);
    CloseHandle(sendPktFile);
    return true;

}