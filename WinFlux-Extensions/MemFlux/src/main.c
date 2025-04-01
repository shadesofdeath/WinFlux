#define UNICODE
#define _UNICODE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <windows.h>
#include <psapi.h>
#include <shellapi.h>
#include "../include/memflux.h"

// Flag to indicate silent mode (no console window)
BOOL g_bSilentMode = FALSE;

// Function prototypes
void PrintMemoryInfo();
void CleanMemory(BOOL verbose);
void PrintUsage();
BOOL IsRunAsAdmin();
void PrintAdminWarning();

// Function to handle command line arguments
int HandleCommandLine(int argc, LPWSTR* argv);

// Function to get verbose status
BOOL GetVerboseStatus(int argc, LPWSTR* argv);

// Function to display memory information
void DisplayMemoryInfo(BOOL bVerbose);

// Function to display help information
void DisplayHelp(void);

// Entry point for Windows applications
int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow) {
    int argc;
    LPWSTR* argv = CommandLineToArgvW(GetCommandLineW(), &argc);
    
    // Check for -s or --silent flag first
    for (int i = 1; i < argc; i++) {
        if (wcscmp(argv[i], L"-s") == 0 || wcscmp(argv[i], L"--silent") == 0) {
            g_bSilentMode = TRUE;
            break;
        }
    }
    
    // If not silent mode, allocate console
    if (!g_bSilentMode) {
        AllocConsole();
        FILE* fpstdin = stdin;
        FILE* fpstdout = stdout;
        FILE* fpstderr = stderr;
        
        freopen_s(&fpstdin, "CONIN$", "r", stdin);
        freopen_s(&fpstdout, "CONOUT$", "w", stdout);
        freopen_s(&fpstderr, "CONOUT$", "w", stderr);
    }
    
    // Initialize MemFlux library
    if (!MemfluxInitialize()) {
        if (!g_bSilentMode) {
            wprintf(L"Failed to initialize MemFlux library.\n");
        }
        return 1;
    }
    
    // Handle command line arguments
    int result = HandleCommandLine(argc, argv);
    
    // Cleanup
    MemfluxCleanup();
    LocalFree(argv);
    
    return result;
}

// Entry point for console applications
int wmain(int argc, wchar_t* argv[]) {
    // Check for -s or --silent flag first
    for (int i = 1; i < argc; i++) {
        if (wcscmp(argv[i], L"-s") == 0 || wcscmp(argv[i], L"--silent") == 0) {
            g_bSilentMode = TRUE;
            break;
        }
    }
    
    // If silent mode, don't show console
    if (g_bSilentMode) {
        HWND hWnd = GetConsoleWindow();
        if (hWnd != NULL) {
            ShowWindow(hWnd, SW_HIDE);
        }
    }
    
    // Initialize MemFlux library
    if (!MemfluxInitialize()) {
        if (!g_bSilentMode) {
            wprintf(L"Failed to initialize MemFlux library.\n");
        }
        return 1;
    }
    
    // Handle command line arguments
    int result = HandleCommandLine(argc, argv);
    
    // Cleanup
    MemfluxCleanup();
    
    return result;
}

