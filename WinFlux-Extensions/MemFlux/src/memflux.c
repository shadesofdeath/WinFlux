// memflux.c - Memory cleaning library implementation
#include <windows.h>
#include <psapi.h>
#include "../include/memflux.h"

// Define NTSTATUS
typedef LONG NTSTATUS;
#define STATUS_SUCCESS ((NTSTATUS)0x00000000L)
#define STATUS_ACCESS_DENIED ((NTSTATUS)0xC0000022L)
#define STATUS_PRIVILEGE_NOT_HELD ((NTSTATUS)0xC0000061L)

// Define privilege names
#define SE_PROF_SINGLE_PROCESS_NAME TEXT("SeProfileSingleProcessPrivilege")
#define SE_INC_BASE_PRIORITY_NAME  TEXT("SeIncreaseBasePriorityPrivilege")
#define SE_INC_WORKING_SET_NAME    TEXT("SeIncreaseWorkingSetPrivilege")

// Define the SYSTEM_MEMORY_LIST_COMMAND enum for memory cleaning
typedef enum _SYSTEM_MEMORY_LIST_COMMAND {
    MemoryEmptyWorkingSets = 0,
    MemoryFlushModifiedList = 1,
    MemoryFlushStandbyList = 2,
    MemoryFlushPriorityZeroStandbyList = 3,
    MemoryFlushCombinedList = 4, // Windows 10 and above
    MemoryFlushModifiedFileCache = 5 // System modified file cache
} SYSTEM_MEMORY_LIST_COMMAND;

// Define SystemInformationClass
typedef enum _SYSTEM_INFORMATION_CLASS {
    SystemMemoryListInformation = 80,
    SystemFileCacheInformation = 21
} SYSTEM_INFORMATION_CLASS;

// Define NtSetSystemInformation function pointer
typedef NTSTATUS (NTAPI *PFN_NTSETSTEMINFORMATION)(
    SYSTEM_INFORMATION_CLASS SystemInformationClass,
    PVOID SystemInformation,
    ULONG SystemInformationLength
);

// System file cache structure (used for registry cache cleaning)
typedef struct _SYSTEM_FILECACHE_INFORMATION {
    SIZE_T CurrentSize;
    SIZE_T PeakSize;
    ULONG PageFaultCount;
    SIZE_T MinimumWorkingSet;
    SIZE_T MaximumWorkingSet;
    SIZE_T CurrentSizeIncludingTransitionInPages;
    SIZE_T PeakSizeIncludingTransitionInPages;
    ULONG TransitionRePurposeCount;
    ULONG Flags;
} SYSTEM_FILECACHE_INFORMATION, *PSYSTEM_FILECACHE_INFORMATION;

static PFN_NTSETSTEMINFORMATION NtSetSystemInformation = NULL;
static HANDLE hNtdll = NULL;

// Last error storage
static NTSTATUS g_LastNtStatus = STATUS_SUCCESS;

// Function to enable a specific privilege
static BOOL EnablePrivilege(LPCWSTR privilegeName) {
    HANDLE hToken;
    TOKEN_PRIVILEGES tp;
    LUID luid;
    BOOL result;

    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken))
        return FALSE;

    if (!LookupPrivilegeValue(NULL, privilegeName, &luid)) {
        CloseHandle(hToken);
        return FALSE;
    }

    tp.PrivilegeCount = 1;
    tp.Privileges[0].Luid = luid;
    tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    result = AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), NULL, NULL);
    
    CloseHandle(hToken);
    
    // AdjustTokenPrivileges can return TRUE even when it actually failed
    // So we need to check GetLastError as well
    return (result && GetLastError() == ERROR_SUCCESS);
}

// Adjust our process token privileges - using what MemReduct does
static BOOL SetupPrivileges() {
    BOOL result = FALSE;
    
    // Try to enable multiple privileges that might help with memory operations
    result |= EnablePrivilege(SE_INCREASE_QUOTA_NAME);       // Required for memory operations
    result |= EnablePrivilege(SE_PROF_SINGLE_PROCESS_NAME);  // For working with processes
    result |= EnablePrivilege(SE_DEBUG_NAME);                // For accessing system process
    result |= EnablePrivilege(SE_INC_BASE_PRIORITY_NAME);    // For priority adjustments
    result |= EnablePrivilege(SE_INC_WORKING_SET_NAME);      // For working set manipulation
    
    return result;
}

// Get the last NTSTATUS error
MEMFLUX_API NTSTATUS MemfluxGetLastNtStatus() {
    return g_LastNtStatus;
}

