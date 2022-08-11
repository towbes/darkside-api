#pragma once

#include "DarksideAPI.h"

#ifdef __cplusplus
extern "C" {
#endif

	extern __declspec(dllexport) DarksideAPI* CreateDarksideAPI();

	extern __declspec(dllexport) void DisposeDarksideAPI(DarksideAPI* apiObject);

	extern __declspec(dllexport) void InjectPid(DarksideAPI* apiObject, int pid);

	extern __declspec(dllexport) bool GetPlayerInfo(DarksideAPI* apiObject, LPVOID lpBuffer);
#ifdef __cplusplus
}
#endif