// Function to handle command line arguments
int HandleCommandLine(int argc, LPWSTR* argv) {
    BOOL bCleanMemory = FALSE;
    BOOL bCleanWorkingSet = FALSE;
    BOOL bCleanStandbyList = FALSE;
    BOOL bCleanModifiedList = FALSE;
    BOOL bVerbose = GetVerboseStatus(argc, argv);
    BOOL bQuiet = FALSE;
    int nPriority = 2; // Default priority (normal)
    
    // Parse command line arguments
    for (int i = 1; i < argc; i++) {
        if (wcscmp(argv[i], L"-h") == 0 || wcscmp(argv[i], L"--help") == 0) {
            if (!g_bSilentMode) {
                DisplayHelp();
            }
            return 0;
        } else if (wcscmp(argv[i], L"-c") == 0 || wcscmp(argv[i], L"--clean") == 0) {
            bCleanMemory = TRUE;
        } else if (wcscmp(argv[i], L"-w") == 0 || wcscmp(argv[i], L"--working") == 0) {
            bCleanWorkingSet = TRUE;
        } else if (wcscmp(argv[i], L"-b") == 0 || wcscmp(argv[i], L"--standby") == 0) {
            bCleanStandbyList = TRUE;
        } else if (wcscmp(argv[i], L"-m") == 0 || wcscmp(argv[i], L"--modified") == 0) {
            bCleanModifiedList = TRUE;
        } else if (wcscmp(argv[i], L"-q") == 0 || wcscmp(argv[i], L"--quiet") == 0) {
            bQuiet = TRUE;
        } else if (wcscmp(argv[i], L"-s") == 0 || wcscmp(argv[i], L"--silent") == 0) {
            // Already handled above
            continue;
        } else if (wcscmp(argv[i], L"-p") == 0 || wcscmp(argv[i], L"--priority") == 0) {
            if (i + 1 < argc) {
                nPriority = _wtoi(argv[++i]);
                if (nPriority < 0) nPriority = 0;
                if (nPriority > 5) nPriority = 5;
                
                // Set process priority
                HANDLE hProcess = GetCurrentProcess();
                DWORD dwPriorityClass = NORMAL_PRIORITY_CLASS;
                
                switch (nPriority) {
                    case 0: dwPriorityClass = IDLE_PRIORITY_CLASS; break;
                    case 1: dwPriorityClass = BELOW_NORMAL_PRIORITY_CLASS; break;
                    case 2: dwPriorityClass = NORMAL_PRIORITY_CLASS; break;
                    case 3: dwPriorityClass = ABOVE_NORMAL_PRIORITY_CLASS; break;
                    case 4: dwPriorityClass = HIGH_PRIORITY_CLASS; break;
                    case 5: dwPriorityClass = REALTIME_PRIORITY_CLASS; break;
                }
                
                SetPriorityClass(hProcess, dwPriorityClass);
            }
        }
    }
    
    // If no specific cleaning option is specified, but -c is used, clean all memory
    if (bCleanMemory && !bCleanWorkingSet && !bCleanStandbyList && !bCleanModifiedList) {
        bCleanWorkingSet = TRUE;
        bCleanStandbyList = TRUE;
        bCleanModifiedList = TRUE;
    }
    
    // If no cleaning option is specified at all, just display memory info
    if (!bCleanMemory && !bCleanWorkingSet && !bCleanStandbyList && !bCleanModifiedList) {
        if (!g_bSilentMode && !bQuiet) {
            DisplayMemoryInfo(bVerbose);
        }
        return 0;
    }
    
    // Display memory info before cleaning (if verbose and not quiet/silent)
    if (bVerbose && !bQuiet && !g_bSilentMode) {
        wprintf(L"Memory before cleaning:\n");
        DisplayMemoryInfo(FALSE);
        wprintf(L"\n");
    }
    
    // Clean memory
    BOOL bResult = TRUE;
    
    if (bCleanWorkingSet) {
        if (!bQuiet && !g_bSilentMode) wprintf(L"Cleaning working sets...\n");
        if (!MemfluxCleanWorkingSets()) {
            if (!bQuiet && !g_bSilentMode) {
                NTSTATUS status = MemfluxGetLastNtStatus();
                const char* errorMsg = MemfluxGetNtStatusString(status);
                wprintf(L"Failed to clean working sets. Error: %hs (0x%08X)\n", errorMsg, status);
            }
            bResult = FALSE;
        }
    }
    
    if (bCleanStandbyList) {
        if (!bQuiet && !g_bSilentMode) wprintf(L"Cleaning standby lists...\n");
        if (!MemfluxCleanStandbyList()) {
            if (!bQuiet && !g_bSilentMode) {
                NTSTATUS status = MemfluxGetLastNtStatus();
                const char* errorMsg = MemfluxGetNtStatusString(status);
                wprintf(L"Failed to clean standby lists. Error: %hs (0x%08X)\n", errorMsg, status);
            }
            bResult = FALSE;
        }
    }
    
    if (bCleanModifiedList) {
        if (!bQuiet && !g_bSilentMode) wprintf(L"Cleaning modified page list...\n");
        if (!MemfluxCleanModifiedPageList()) {
            if (!bQuiet && !g_bSilentMode) {
                NTSTATUS status = MemfluxGetLastNtStatus();
                const char* errorMsg = MemfluxGetNtStatusString(status);
                wprintf(L"Failed to clean modified page list. Error: %hs (0x%08X)\n", errorMsg, status);
            }
            bResult = FALSE;
        }
    }
    
    // Display memory info after cleaning (if verbose and not quiet/silent)
    if (bVerbose && !bQuiet && !g_bSilentMode) {
        wprintf(L"\nMemory after cleaning:\n");
        DisplayMemoryInfo(FALSE);
    } else if (!bQuiet && !bVerbose && !g_bSilentMode) {
        // Just display a simple message if not verbose and not quiet
        if (bResult) {
            wprintf(L"Memory cleaning completed successfully.\n");
        } else {
            wprintf(L"Memory cleaning completed with some errors.\n");
        }
    }
    
    return bResult ? 0 : 1;
}

// Function to get verbose status
BOOL GetVerboseStatus(int argc, LPWSTR* argv) {
    for (int i = 1; i < argc; i++) {
        if (wcscmp(argv[i], L"-v") == 0 || wcscmp(argv[i], L"--verbose") == 0) {
            return TRUE;
        }
    }
    
    return FALSE;
}