// Convert NTSTATUS to human readable error
MEMFLUX_API const char* MemfluxGetNtStatusString(NTSTATUS status) {
    switch(status) {
        case STATUS_SUCCESS:
            return "Operation completed successfully";
        case STATUS_ACCESS_DENIED:
            return "Access denied, administrative privileges required";
        case STATUS_PRIVILEGE_NOT_HELD:
            return "Required privilege not held, run as administrator";
        default:
            return "Unknown error occurred";
    }
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID lpReserved) {
    switch (reason) {
        case DLL_PROCESS_ATTACH:
            MemfluxInitialize();
            break;
        case DLL_PROCESS_DETACH:
            if (hNtdll) {
                FreeLibrary(hNtdll);
                hNtdll = NULL;
                NtSetSystemInformation = NULL;
            }
            break;
    }
    return TRUE;
}

MEMFLUX_API BOOL MemfluxInitialize() {
    if (NtSetSystemInformation == NULL) {
        hNtdll = LoadLibrary(L"ntdll.dll");
        if (hNtdll) {
            NtSetSystemInformation = (PFN_NTSETSTEMINFORMATION)GetProcAddress(hNtdll, "NtSetSystemInformation");
            if (NtSetSystemInformation) {
                // Try to enable the privileges we need for memory cleaning
                SetupPrivileges();
                g_LastNtStatus = STATUS_SUCCESS;
                return TRUE;
            }
        }
        g_LastNtStatus = STATUS_ACCESS_DENIED; // Generic error for initialization failure
        return FALSE;
    }
    return TRUE;
}

// Function to clean registry cache (taken from MemReduct approach)
static BOOL CleanRegistryCache() {
    SYSTEM_FILECACHE_INFORMATION sfc = {0};
    NTSTATUS status;
    
    // Set minimum working set to zero to clean registry cache
    sfc.MinimumWorkingSet = (ULONG_PTR)-1;
    sfc.MaximumWorkingSet = (ULONG_PTR)-1;
    
    status = NtSetSystemInformation(
        SystemFileCacheInformation,
        &sfc,
        sizeof(SYSTEM_FILECACHE_INFORMATION)
    );
    
    return (status == STATUS_SUCCESS);
}

// Function to clean modified file cache
static BOOL CleanModifiedFileCache() {
    SYSTEM_MEMORY_LIST_COMMAND command = MemoryFlushModifiedFileCache;
    NTSTATUS status;
    
    status = NtSetSystemInformation(
        SystemMemoryListInformation,
        &command,
        sizeof(SYSTEM_MEMORY_LIST_COMMAND)
    );
    
    return (status == STATUS_SUCCESS);
}

MEMFLUX_API BOOL MemfluxCleanMemory(MEMFLUX_CLEAN_TYPE CleanType) {
    SYSTEM_MEMORY_LIST_COMMAND command;
    NTSTATUS status;

    if (!MemfluxInitialize()) {
        return FALSE;
    }
    
    // Try to setup privileges again just to be sure
    SetupPrivileges();

    switch (CleanType) {
        case MemfluxWorkingSet:
            command = MemoryEmptyWorkingSets;
            break;
        case MemfluxStandbyList:
            command = MemoryFlushStandbyList;
            break;
        case MemfluxModifiedList:
            command = MemoryFlushModifiedList;
            break;
        case MemfluxCombinedList:
            command = MemoryFlushCombinedList;
            break;
        case MemfluxModifiedFileCache:
            return CleanModifiedFileCache();
        default:
            g_LastNtStatus = STATUS_ACCESS_DENIED; // Generic error
            return FALSE;
    }

    // Try to perform memory cleaning
    status = NtSetSystemInformation(
        SystemMemoryListInformation,
        &command,
        sizeof(SYSTEM_MEMORY_LIST_COMMAND)
    );
    
    // Store the status for later retrieval
    g_LastNtStatus = status;

    // If special case for registry cache - this may succeed when other methods fail
    if (status != STATUS_SUCCESS && CleanType == MemfluxCombinedList) {
        return CleanRegistryCache();
    }

    return (status == STATUS_SUCCESS);
}

