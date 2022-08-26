#pragma once
// game.dll offsets
#define moduleBase_x		0x400000
#define ptrServerNameAddress_x		0x0104A6A8

// Functions
#define funcSendPacket_x		0x4281df
#define funcRecvPacket_x		0x427f5e
#define funcSellItem_x		0x42b2e3
#define funcUseSkill_x		0x42b5d5
#define funcUseSpell_x		0x42b4b8
#define funcSetTarget_x		0x43a8f3
#define funcSetTargetUI_x		0x48f194

// Entities
#define funcGetEntityPointer_x		0x43589f
#define funcGetEntityName_x		0x4358ee
#define funcEntityPtrSanityCheck_x		0x4358bf
#define funcGetNPCEntityOffsetFromOid_x		0x411721
#define ptrEntityListMax_x		0xaa4c5c

// Player related
#define ptrAutorunToggle2_x		0x1049898
#define funcForwardRunSpeed_x		0x438db7
#define ptrPlayerPosition_x		0x0104A7FC
#define ptrZoneYoffset_x		0x1495820
#define ptrZoneXoffset_x		0x149581c
#define ptrPlayerHp_x		0xf69df0
#define ptrPlyrSpells_x		0x161da70
#define ptrPlyrBuffs_x		0xfdd300
#define ptrPlyrUseSkill_x		0x9a9730
#define ptrCurrentTargetEntOffset_x		0x10498b0
#define ptrInventory_x		0xf9b8d0

// Party related
#define ptrPartyMemberInfo_x		0x1672ae0

