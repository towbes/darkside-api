#pragma once

struct playerpos_t {
    float pos_x;                //0x0
    short heading;              //0x4
    unsigned char unknown[20];  
    int playerSpeedFwd;         //0x1c
    unsigned char unknown2[12];
    int momentumMaxFwdBack;     //0x2c
    float momentumFwdBack;      //0x30
    float momentumLeftRight;    //0x34
    unsigned char unknown3[12];
    float momentumFwdBackWrite; //0x44
    int unknown4;
    float pos_y;                //0x4c
    float writeablePos_zAdd;    //0x50
    int unknown5;
    float pos_z;                //0x58
    unsigned char unknown6[16];
    int rotatePlayer;           //0x6c
};

struct partymemberinfo_t {
	int hp_pct;
	int endu_pct;
	int unknown;
	int pow_pct;
	unsigned char name[20];
	unsigned char unknown2[24];
	unsigned char class_name[24];
	unsigned char unknown3[4616];
};

struct partymembers_t {
	partymemberinfo_t members[8];
};

struct positionUpdate_t {
	bool changeHeading;
    bool changeFwd;
    bool changeStrafe;
    short newHeading;
    float newFwd;
    float newStrafe;
};

//useSpell_t plyrUseSpellTable[150];
struct useSpell_t {
    char name[64];
    short spellLevel;
    short unknown1;
    int tickCount;
    int unknown2;
    int unknown3;
    int unknown4;
    int unknown5;
    int unknown6;
};

//array start address is 161d9f0
//6968 bytes total = 0x1B38
struct spellCategory_t {
    char categoryName[64];
    useSpell_t spellArray[75];
    int alignBuf;
};

struct spellQueue_t {
    bool rdySend;
    int spellCategory;
    int spellLevel;
};

struct petCmd_t {
    int aggroState;
    int walkState;
    int petCmd;
};

//item structure starts at 0xf9b8d0
struct item_t {
    int itemId;
    unsigned char unknown[72];
    unsigned char itemName[80];
    unsigned char unknown2[312];
};

//Player Buffs
//buff_t plyrBuffTable[75];
struct buff_t {
    unsigned char name[64];
    int unknown1;
    int buffTimeRemaining; //milliseconds
    int slotNumber;
    int tooltipId;
    unsigned char unknown2[20];
    int buffId;
    unsigned char unknown3[64];
};

////Skills
//useSkill_t plyrUseSkillTable[150];
struct useSkill_t {
    unsigned char name[64];
    int unknown1;
    int unknown2;
    int tickCount;
};

//player hp/pow/endu
struct plyrinfo_t {
    int hp;
    int pow;
    int endu;
    unsigned char name[168];
    unsigned char className[168];
    useSkill_t skills[150];
    spellCategory_t spells[8];
    buff_t buffs[75];
    item_t inventory[40];
    int entIndex;
    int petEntIndex;
};

//entList[2000]
struct entity_t {
    char bytes[6518];
};

//entNameList[2000]
struct entName_t {
    char name[48];
};

struct targetInfo_t {
    int entOffset;
    int tarHp;
    int tarColor;
    char tarName[48];
    char hasTarget[4];
};

//Not a game struct, but used to update API with zone offsets
struct zoneoffset_t {
    float zoneYoffset;
    float zoneXoffset;
};

struct chatManager_t {
    std::mutex cmMutex;
    //Flag if API is ready for a new message
    bool rdySend;
    char buffer[2048];
};

struct sendCmd_t {
    std::mutex cmdMutex;
    bool rdyRecv;
    int cmdMode;
    int iMode;
    char buffer[512];
};

struct sendPacket_t {
    std::mutex pktMutex;
    bool rdySendPkt;
    unsigned char packetBuffer[2048];
    DWORD packetHeader;
    DWORD packetLen;
    DWORD unknown;
};

struct moveItem_t {
    bool rdyMove;
    int fromSlot;
    int toSlot;
    int count;
};