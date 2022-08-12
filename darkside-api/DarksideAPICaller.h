#pragma once

#include "DarksideAPI.h"

#ifdef __cplusplus
extern "C" {
#endif

	extern __declspec(dllexport) DarksideAPI* CreateDarksideAPI();

	extern __declspec(dllexport) void DisposeDarksideAPI(DarksideAPI* apiObject);

	extern __declspec(dllexport) void InjectPid(DarksideAPI* apiObject, int pid);

	extern __declspec(dllexport) bool GetPlayerPosition(DarksideAPI* apiObject, LPVOID lpBuffer);

	extern __declspec(dllexport) bool SetAutorun(DarksideAPI* apiObject, bool autorun);

	extern __declspec(dllexport) bool GetPartyMember(DarksideAPI* apiObject, int memberIndex, LPVOID lpBuffer);
#ifdef __cplusplus
}
#endif