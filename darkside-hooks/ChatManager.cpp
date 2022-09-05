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

    //Setup the PlayerInfo mmf
    //posInfommf_name = std::to_wstring(pid) + L"_pInfo";
    //std::size_t fileSize = sizeof(playerpos_t);
    //
    //auto hMapFile = CreateFileMapping(
    //    INVALID_HANDLE_VALUE,    // use paging file
    //    NULL,                    // default security
    //    PAGE_READWRITE,          // read/write access
    //    0,                       // maximum object size (high-order DWORD)
    //    fileSize,                // maximum object size (low-order DWORD)
    //    posInfommf_name.c_str());                 // name of mapping object
    //
    //if (hMapFile == NULL)
    //{
    //    _tprintf(TEXT("Plyr Pos Could not create file mapping object (%d).\n"),
    //        GetLastError());
    //}
    //
    //if (hMapFile != 0) {
    //    pShmPlayerPos = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    //}//Todo add exception
    //
    //if (pShmPlayerPos != NULL) {
    //    //Create new playerPositionInfo in order to update x/y offsets
    //    zoneYoffset = *(float*)ptrZoneYoffset_x;
    //    zoneXoffset = *(float*)ptrZoneXoffset_x;
    //    playerpos_t tempPos = *playerPositionInfo;
    //    tempPos.pos_x = tempPos.pos_x - zoneXoffset;
    //    tempPos.pos_y = tempPos.pos_y - zoneYoffset;
    //    tempPos.heading = (((((tempPos.heading + 0xcb6) + 0x800) * 0x168) / 0x1000) % 0x168);
    //    *pShmPlayerPos = tempPos;
    //}//Todo add exception
}

ChatManager::~ChatManager() {

}

uintptr_t ChatManager::GetPtrPrintChat() {
    return ptrPrintChat;
}

void ChatManager::CopyChat(const char* buffer) {
    strcpy_s(tmpBuff, buffer);
}

