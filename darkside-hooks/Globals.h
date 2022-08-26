#pragma once
#include "pch.h"

extern HMODULE ghModule;

struct apicmd_t {
	char test[10];
};

// game
extern uintptr_t moduleBase;
extern uintptr_t ptrServerNameAddress;

// Functions
extern uintptr_t funcSendPacket;
extern uintptr_t funcRecvPacket;
extern uintptr_t funcSellItem;

// Player
extern uintptr_t ptrAutorunToggle2;
extern uintptr_t funcRunSpeed;
extern uintptr_t ptrPlayerPosition;
extern uintptr_t playerPositionInfo;

