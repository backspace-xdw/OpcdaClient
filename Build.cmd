@echo off
echo === Building OPC DA Client ===
echo.

REM Check for Visual Studio 2019 MSBuild
set "MSBUILD_PATH="
set "VS2019_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2019"

REM Try different VS2019 editions
for %%E in (Enterprise Professional Community BuildTools) do (
    if exist "%VS2019_PATH%\%%E\MSBuild\Current\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=%VS2019_PATH%\%%E\MSBuild\Current\Bin\MSBuild.exe"
        echo Found MSBuild in VS2019 %%E
        goto :found
    )
)

REM Try MSBuild from .NET Framework
if exist "%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
    echo Found MSBuild in .NET Framework
    goto :found
)

echo ERROR: MSBuild not found!
echo Please install Visual Studio 2019 or Build Tools for Visual Studio
pause
exit /b 1

:found
echo Using MSBuild: %MSBUILD_PATH%
echo.

REM Restore NuGet packages
echo Restoring NuGet packages...
nuget restore OpcDaClient.sln 2>nul
if errorlevel 1 (
    echo Warning: NuGet restore failed. Continuing anyway...
)

echo.
echo Building Debug configuration...
"%MSBUILD_PATH%" OpcDaClient.sln /p:Configuration=Debug /p:Platform=x86 /v:minimal
if errorlevel 1 (
    echo ERROR: Debug build failed!
    pause
    exit /b 1
)

echo.
echo Building Release configuration...
"%MSBUILD_PATH%" OpcDaClient.sln /p:Configuration=Release /p:Platform=x86 /v:minimal
if errorlevel 1 (
    echo ERROR: Release build failed!
    pause
    exit /b 1
)

echo.
echo === Build completed successfully! ===
echo.
echo Output files:
echo Debug:   bin\x86\Debug\OpcDaClient.exe
echo Release: bin\x86\Release\OpcDaClient.exe
echo.
pause