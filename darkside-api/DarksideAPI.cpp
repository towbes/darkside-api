#include "DarksideAPI.h"
#include "simple-inject.h"
#include <format>

struct playerpos_t {
    float pos_x;
    short heading;
    unsigned char unknown[68];
    float pos_y;
    unsigned char unknown2[8];
    float pos_z;
    char unknown4;
};

DarksideAPI::DarksideAPI() {}

DarksideAPI::~DarksideAPI() {}

void DarksideAPI::InjectPid(int pid) {
    simpleInject("C:\\Users\\ritzgames\\Desktop\\daoc\\darkside\\darkside-api\\DarksideGUI\\bin\\Debug\\net6.0-windows\\darkside-hooks.dll", (DWORD)pid);
    std::wstring msg = std::format(L"Injected {}\n", pid);
    MessageBox(0, msg.c_str(), L"Hi", MB_ICONINFORMATION);
    this->pidHandle = pid;
}

bool DarksideAPI::GetPlayerInfo(LPVOID lpBuffer) {
    std::wstring msg = std::format(L"GetPlayerInfo {}\n", this->pidHandle);
    MessageBox(0, msg.c_str(), L"Hi", MB_ICONINFORMATION);
    playerpos_t test = { 30000, 300, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 50000, "bbbbbbb", 6000, 'c' };
    memcpy_s(lpBuffer, sizeof(playerpos_t), &test, sizeof(playerpos_t));
    //Use pid to communicate with the proper DLL
    return true;
}