// memflux.h - Memory cleaning library header
#ifndef MEMFLUX_H
#define MEMFLUX_H

#ifdef __cplusplus
extern "C" {
#endif

#ifdef MEMFLUX_EXPORTS
#define MEMFLUX_API __declspec(dllexport)
#else
#define MEMFLUX_API __declspec(dllimport)
#endif

#include <windows.h>

// NTSTATUS definition
typedef LONG NTSTATUS;

// Memory information structure
typedef struct _MEMFLUX_MEMORY_INFO {
    DWORD dwMemoryLoad;  // Memory load percentage (0-100)
    DWORDLONG ullTotalPhys;
    DWORDLONG ullAvailPhys;
    DWORDLONG ullTotalPageFile;
    DWORDLONG ullAvailPageFile;
    DWORDLONG ullTotalVirtual;
    DWORDLONG ullAvailVirtual;
} MEMFLUX_MEMORY_INFO;

// Memory cleaning types
typedef enum _MEMFLUX_CLEAN_TYPE {
    MemfluxWorkingSet      = 0x01,
    MemfluxStandbyList     = 0x02,
    MemfluxModifiedList    = 0x04,
    MemfluxModifiedFileCache = 0x08,
    MemfluxCombinedList    = 0x10,
    MemfluxAllLists        = 0xFF
} MEMFLUX_CLEAN_TYPE;

// Function prototypes
MEMFLUX_API BOOL MemfluxInitialize(void);
MEMFLUX_API BOOL MemfluxCleanup(void);
MEMFLUX_API BOOL MemfluxCleanMemory(MEMFLUX_CLEAN_TYPE CleanType);
MEMFLUX_API BOOL MemfluxCleanSystemWorkingSet(void);
MEMFLUX_API BOOL MemfluxGetMemoryInfo(MEMFLUX_MEMORY_INFO* pMemInfo);

// New functions for better memory cleaning
MEMFLUX_API BOOL MemfluxCleanWorkingSets(void);
MEMFLUX_API BOOL MemfluxCleanStandbyList(void);
MEMFLUX_API BOOL MemfluxCleanModifiedPageList(void);
MEMFLUX_API BOOL MemfluxCleanAllMemory(void);

// Utility functions
MEMFLUX_API NTSTATUS MemfluxGetLastNtStatus(void);
MEMFLUX_API const char* MemfluxGetNtStatusString(NTSTATUS Status);

#ifdef __cplusplus
}
#endif

#endif // MEMFLUX_H 