# MemFlux - Advanced RAM Cleaner

MemFlux is a powerful command-line memory cleaning application for Windows that provides advanced memory cleaning capabilities similar to MemReduct but with a cleaner codebase and CLI interface.

## Features

- Clean various memory areas in Windows:
  - Working sets
  - System working set
  - Standby list
  - Modified list
  - Combined list (Windows 10+)
- Display detailed memory information
- Run silently or with detailed output
- Lightweight and efficient

## Usage

MemFlux is a command-line application with various options:

```
MemFlux.exe [options]

Options:
  -h, --help                 Show help message
  -i, --info                 Display memory information
  -c, --clean                Clean all memory areas with detailed output
  -s, --clean-silent         Clean all memory areas without output
  -w, --working-set          Clean working sets only
  -sys, --system-working-set Clean system working set only
  -sl, --standby-list        Clean standby list only
  -ml, --modified-list       Clean modified list only
  -cl, --combined-list       Clean combined list only (Windows 10+)
```

### Examples

```
MemFlux.exe -i             # Display memory information
MemFlux.exe -c             # Clean all memory areas
MemFlux.exe -w -sl         # Clean working sets and standby list
```

## Important Note

**For full functionality, MemFlux must be run with administrator privileges.**

Many memory cleaning operations require administrator rights to work properly. If you run MemFlux without administrator privileges, some operations may fail or have limited effectiveness.

To run with administrator privileges:
1. Right-click on MemFlux.exe
2. Select "Run as administrator"

Or from an elevated command prompt/PowerShell:
```
.\MemFlux.exe [options]
```

## Building from Source

MemFlux can be built using Visual Studio 2022 with the included build script:

```
.\build.bat
```

This will compile both the MemFlux DLL and the console application, placing the output files in the `bin` directory.

## Architecture

MemFlux consists of two main components:

1. **memflux.dll** - Core library that handles memory cleaning operations using Windows Native API
2. **MemFlux.exe** - Command-line interface that uses the DLL to provide user interface and functionality

## License

This software is provided as-is with no warranty. 