// Function to display memory information
void DisplayMemoryInfo(BOOL bVerbose) {
    MEMFLUX_MEMORY_INFO memInfo;
    
    if (MemfluxGetMemoryInfo(&memInfo)) {
        // Display general memory info
        wprintf(L"Memory load: %d%%\n", memInfo.dwMemoryLoad);
        
        // Convert to MB for easier reading
        double totalPhysMB = memInfo.ullTotalPhys / (1024.0 * 1024.0);
        double availPhysMB = memInfo.ullAvailPhys / (1024.0 * 1024.0);
        double totalPageMB = memInfo.ullTotalPageFile / (1024.0 * 1024.0);
        double availPageMB = memInfo.ullAvailPageFile / (1024.0 * 1024.0);
        double totalVirtMB = memInfo.ullTotalVirtual / (1024.0 * 1024.0);
        double availVirtMB = memInfo.ullAvailVirtual / (1024.0 * 1024.0);
        
        // Display physical memory info
        wprintf(L"Physical memory: %.2f MB / %.2f MB (%.2f MB free)\n",
            totalPhysMB - availPhysMB, totalPhysMB, availPhysMB);
        
        // Display page file info
        wprintf(L"Page file: %.2f MB / %.2f MB (%.2f MB free)\n",
            totalPageMB - availPageMB, totalPageMB, availPageMB);
        
        // Display virtual memory info if verbose
        if (bVerbose) {
            wprintf(L"Virtual memory: %.2f MB / %.2f MB (%.2f MB free)\n",
                totalVirtMB - availVirtMB, totalVirtMB, availVirtMB);
        }
    } else {
        wprintf(L"Failed to get memory information.\n");
    }
}

// Function to display help information
void DisplayHelp(void) {
    wprintf(L"MemFlux - Advanced Memory Cleaner\n");
    wprintf(L"-----------------------------\n\n");
    wprintf(L"Usage: MemFlux.exe [options]\n\n");
    wprintf(L"Options:\n");
    wprintf(L"  -h, --help       Show this help message\n");
    wprintf(L"  -c, --clean      Clean all memory\n");
    wprintf(L"  -w, --working    Clean working sets\n");
    wprintf(L"  -b, --standby    Clean standby list\n");
    wprintf(L"  -m, --modified   Clean modified page list\n");
    wprintf(L"  -p, --priority   Set process priority (0-5, default: 2)\n");
    wprintf(L"                   0: Idle, 1: Below normal, 2: Normal,\n");
    wprintf(L"                   3: Above normal, 4: High, 5: Realtime\n");
    wprintf(L"  -v, --verbose    Show detailed information\n");
    wprintf(L"  -q, --quiet      Quiet mode (don't show output)\n");
    wprintf(L"  -s, --silent     Silent mode (no console window)\n\n");
    wprintf(L"Examples:\n");
    wprintf(L"  MemFlux.exe -c                   Clean all memory\n");
    wprintf(L"  MemFlux.exe -w -b -m             Clean all memory (same as -c)\n");
    wprintf(L"  MemFlux.exe -c -v                Clean memory and show details\n");
    wprintf(L"  MemFlux.exe -w -p 4              Clean working sets with high priority\n");
    wprintf(L"  MemFlux.exe -c -s                Clean memory in silent mode\n");
    wprintf(L"  MemFlux.exe                      Show memory information\n");
}

void PrintMemoryInfo() {
    MEMFLUX_MEMORY_INFO memInfo;
    
    printf("======== MemFlux Memory Information ========\n");
    
    if (MemfluxGetMemoryInfo(&memInfo)) {
        printf("Memory Load: %d%%\n", memInfo.dwMemoryLoad);
        printf("Physical Memory: %.2f GB / %.2f GB (%.2f GB free)\n",
               (memInfo.ullTotalPhys - memInfo.ullAvailPhys) / (1024.0 * 1024.0 * 1024.0),
               memInfo.ullTotalPhys / (1024.0 * 1024.0 * 1024.0),
               memInfo.ullAvailPhys / (1024.0 * 1024.0 * 1024.0));
               
        printf("Page File: %.2f GB / %.2f GB (%.2f GB free)\n",
               (memInfo.ullTotalPageFile - memInfo.ullAvailPageFile) / (1024.0 * 1024.0 * 1024.0),
               memInfo.ullTotalPageFile / (1024.0 * 1024.0 * 1024.0),
               memInfo.ullAvailPageFile / (1024.0 * 1024.0 * 1024.0));
               
        printf("Virtual Memory: %.2f GB / %.2f GB (%.2f GB free)\n",
               (memInfo.ullTotalVirtual - memInfo.ullAvailVirtual) / (1024.0 * 1024.0 * 1024.0),
               memInfo.ullTotalVirtual / (1024.0 * 1024.0 * 1024.0),
               memInfo.ullAvailVirtual / (1024.0 * 1024.0 * 1024.0));
    } else {
        printf("Failed to get memory information.\n");
    }
    
    printf("===========================================\n");
}

