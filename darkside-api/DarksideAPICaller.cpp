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

bool InjectPid(DarksideAPI* apiObject, int pid) {
	if (apiObject != NULL) {
		if (apiObject->InjectPid(pid)) {
			return true;
		}
	}
	return false;
}

int GetPid(DarksideAPI* apiObject) {
	if (apiObject != NULL) {
		return (apiObject->GetPid());
	}
	return 0;
}

bool Unload(DarksideAPI* apiObject, int pid) {
	if (apiObject != NULL) {
		if (apiObject->Unload(pid)) {
			DisposeDarksideAPI(apiObject);
			return true;
		}
	}
	return false;
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

bool GetEntityList(DarksideAPI* apiObject, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetEntityList(lpBuffer)) {
			return true;
		}
	}
	return false;
}

bool GetPlayerInfo(DarksideAPI* apiObject, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetPlayerInfo(lpBuffer)) {
			return true;
		}
	}
	return false;
}

bool GetTargetInfo(DarksideAPI* apiObject, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetTargetInfo(lpBuffer)) {
			return true;
		}
	}
	return false;
}

bool SetTarget(DarksideAPI* apiObject, int entIndex) {
	if (apiObject != NULL) {
		if (apiObject->SetTarget(entIndex)) {
			return true;
		}
	}
	return false;
}

bool InteractRequest(DarksideAPI* apiObject, uint16_t objId) {
	if (apiObject != NULL) {
		if (apiObject->InteractRequest(objId)) {
			return true;
		}
	}
	return false;
}

bool UseSkill(DarksideAPI* apiObject, int skillOffset) {
	if (apiObject != NULL) {
		if (apiObject->UseSkill(skillOffset)) {
			return true;
		}
	}
	return false;
}

bool UseSpell(DarksideAPI* apiObject, int spellOffset) {
	if (apiObject != NULL) {
		if (apiObject->UseSpell(spellOffset)) {
			return true;
		}
	}
	return false;
}

bool UsePetCmd(DarksideAPI* apiObject, int aggState, int walkState, int petCmd) {
	if (apiObject != NULL) {
		if (apiObject->UsePetCmd(aggState, walkState, petCmd)) {
			return true;
		}
	}
	return false;
}

bool MoveItem(DarksideAPI* apiObject, int fromSlot, int toSlot, int count) {
	if (apiObject != NULL) {
		if (apiObject->MoveItem(fromSlot, toSlot, count)) {
			return true;
		}
	}
	return false;
}


bool GetChatline(DarksideAPI* apiObject, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->GetChatline(lpBuffer)) {
			return true;
		}
	}
	return false;
}

bool SendCommand(DarksideAPI* apiObject, int cmdMode, int iMode, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->SendCommand(cmdMode, iMode, lpBuffer)) {
			return true;
		}
	}
	return false;
}

bool SendPacket(DarksideAPI* apiObject, LPVOID lpBuffer) {
	if (apiObject != NULL) {
		if (apiObject->SendPacket(lpBuffer)) {
			return true;
		}
	}
	return false;
}

