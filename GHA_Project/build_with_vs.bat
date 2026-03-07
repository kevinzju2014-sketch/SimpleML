@echo off
REM 使用Visual Studio Developer Command Prompt构建
REM 这个脚本需要在Visual Studio Developer Command Prompt中运行

echo ========================================
echo SimpleML GHA Builder (Visual Studio)
echo ========================================
echo.

echo [1/3] Restoring NuGet packages...
msbuild /t:Restore SimpleML.csproj
if errorlevel 1 (
    echo WARNING: NuGet restore failed, continuing anyway...
)
echo.

echo [2/3] Building project...
msbuild /p:Configuration=Release /p:Platform=AnyCPU SimpleML.csproj
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

echo [3/3] Copying GHA file...
if exist "bin\Release\SimpleML.dll" (
    copy /Y "bin\Release\SimpleML.dll" "bin\Release\SimpleML.gha" >nul
    echo.
    echo ========================================
    echo Build completed successfully!
    echo ========================================
    echo.
    echo GHA file location: bin\Release\SimpleML.gha
    echo.
) else (
    echo ERROR: DLL not found after build
    pause
    exit /b 1
)

pause
