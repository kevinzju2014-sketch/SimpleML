@echo off
echo ========================================
echo SimpleML GHA Builder
echo ========================================
echo.

REM 查找MSBuild
set "MSBUILD_PATH="

REM 方法1: 使用Visual Studio 2022的MSBuild
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

REM 方法2: 使用Visual Studio 2019的MSBuild
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

REM 方法3: 使用Build Tools
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

REM 方法4: 尝试.NET SDK的dotnet命令（需要验证SDK是否真的可用）
where dotnet >nul 2>&1
if not errorlevel 1 (
    dotnet --version >nul 2>&1
    if not errorlevel 1 (
        REM 验证dotnet build命令是否可用
        dotnet build --help >nul 2>&1
        if not errorlevel 1 (
            echo Using .NET SDK...
            echo [1/3] Restoring NuGet packages...
            dotnet restore
            if errorlevel 1 (
                echo ERROR: Failed to restore packages
                pause
                exit /b 1
            )
            echo.
            echo [2/3] Building project...
            dotnet build -c Release
            if errorlevel 1 (
                echo ERROR: Build failed
                pause
                exit /b 1
            )
            echo.
            goto :copy_gha
        )
    )
)

REM 如果都找不到，报错
echo ERROR: MSBuild not found!
echo.
echo Please install one of the following:
echo 1. Visual Studio 2019/2022 (Community, Professional, or Enterprise)
echo 2. Visual Studio Build Tools 2022
echo 3. .NET SDK 6.0 or later
echo.
echo Download links:
echo - Visual Studio: https://visualstudio.microsoft.com/
echo - Build Tools: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
echo - .NET SDK: https://dotnet.microsoft.com/download
echo.
pause
exit /b 1

:found_msbuild
echo Found MSBuild at: %MSBUILD_PATH%
echo.

echo [1/3] Restoring NuGet packages...
"%MSBUILD_PATH%" /t:Restore SimpleML.csproj
if errorlevel 1 (
    echo WARNING: NuGet restore failed, continuing anyway...
)
echo.

echo [2/3] Building project...
"%MSBUILD_PATH%" /p:Configuration=Release /p:Platform=AnyCPU SimpleML.csproj
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

:copy_gha
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
    echo To install:
    echo 1. Copy SimpleML.gha to: %%APPDATA%%\Grasshopper\Libraries\
    echo 2. Copy myML folder to: %%APPDATA%%\Grasshopper\UserObjects\SimpleML\myML
    echo 3. Restart Grasshopper
    echo.
) else (
    echo ERROR: DLL not found after build
    echo Expected location: bin\Release\SimpleML.dll
    echo.
    echo Please check:
    echo 1. Build completed without errors
    echo 2. Output directory exists
    echo 3. File permissions
    pause
    exit /b 1
)

pause
