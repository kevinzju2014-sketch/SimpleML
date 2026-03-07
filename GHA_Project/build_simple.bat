@echo off
REM 最简单的构建脚本 - 使用Visual Studio Developer Command Prompt中的msbuild

echo ========================================
echo SimpleML GHA Builder (Simple)
echo ========================================
echo.
echo This script uses msbuild from PATH.
echo Make sure you run this from Visual Studio Developer Command Prompt,
echo or that msbuild is in your PATH.
echo.

REM 检查msbuild是否可用
where msbuild >nul 2>&1
if errorlevel 1 (
    echo ERROR: msbuild not found in PATH!
    echo.
    echo Please:
    echo 1. Open Visual Studio Developer Command Prompt, OR
    echo 2. Run this script from Visual Studio Developer Command Prompt
    echo.
    echo To open Developer Command Prompt:
    echo Start Menu -^> Visual Studio -^> Developer Command Prompt
    echo.
    pause
    exit /b 1
)

echo Found msbuild, building...
echo.

echo [1/3] Restoring packages...
msbuild /t:Restore SimpleML.csproj
if errorlevel 1 (
    echo WARNING: Restore failed, continuing...
)
echo.

echo [2/3] Building project...
msbuild /p:Configuration=Release /p:Platform=AnyCPU /v:minimal SimpleML.csproj
if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo Please check the error messages above.
    pause
    exit /b 1
)
echo.

echo [3/3] Creating GHA file...
if exist "bin\Release\SimpleML.dll" (
    copy /Y "bin\Release\SimpleML.dll" "bin\Release\SimpleML.gha" >nul
    echo.
    echo ========================================
    echo Build completed successfully!
    echo ========================================
    echo.
    echo GHA file: bin\Release\SimpleML.gha
    echo.
) else (
    echo ERROR: DLL not found!
    pause
    exit /b 1
)

pause