MEMFLUX_API BOOL MemfluxCleanAllMemory() {
    // Initialize result tracking
    BOOL workingSetsResult = FALSE;
    BOOL systemWSResult = FALSE;
    BOOL modifiedListResult = FALSE;
    BOOL standbyListResult = FALSE;
    BOOL combinedListResult = FALSE;
    BOOL regCacheResult = FALSE;
    BOOL modifiedFileCacheResult = FALSE;
    BOOL anySuccess = FALSE;
    
    // Try to setup privileges first
    SetupPrivileges();
    
    // Clean working sets - most basic operation
    workingSetsResult = MemfluxCleanMemory(MemfluxWorkingSet);
    if (workingSetsResult) anySuccess = TRUE;
    
    // Clean system working set
    systemWSResult = MemfluxCleanSystemWorkingSet();
    if (systemWSResult) anySuccess = TRUE;
    
    // Clean modified page list
    modifiedListResult = MemfluxCleanMemory(MemfluxModifiedList);
    if (modifiedListResult) anySuccess = TRUE;
    
    // Clean standby list (file cache)
    standbyListResult = MemfluxCleanMemory(MemfluxStandbyList);
    if (standbyListResult) anySuccess = TRUE;
    
    // Try cleaning combined list (Windows 10+)
    combinedListResult = MemfluxCleanMemory(MemfluxCombinedList);
    if (combinedListResult) anySuccess = TRUE;
    
    // Try cleaning modified file cache (follow MemReduct latest version)
    modifiedFileCacheResult = MemfluxCleanMemory(MemfluxModifiedFileCache);
    if (modifiedFileCacheResult) anySuccess = TRUE;
    
    // As a last resort, try to clean registry cache directly
    regCacheResult = CleanRegistryCache();
    if (regCacheResult) anySuccess = TRUE;

    // Return success if any operation succeeded
    return anySuccess;
}

MEMFLUX_API BOOL MemfluxGetMemoryInfo(MEMFLUX_MEMORY_INFO* pMemInfo) {
    if (!pMemInfo) {
        return FALSE;
    }

    MEMORYSTATUSEX memStat;
    memStat.dwLength = sizeof(memStat);
    
    if (GlobalMemoryStatusEx(&memStat)) {
        pMemInfo->dwMemoryLoad = memStat.dwMemoryLoad;
        pMemInfo->ullTotalPhys = memStat.ullTotalPhys;
        pMemInfo->ullAvailPhys = memStat.ullAvailPhys;
        pMemInfo->ullTotalPageFile = memStat.ullTotalPageFile;
        pMemInfo->ullAvailPageFile = memStat.ullAvailPageFile;
        pMemInfo->ullTotalVirtual = memStat.ullTotalVirtual;
        pMemInfo->ullAvailVirtual = memStat.ullAvailVirtual;
        
        return TRUE;
    }
    
    return FALSE;
}

MEMFLUX_API BOOL MemfluxCleanSystemWorkingSet() {
    HANDLE hProcess;
    SIZE_T minWS = 0, maxWS = 0;
    BOOL result = FALSE;

    // Try to enable privileges first
    SetupPrivileges();

    // First try with system process
    hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_SET_QUOTA, FALSE, 4); // 4 is the System process
    if (hProcess) {
        // Set working set size to minimum for system process
        result = SetProcessWorkingSetSize(hProcess, (SIZE_T)-1, (SIZE_T)-1);
        CloseHandle(hProcess);
    }
    
    // If system process failed, try with our own process
    if (!result) {
        // Get our process handle
        hProcess = GetCurrentProcess();
        
        // Set working set size to minimum for our process
        result = SetProcessWorkingSetSize(hProcess, (SIZE_T)-1, (SIZE_T)-1);
    }
    
    if (!result) {
        g_LastNtStatus = STATUS_PRIVILEGE_NOT_HELD;
    } else {
        g_LastNtStatus = STATUS_SUCCESS;
    }
    
    return result;
}

MEMFLUX_API DWORD MemfluxGetMemoryLoad() {
    MEMORYSTATUSEX memStatus;
    memStatus.dwLength = sizeof(MEMORYSTATUSEX);
    
    if (GlobalMemoryStatusEx(&memStatus)) {
        return memStatus.dwMemoryLoad;
    }
    
    return 0;
}

// Clean working sets of all processes
MEMFLUX_API BOOL MemfluxCleanWorkingSets(void) {
    return MemfluxCleanMemory(MemfluxWorkingSet);
}

// Clean standby list
MEMFLUX_API BOOL MemfluxCleanStandbyList(void) {
    return MemfluxCleanMemory(MemfluxStandbyList);
}

// Clean modified page list
MEMFLUX_API BOOL MemfluxCleanModifiedPageList(void) {
    return MemfluxCleanMemory(MemfluxModifiedList);
}

// Cleanup library resources
MEMFLUX_API BOOL MemfluxCleanup(void) {
    if (hNtdll) {
        FreeLibrary(hNtdll);
        hNtdll = NULL;
    }
    
    NtSetSystemInformation = NULL;
    
    return TRUE;
} 