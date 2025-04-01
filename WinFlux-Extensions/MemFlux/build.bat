@echo off
setlocal enabledelayedexpansion

echo MemFlux Build Script
echo -------------------

:: Check if Visual Studio 2022 is installed and get the path
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo ERROR: Visual Studio 2022 not found.
    echo Please install Visual Studio 2022 with C++ development workload.
    exit /b 1
)

:: Find Visual Studio 2022 installation path
for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
    set VS_PATH=%%i
)

if not defined VS_PATH (
    echo ERROR: Visual Studio 2022 with C++ tools not found.
    exit /b 1
)

:: Set up Visual Studio environment
echo Setting up Visual Studio 2022 environment...
call "%VS_PATH%\VC\Auxiliary\Build\vcvarsall.bat" x64

:: Create directories if they don't exist
if not exist bin mkdir bin
if not exist obj mkdir obj

:: Clean up old build files first
echo Cleaning old build files...
if exist obj\*.obj del /Q obj\*.obj
if exist obj\*.res del /Q obj\*.res
if exist bin\*.dll del /Q bin\*.dll
if exist bin\*.exe del /Q bin\*.exe
if exist bin\*.lib del /Q bin\*.lib
if exist bin\*.exp del /Q bin\*.exp

:: Compile resources
echo Compiling resources...
rc /nologo /fo obj\simple_res.res src\simple_resource.rc
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to compile resources.
    exit /b %ERRORLEVEL%
)

:: Compile the DLL
echo Compiling MemFlux DLL...
cl /nologo /W3 /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_USRDLL" /D "MEMFLUX_EXPORTS" /D "_WINDLL" /D "_UNICODE" /D "UNICODE" /Gm- /EHsc /MT /GS /Fo"obj\\" /Fd"obj\\" /Gd /TC /analyze- /FC src\memflux.c /link /OUT:"bin\memflux.dll" /INCREMENTAL:NO /NOLOGO kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /SUBSYSTEM:WINDOWS /DYNAMICBASE /NXCOMPAT /IMPLIB:"bin\memflux.lib" /MACHINE:X64 /DLL

:: Check for DLL compilation errors
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to compile DLL.
    exit /b %ERRORLEVEL%
)

:: Compile the MemFlux EXE with Windows subsystem
echo Compiling MemFlux as Windows application...
cl /nologo /W3 /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_UNICODE" /D "UNICODE" /Gm- /EHsc /MT /GS /Fo"obj\\" /Fd"obj\\" /Gd /TC /analyze- /FC src\main.c /link /OUT:"bin\MemFlux.exe" /INCREMENTAL:NO /NOLOGO kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib psapi.lib shlwapi.lib bin\memflux.lib obj\simple_res.res /SUBSYSTEM:WINDOWS /DYNAMICBASE /NXCOMPAT /MACHINE:X64

:: Check for Windows EXE compilation errors
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to compile Windows EXE.
    exit /b %ERRORLEVEL%
)

echo Build completed successfully!
echo The compiled files are in the 'bin' directory.

endlocal 