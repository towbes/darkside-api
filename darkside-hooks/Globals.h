#pragma once
#include "DaocGame.h"
#include "DaocStructs.h"
#include "pch.h"

template <typename T, typename = std::enable_if_t<std::is_integral_v<T>, void>>
inline uintptr_t FixDaocGameOffset(T nOffset)
{
	return static_cast<uintptr_t>(nOffset);
}


#define INITIALIZE_DAOCGAME_OFFSET(var) uintptr_t var = FixDaocGameOffset(var##_x)

// game
uintptr_t moduleBase;
uintptr_t ptrServerNameAddress;

// Functions
uintptr_t funcSendPacket;
uintptr_t funcRecvPacket;
uintptr_t funcSellItem;

// Player
uintptr_t ptrAutorunToggle2;
uintptr_t funcRunSpeed;
uintptr_t ptrPlayerPosition;
playerpos_t* playerPositionInfo;