void CleanMemory(BOOL verbose) {
    MEMFLUX_MEMORY_INFO memInfoBefore, memInfoAfter;
    DWORD memLoadBefore, memLoadAfter;
    
    if (verbose) {
        // Get memory info before cleaning
        if (MemfluxGetMemoryInfo(&memInfoBefore)) {
            memLoadBefore = memInfoBefore.dwMemoryLoad;
            printf("Memory before cleaning: %d%%\n", memLoadBefore);
        }
    }
    
    // Clean all memory areas
    if (MemfluxCleanAllMemory()) {
        if (verbose) {
            // Wait a bit for the system to update memory information
            Sleep(500);
            
            // Get memory info after cleaning
            if (MemfluxGetMemoryInfo(&memInfoAfter)) {
                memLoadAfter = memInfoAfter.dwMemoryLoad;
                printf("Memory after cleaning: %d%%\n", memLoadAfter);
                
                if (memLoadBefore > memLoadAfter) {
                    printf("Memory freed: %d%%\n", memLoadBefore - memLoadAfter);
                } else {
                    printf("Memory usage unchanged or increased slightly.\n");
                }
                
                // Show detailed memory information
                printf("Physical memory free: %.2f GB\n", 
                       memInfoAfter.ullAvailPhys / (1024.0 * 1024.0 * 1024.0));
            }
            
            printf("Memory cleaning completed.\n");
        }
    } else {
        // Get more detailed error information
        NTSTATUS status = MemfluxGetLastNtStatus();
        const char* errorMsg = MemfluxGetNtStatusString(status);
        
        printf("Failed to clean memory. Error: %s (0x%08X)\n", errorMsg, status);
        
        if (!IsRunAsAdmin()) {
            printf("Try running as administrator for full memory cleaning capabilities.\n");
        }
    }
}

void PrintUsage() {
    printf("MemFlux - Advanced RAM Cleaner\n");
    printf("Version 1.0\n\n");
    printf("Usage: MemFlux.exe [options]\n\n");
    printf("Options:\n");
    printf("  -h, --help                 Show this help message\n");
    printf("  -i, --info                 Display memory information\n");
    printf("  -c, --clean                Clean all memory areas with detailed output\n");
    printf("  -s, --clean-silent         Clean all memory areas without output\n");
    printf("  -w, --working-set          Clean working sets only\n");
    printf("  -sys, --system-working-set Clean system working set only\n");
    printf("  -sl, --standby-list        Clean standby list only\n");
    printf("  -ml, --modified-list       Clean modified list only\n");
    printf("  -mfc, --modified-file-cache Clean modified file cache only\n");
    printf("  -cl, --combined-list       Clean combined list only (Windows 10+)\n");
    printf("\nExamples:\n");
    printf("  MemFlux.exe -i             Display memory information\n");
    printf("  MemFlux.exe -c             Clean all memory areas\n");
    printf("  MemFlux.exe -w -sl         Clean working sets and standby list\n");
    printf("\nNote: Run as administrator for full memory cleaning capabilities.\n");
}

BOOL IsRunAsAdmin() {
    BOOL fIsRunAsAdmin = FALSE;
    DWORD dwError = ERROR_SUCCESS;
    PSID pAdministratorsGroup = NULL;

    // Allocate and initialize a SID of the administrators group.
    SID_IDENTIFIER_AUTHORITY NtAuthority = SECURITY_NT_AUTHORITY;
    if (!AllocateAndInitializeSid(
        &NtAuthority, 
        2, 
        SECURITY_BUILTIN_DOMAIN_RID, 
        DOMAIN_ALIAS_RID_ADMINS, 
        0, 0, 0, 0, 0, 0, 
        &pAdministratorsGroup)) {
        dwError = GetLastError();
        goto Cleanup;
    }

    // Determine whether the SID of administrators group is enabled in 
    // the primary access token of the process.
    if (!CheckTokenMembership(NULL, pAdministratorsGroup, &fIsRunAsAdmin)) {
        dwError = GetLastError();
        goto Cleanup;
    }

Cleanup:
    // Free SID if allocated.
    if (pAdministratorsGroup) {
        FreeSid(pAdministratorsGroup);
        pAdministratorsGroup = NULL;
    }

    return fIsRunAsAdmin;
}

void PrintAdminWarning() {
    printf("WARNING: Not running as Administrator. Some memory cleaning operations may fail.\n");
    printf("         For best results, run MemFlux.exe as Administrator.\n\n");
} 