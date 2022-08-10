#include "DarksideAPI.h"
#include "simple-inject.h"
#include <format>


DarksideAPI::DarksideAPI() {}

DarksideAPI::~DarksideAPI() {}

void DarksideAPI::InjectPid(int pid) {
    simpleInject("C:\\Users\\ritzgames\\Desktop\\daoc\\darkside\\darkside-api\\DarksideGUI\\bin\\Debug\\net6.0-windows\\daoc-logger.dll", (DWORD)pid);
    std::wstring msg = std::format(L"Injected {}\n", pid);
    MessageBox(0, msg.c_str(), L"Hi", MB_ICONINFORMATION);
    this->pidHandle = pid;
}