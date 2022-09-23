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
    ptrPlayerName = (void*)ptrPlayerName_x;
    ptrPlayerEntIndex = *(int*)ptrGameState_x;
    ptrPetEntIndex = (void*)ptrPetEntIndex_x;
    afkcheck = (int*)ptrMouseOrKeyPress_x;

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
        //Pointer to grab hp/pow/endu offsets
        unsigned char* tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerHp);
        pShmPlayerInfo->hp = *(int*)tempPtr;
        pShmPlayerInfo->pow = *(int*)(tempPtr + 0x4);
        pShmPlayerInfo->endu = *(int*)(tempPtr + 0x8);
        //Ptr to player name, class is at +0xa8
        tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerName);
        memcpy(pShmPlayerInfo->name, tempPtr, sizeof(pShmPlayerInfo->name));
        memcpy(pShmPlayerInfo->className, (tempPtr + 0xa8), sizeof(pShmPlayerInfo->name));
        //Copy the structs for skill/spells/buffs/inventory
        memcpy(pShmPlayerInfo->skills, ptrUseSkills, sizeof(pShmPlayerInfo->skills));
        memcpy(pShmPlayerInfo->spells, ptrUseSpells, sizeof(pShmPlayerInfo->spells));
        memcpy(pShmPlayerInfo->buffs, ptrBuffs, sizeof(pShmPlayerInfo->buffs));
        memcpy(pShmPlayerInfo->inventory, ptrInventory, sizeof(pShmPlayerInfo->inventory));
        //Player entity list index
        tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerEntIndex);
        //entIndex is at 0x64;
        pShmPlayerInfo->entIndex = *(int*)(tempPtr + 0x64);
        pShmPlayerInfo->petEntIndex = *(int*)ptrPetEntIndex;

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
    else {
        pShmUseSkill = (int*)MapViewOfFile(skillMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }
    
    if (pShmUseSkill == NULL) {
        _tprintf(TEXT("Skill Map Could not create file mapping object (%d).\n"),
            GetLastError());
    }
    else {
        //Set to -1 while waiting for skill
        *pShmUseSkill = -1;
    }

    //set up skill and spell casting function shaerd memory
    spellmmf_name = std::to_wstring(pid) + L"_plyrUseSpell";
    std::size_t useSpellfileSize = sizeof(spellQueue_t);

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
    else {
        pShmUseSpell = (spellQueue_t*)MapViewOfFile(spellMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }

    

    if (pShmUseSpell == NULL) {
        _tprintf(TEXT("Spell map Could not create file mapping object (%d).\n"),
            GetLastError());
    }
    else {
        spellQueue_t temp = { true, 0,0 };
        //Set to -1 while waiting for spell
        memcpy(pShmUseSpell, &temp, sizeof(spellQueue_t));
    }

    //set up petCommand shaerd memory
    petCmdmmf_name = std::to_wstring(pid) + L"_plyrPetCmd";
    std::size_t petCmdfileSize = sizeof(petCmd_t);

    auto petCmdMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        petCmdfileSize,                // maximum object size (low-order DWORD)
        petCmdmmf_name.c_str());                 // name of mapping object

    if (petCmdMapFile == NULL)
    {
        _tprintf(TEXT("Spell map Could not create file mapping object (%d).\n"),
            GetLastError());
    }
    else {
        pShmPetCmd = (petCmd_t*)MapViewOfFile(petCmdMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }

    if (pShmPetCmd == NULL) {
        _tprintf(TEXT("Spell map Could not create file mapping object (%d).\n"),
            GetLastError());
    }
    else {
        //Set to -1 while waiting for cmd
        petCmd_t tmpPet = { -1,-1,-1 };
        memcpy(pShmPetCmd, &tmpPet, sizeof(petCmd_t));
    }

    //set up MoveItem shaerd memory
    moveItemmmf_name = std::to_wstring(pid) + L"_moveItem";
    std::size_t moveItemfileSize = sizeof(moveItem_t);

    auto moveItemMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        moveItemfileSize,                // maximum object size (low-order DWORD)
        moveItemmmf_name.c_str());                 // name of mapping object

    if (moveItemMapFile == NULL)
    {
        _tprintf(TEXT("MoveItem Could not create file object (%d).\n"),
            GetLastError());
    }
    else {
        pShmMoveItem = (moveItem_t*)MapViewOfFile(moveItemMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);
    }

    if (pShmMoveItem == NULL) {
        _tprintf(TEXT("MoveItem Could not create file mapping object (%d).\n"),
            GetLastError());
        CloseHandle(moveItemMapFile);
    }
    else {
        //Set to -1 while waiting for cmd
        moveItem_t tmpMove = { true, -1,-1,-1 };
        memcpy(pShmMoveItem, &tmpMove, sizeof(moveItem_t));
    }

}

