#pragma once

#include "DarksideAPI.h"

#ifdef __cplusplus
extern "C" {
#endif

	extern __declspec(dllexport) DarksideAPI* CreateDarksideAPI();

	extern __declspec(dllexport) void DisposeDarksideAPI(DarksideAPI* apiObject);

	extern __declspec(dllexport) bool InjectPid(DarksideAPI* apiObject, int pid);

	extern __declspec(dllexport) int GetPid(DarksideAPI* apiObject);

	extern __declspec(dllexport) bool GetPlayerPosition(DarksideAPI* apiObject, LPVOID lpBuffer);

	extern __declspec(dllexport) bool SetPlayerHeading(DarksideAPI* apiObject, bool changeHeading, short newHeading);

	extern __declspec(dllexport) bool SetAutorun(DarksideAPI* apiObject, bool autorun);

	extern __declspec(dllexport) bool GetPartyMember(DarksideAPI* apiObject, int memberIndex, LPVOID lpBuffer);

	extern __declspec(dllexport) bool GetEntityInfo(DarksideAPI* apiObject, int entIndex, LPVOID lpBuffer);

	extern __declspec(dllexport) bool GetPlayerInfo(DarksideAPI* apiObject, LPVOID lpBuffer);

	extern __declspec(dllexport) bool GetTargetInfo(DarksideAPI* apiObject, LPVOID lpBuffer);

	extern __declspec(dllexport) bool SetTarget(DarksideAPI* apiObject, int entIndex);

	extern __declspec(dllexport) bool UseSkill(DarksideAPI* apiObject, int skillOffset);

	extern __declspec(dllexport) bool UseSpell(DarksideAPI* apiObject, int spellOffset);

	extern __declspec(dllexport) bool UsePetCmd(DarksideAPI* apiObject, int aggState, int walkState, int petCmd);

	extern __declspec(dllexport) bool GetChatline(DarksideAPI* apiObject, LPVOID lpBuffer);

	extern __declspec(dllexport) bool SendCommand(DarksideAPI* apiObject, LPVOID lpBuffer);
#ifdef __cplusplus
}
#endif