// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "Globals.h"



DWORD WINAPI MainThread(HMODULE hModule) {
#ifdef _DEBUG
    //Create Console
    AllocConsole();
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    std::cout << "DLL got injected!" << std::endl;
#endif 

    //Initialize player pointer
    ptrPlayerPosition = ptrPlayerPosition_x;
    playerPositionInfo = *(playerpos_t**)ptrPlayerPosition;

#ifdef _DEBUG
    std::cout << "PlayerPositionPtr: " << std::hex << (int)ptrPlayerPosition << std::endl;
    std::cout << "Player Position X: " << std::fixed << std::setprecision(0) << playerPositionInfo->pos_x << std::endl;
#endif

#define BUF_SIZE 256
    std::size_t fileSize = sizeof(playerpos_t);
    //const char* fileName = "pid_mmf";
    TCHAR szName[] = TEXT("pid_mmf");

    //HANDLE hMapFile;
    //LPVOID pBuf;

    auto hMapFile = CreateFileMapping(
        INVALID_HANDLE_VALUE,    // use paging file
        NULL,                    // default security
        PAGE_READWRITE,          // read/write access
        0,                       // maximum object size (high-order DWORD)
        fileSize,                // maximum object size (low-order DWORD)
        szName);                 // name of mapping object

    if (hMapFile == NULL)
    {
        _tprintf(TEXT("Could not create file mapping object (%d).\n"),
            GetLastError());
        return 1;
    }
    //pBuf = (LPVOID)MapViewOfFile(hMapFile,   // handle to map object
    //    FILE_MAP_ALL_ACCESS, // read/write permission
    //    0,
    //    0,
    //    256);

    //if (pBuf == NULL)
    //{
    //    _tprintf(TEXT("Could not map view of file (%d).\n"),
    //        GetLastError());
    //
    //    CloseHandle(hMapFile);
    //
    //    return 1;
    //}
    //create demo playerpos_t
    playerpos_t* test = new playerpos_t { 30000, 300, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 50000, "bbbbbbb", 6000, 'c' };

    playerpos_t* test2 = (playerpos_t*)MapViewOfFile(hMapFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0);

    *test2 = *playerPositionInfo;

    //CopyMemory((PVOID)pBuf, &test, sizeof(playerpos_t));
    //memcpy_s((LpBuf, sizeof(playerpos_t), &test, sizeof(playerpos_t));



    ////Memory mapped file: https://www.boost.org/doc/libs/1_59_0/doc/html/interprocess/sharedmemorybetweenprocesses.html#interprocess.sharedmemorybetweenprocesses.mapped_file
    //std::size_t fileSize = sizeof(playerpos_t);
    //const char* fileName = "pid_mmf";
    //
    //
    ////Setup the memory mapped file
    //boost::interprocess::file_mapping::remove(fileName);
    //std::filebuf fbuf;
    //fbuf.open(fileName, std::ios_base::in | std::ios_base::out | std::ios_base::trunc | std::ios_base::binary);
    //
    //if (fbuf.is_open()) {
    //    //Remove on exit
    //    struct file_remove
    //    {
    //        file_remove(const char* fileName)
    //            : fileName_(fileName) {}
    //        ~file_remove() { boost::interprocess::file_mapping::remove(fileName_); }
    //        const char* fileName_;
    //    } remover(fileName);
    //
    //    fbuf.pubseekoff(fileSize - 1, std::ios_base::beg);
    //    fbuf.sputc(0);
    //    
    //    //create file mapping
    //    boost::interprocess::file_mapping m_file(
    //        fileName,
    //        boost::interprocess::read_write
    //    );
    //
    //    //map the whole file
    //    boost::interprocess::mapped_region region(
    //        m_file,
    //        boost::interprocess::read_write
    //    );
    //
    //    //Get the address of the mapped region
    //    void* addr = region.get_address();
    //    std::size_t size = region.get_size();
    //
    //    //create demo playerpos_t
    //    playerpos_t test = { 30000, 300, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 50000, "bbbbbbb", 6000, 'c' };
    //    //memset it
    //    memcpy_s(addr, sizeof(playerpos_t), &test, sizeof(playerpos_t));
    //}

    //wait for user input
    while (true) {
        //break when user presses end
        if (GetAsyncKeyState(VK_END) & 1) {
            break;
        }
        Sleep(5);
    }
#ifdef _DEBUG
    if (f != 0) {
        fclose(f);
    }
    FreeConsole();
#endif

    UnmapViewOfFile(test);

    CloseHandle(hMapFile);

    FreeLibraryAndExitThread(hModule, 0);
    return 0;


    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        HANDLE ThreadHandle = CreateThread(0, NULL, (LPTHREAD_START_ROUTINE)MainThread, hModule, NULL, NULL);

        if (ThreadHandle != NULL) {
            CloseHandle(ThreadHandle);
        }
        break;
    }
    return TRUE;
}