//Deconstructor to release shared memory
PlayerInfo::~PlayerInfo() {
    UnmapViewOfFile(pShmPlayerInfo);
    CloseHandle(hMapFile);
    UnmapViewOfFile((LPCVOID)pShmUseSkill);
    CloseHandle(skillMapFile);
    UnmapViewOfFile((LPCVOID)pShmUseSpell);
    CloseHandle(spellMapFile);
    UnmapViewOfFile((LPCVOID)pShmPetCmd);
    CloseHandle(petCmdMapFile);
}

//Update the plyrInfo_t struct in shared memory
void PlayerInfo::GetPlayerInfo() {
    if (pShmPlayerInfo != NULL) {
        //Set mouse / key press to 1
        *(int*)afkcheck = 1;
        //Pointer to grab hp/pow/endu offsets
        unsigned char* tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerHp);
        pShmPlayerInfo->hp = *(int*)tempPtr;
        pShmPlayerInfo->pow = *(int*)(tempPtr + 0x4);
        pShmPlayerInfo->endu = *(int*)(tempPtr + 0x8);
        //Ptr to player name, class is at +0x168
        tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerName);
        memcpy(pShmPlayerInfo->name, tempPtr, sizeof(pShmPlayerInfo->name));
        memcpy(pShmPlayerInfo->className, (tempPtr + 0xa8), sizeof(pShmPlayerInfo->name));
        //Copy the structs for skill/spells/buffs/inventory
        memcpy(pShmPlayerInfo->skills, ptrUseSkills, sizeof(pShmPlayerInfo->skills));
        memcpy(pShmPlayerInfo->spells, ptrUseSpells, sizeof(pShmPlayerInfo->spells));
        memcpy(pShmPlayerInfo->buffs, ptrBuffs, sizeof(pShmPlayerInfo->buffs));
        memcpy(pShmPlayerInfo->inventory, ptrInventory, sizeof(pShmPlayerInfo->inventory));
        //Player entity list index
        tempPtr = reinterpret_cast<unsigned char*>(ptrPlayerEntIndex);
        //entIndex is at 0x64;
        pShmPlayerInfo->entIndex = *(int*)(tempPtr + 0x64);
        pShmPlayerInfo->petEntIndex = *(int*)ptrPetEntIndex;
    }
}

//Function runs each frame
//API will update shared memory with skill offset #
//Function pushes that to queue and resets shared memory to -1
//Todo: add a condition to only try to empty the queue when not casting or moving
void PlayerInfo::QueueSkill() {
    if (*pShmUseSkill >= 0) {
        skillQueue.push(*pShmUseSkill);
        *pShmUseSkill = -1;
    }
    if (!skillQueue.empty()) {
        currSkill = skillQueue.front();
        skillQueue.pop();
        daoc::UseSkill(currSkill, 1);
    }
}

//Function runs each frame
//API will update shared memory with spell offset #
//Function pushes that to queue and resets shared memory to -1
//Todo: add a condition to only try to empty the queue when not casting or moving
void PlayerInfo::QueueSpell() {
    if (pShmUseSpell->rdySend == false) {
        daoc::UseSpell(pShmUseSpell->spellCategory, pShmUseSpell->spellLevel);
        pShmUseSpell->rdySend = true;
    }
}

//Function runs each frame
//API will update shared memory with petCmd_t object info
//Function pushes that to queue and resets aggroState to -1
//The action applies to players current target
void PlayerInfo::QueuePetCmd() {
    if (pShmPetCmd->aggroState >= 0) {
        petCmdQueue.push(*pShmPetCmd);
        pShmPetCmd->aggroState = -1;
    }
    if (!petCmdQueue.empty()) {
        currPetCmd = petCmdQueue.front();
        petCmdQueue.pop();
        daoc::UsePetCommand(currPetCmd.aggroState, currPetCmd.walkState, currPetCmd.petCmd);
    }
}

//Function runs each frame
//MoveItem
void PlayerInfo::QueueMoveItem() {
    if (pShmMoveItem->rdyMove == false) {
        daoc::MoveItem(pShmMoveItem->toSlot, pShmMoveItem->fromSlot, pShmMoveItem->count);
        pShmMoveItem->rdyMove = true;
    }
}