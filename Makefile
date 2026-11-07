SHELL := cmd.exe

.SILENT:

.PHONY: all restore milim nava copy-dlls terminal no-terminal clean

all: terminal no-terminal
	@echo ========================================
	@echo [Nava] All builds succeeded!
	@echo Terminal:    injector\nava.exe
	@echo No Terminal: injector\nava_no_terminal.exe
	@echo ========================================

restore:
	@echo [Nava] Restoring NuGet packages...
	@if not exist Milim\nuget.exe powershell -Command "Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile Milim\nuget.exe"
	@Milim\nuget.exe restore Milim\Milim.sln -Verbosity quiet

milim: restore
	@echo [Nava] Building Milim.dll (C#)...
	@dotnet build Milim\Milim.csproj -c Debug -p:Platform=x64 --nologo -v q

nava:
	@echo [Nava] Building nava.dll (Go)...
	@cd Nava && go build -o nava.dll --buildmode=c-shared

copy-dlls: milim nava
	@echo [Nava] Copying DLLs to injector directory...
	@if exist Milim\bin\x64\Debug\Milim.dll copy /Y Milim\bin\x64\Debug\Milim.dll injector\Milim.dll >nul
	@if exist Nava\nava.dll copy /Y Nava\nava.dll injector\nava.dll >nul

terminal: copy-dlls
	@echo [Nava] Building Nava (Terminal)...
	@cd injector && go build -o nava.exe

no-terminal: copy-dlls
	@echo [Nava] Building Nava (No Terminal)...
	@cd injector && go build -ldflags="-H windowsgui" -o nava_no_terminal.exe

clean:
	@echo [Nava] Cleaning artifacts...
	@if exist Milim\bin rmdir /S /Q Milim\bin
	@if exist Milim\obj rmdir /S /Q Milim\obj
	@if exist Milim\packages rmdir /S /Q Milim\packages
	@if exist Milim\nuget.exe del /F /Q Milim\nuget.exe
	@if exist Nava\nava.dll del /F /Q Nava\nava.dll
	@if exist Nava\nava.h del /F /Q Nava\nava.h
	@if exist injector\Milim.dll del /F /Q injector\Milim.dll
	@if exist injector\nava.dll del /F /Q injector\nava.dll
	@if exist injector\nava.exe del /F /Q injector\nava.exe
	@if exist injector\nava_no_terminal.exe del /F /Q injector\nava_no_terminal.exe
	@echo [Nava] Clean complete.
