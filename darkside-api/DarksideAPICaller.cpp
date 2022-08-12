#include "DarksideAPICaller.h"

DarksideAPI* CreateDarksideAPI() {
	return new DarksideAPI();
}

void DisposeDarksideAPI(DarksideAPI* apiObject) {
	if (apiObject != NULL) {
		delete apiObject;
		apiObject = NULL;
	}
}

void InjectPid(DarksideAPI* apiObject, int pid) {
	if (apiObject != NULL) {
		apiObject->InjectPid(pid);
	}
}

bool GetPlayerInfo(DarksideAPI* apiObject, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetPlayerInfo(lpBuffer)) {
			return true;
		}
		
	}
	return false;
}

bool SetAutorun(DarksideAPI* apiObject, bool autorun) {
	if (apiObject != NULL) {
		if (apiObject->SetAutorun(autorun)) {
			return true;
		}
	}
	return false;
}