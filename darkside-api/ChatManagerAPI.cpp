#include "pch.h"
#include "DarksideAPI.h"

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

    while (true) {
        if (pShmChatmanager->rdySend == false) {
            std::lock_guard<std::mutex> lg(pShmChatmanager->cmMutex);
            std::string tmp = std::string(pShmChatmanager->buffer);
            chatLog.push(tmp);
            ::OutputDebugStringA(std::format("Text: {}", tmp).c_str());
            pShmChatmanager->rdySend = true;
        }
        Sleep(10);
    }
}

bool DarksideAPI::GetChatline(LPVOID lpBuffer) {
    if (chatLog.size() > 0) {
        memcpy(lpBuffer, chatLog.front().c_str(), 512);
        chatLog.pop();
        return true;
    }
    else {
        memset(lpBuffer, 0, 512);
    }
    return false;
}