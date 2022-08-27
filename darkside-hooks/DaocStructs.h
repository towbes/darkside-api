#pragma once

struct playerpos_t {
    float pos_x;
    short heading;
    unsigned char unknown[68];
    float pos_y;
    unsigned char unknown2[8];
    float pos_z;
    char unknown4;
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

struct headingupdate_t {
	bool changeHeading;
	short heading;
};

//useSpell_t plyrUseSpellTable[150];
struct useSpell_t {
    char name[32];
    char unknown[60];
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
    unsigned char name[72];
    int unknown1;
};

//player hp/pow/endu
struct plyrinfo_t {
    int hp;
    int pow;
    int endu;
    useSkill_t skills[150];
    useSpell_t spells[150];
    buff_t buffs[75];
    item_t inventory[40];
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
    entity_t* tarEntPtr;
};

//Not a game struct, but used to update API with zone offsets
struct zoneoffset_t {
    float zoneYoffset;
    float zoneXoffset;
};