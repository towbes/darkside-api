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

bool GetPlayerPosition(DarksideAPI* apiObject, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetPlayerPosition(lpBuffer)) {
			return true;
		}
		
	}
	return false;
}

bool SetPlayerHeading(DarksideAPI* apiObject, bool changeHeading, short newHeading) {
	if (apiObject != NULL) {
		if (apiObject->SetPlayerHeading(changeHeading, newHeading)) {
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

bool GetPartyMember(DarksideAPI* apiObject, int memberIndex, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetPartyMember(memberIndex, lpBuffer)) {
			return true;
		}
	}
	return false;
}

bool GetEntityInfo(DarksideAPI* apiObject, int entIndex, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetEntityInfo(entIndex, lpBuffer)) {
			return true;
		}
	}
	return false;
}