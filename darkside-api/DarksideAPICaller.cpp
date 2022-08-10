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