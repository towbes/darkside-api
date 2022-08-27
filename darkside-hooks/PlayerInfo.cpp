#include "pch.h"
#include "PlayerInfo.h"
#include "daochooks.h"

PlayerInfo::PlayerInfo() {
    //Initialize daocgame pointers
    ptrPlayerHp = (void*)ptrPlayerHp_x;
    ptrUseSkills = (void*)ptrPlyrUseSkill_x;
    ptrUseSpells = (void*)ptrPlyrSpells_x;
    ptrBuffs = (void*)ptrPlyrBuffs_x;
    ptrInventory = (void*)ptrInventory_x;

#ifdef _DEBUG
    std::cout << "ptrPlayerHp: " << std::hex << (int)ptrPlayerHp << std::endl;
#endif

    //Get Process ID
    pid = GetCurrentProcessId();
    //https://stackoverflow.com/questions/5235647/c-concat-lpctstr
    //https://stackoverflow.com/questions/12602526/how-can-i-convert-an-int-to-a-cstring

    //Setup the PlayerInfo mmf
    plyrInfommf_name = std::to_wstring(pid) + L"_plyrHp";
    std::size_t fileSize = sizeof(plyrinfo_t);

    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        plyrInfommf_name.c_str());                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("PlayerInfo Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (hMapFile != 0) {
        pShmPlayerInfo = (plyrinfo_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception


    if (pShmPlayerInfo != NULL) {
        unsigned char* tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerHp);
        pShmPlayerInfo->hp = *(int*)tempPtr;
        pShmPlayerInfo->pow = *(int*)(tempPtr + 0x4);
        pShmPlayerInfo->endu = *(int*)(tempPtr + 0x8);
        memcpy(pShmPlayerInfo->skills, ptrUseSkills, sizeof(pShmPlayerInfo->skills));
        memcpy(pShmPlayerInfo->spells, ptrUseSpells, sizeof(pShmPlayerInfo->spells));
        memcpy(pShmPlayerInfo->buffs, ptrBuffs, sizeof(pShmPlayerInfo->buffs));
        memcpy(pShmPlayerInfo->inventory, ptrInventory, sizeof(pShmPlayerInfo->inventory));

#ifdef _DEBUG
        std::cout << "plyrHP: " << std::dec << pShmPlayerInfo->hp << std::endl;
        std::cout << "plyrPow: " << std::dec << pShmPlayerInfo->pow << std::endl;
        std::cout << "plyrEndu: " << std::dec << pShmPlayerInfo->endu << std::endl;
        std::cout << "Skill 1: " << std::dec << pShmPlayerInfo->skills[0].name << std::endl;
        std::cout << "Spell 1: " << std::dec << pShmPlayerInfo->spells[0].name << std::endl;
        std::cout << "Buff 1: " << std::dec << pShmPlayerInfo->buffs[0].name << std::endl;
        std::cout << "Item 1: " << std::dec << pShmPlayerInfo->inventory[0].itemName << std::endl;
#endif
    }//Todo add exception

    //set up skill and spell casting function shaerd memory
    skillmmf_name = std::to_wstring(pid) + L"_plyrUseSkill";
    std::size_t useSkillfileSize = sizeof(int);

    auto skillMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        useSkillfileSize,                // maximum object size (low-order DWORD)
        skillmmf_name.c_str());                 // name of mapping object

    if (skillMapFile == NULL)
    {
        _tprintf(TEXT("Skill Map Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (skillMapFile != 0) {
        pShmUseSkill = (int)MapViewOfFile(skillMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    //Set to -1 while waiting for skill
    pShmUseSkill = -1;

        //set up skill and spell casting function shaerd memory
    spellmmf_name = std::to_wstring(pid) + L"_plyrUseSpell";
    std::size_t useSpellfileSize = sizeof(int);

    auto spellMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        useSpellfileSize,                // maximum object size (low-order DWORD)
        spellmmf_name.c_str());                 // name of mapping object

    if (spellMapFile == NULL)
    {
        _tprintf(TEXT("Spell map Could not create file mapping object (%d).\n"),
            GetLastError());
    }

    if (spellMapFile != 0) {
        pShmUseSpell = (int)MapViewOfFile(spellMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }//Todo add exception

    //Set to -1 while waiting for spell
    pShmUseSpell = -1;
}

PlayerInfo::~PlayerInfo() {
    UnmapViewOfFile(pShmPlayerInfo);
    CloseHandle(hMapFile);
    UnmapViewOfFile((LPCVOID)pShmUseSkill);
    CloseHandle(skillMapFile);
    UnmapViewOfFile((LPCVOID)pShmUseSpell);
    CloseHandle(spellMapFile);
}

void PlayerInfo::GetPlayerInfo() {
    if (pShmPlayerInfo != NULL) {
        unsigned char* tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerHp);
        pShmPlayerInfo->hp = *(int*)tempPtr;
        pShmPlayerInfo->pow = *(int*)(tempPtr + 0x4);
        pShmPlayerInfo->endu = *(int*)(tempPtr + 0x8);
        memcpy(pShmPlayerInfo->skills, ptrUseSkills, sizeof(pShmPlayerInfo->skills));
        memcpy(pShmPlayerInfo->spells, ptrUseSpells, sizeof(pShmPlayerInfo->spells));
        memcpy(pShmPlayerInfo->buffs, ptrBuffs, sizeof(pShmPlayerInfo->buffs));
        memcpy(pShmPlayerInfo->inventory, ptrInventory, sizeof(pShmPlayerInfo->inventory));
    }
}

void PlayerInfo::QueueSkill() {
    if (pShmUseSkill >= 0) {
        skillQueue.push(pShmUseSkill);
        pShmUseSkill = -1;
    }
    if (!skillQueue.empty()) {
        currSkill = skillQueue.front();
        skillQueue.pop();
        daoc::UseSkill(currSkill, 1);
    }
}

void PlayerInfo::QueueSpell() {
    if (pShmUseSpell >= 0) {
        skillQueue.push(pShmUseSpell);
        pShmUseSpell = -1;
    }
    if (!spellQueue.empty()) {
        currSpell = spellQueue.front();
        spellQueue.pop();
        daoc::UseSpell(1, currSpell);
    }
}