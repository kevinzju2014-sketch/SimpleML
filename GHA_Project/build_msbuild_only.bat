@echo off
REM 仅使用MSBuild构建（不尝试dotnet命令）
echo ========================================
echo SimpleML GHA Builder (MSBuild Only)
echo ========================================
echo.

REM 查找MSBuild
set "MSBUILD_PATH="

REM Visual Studio 2022
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

REM Visual Studio 2019
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

REM Build Tools 2022
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

REM Build Tools 2019
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    goto :found_msbuild
)

REM 尝试从PATH查找
where msbuild >nul 2>&1
if not errorlevel 1 (
    for /f "delims=" %%i in ('where msbuild') do (
        set "MSBUILD_PATH=%%i"
        goto :found_msbuild
    )
)

REM 搜索其他可能的路径（Visual Studio可能安装在其他位置）
for /d %%d in ("C:\Program Files\Microsoft Visual Studio\*") do (
    if exist "%%d\MSBuild\Current\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=%%d\MSBuild\Current\Bin\MSBuild.exe"
        goto :found_msbuild
    )
    if exist "%%d\MSBuild\17.0\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=%%d\MSBuild\17.0\Bin\MSBuild.exe"
        goto :found_msbuild
    )
)

for /d %%d in ("C:\Program Files (x86)\Microsoft Visual Studio\*") do (
    if exist "%%d\MSBuild\Current\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=%%d\MSBuild\Current\Bin\MSBuild.exe"
        goto :found_msbuild
    )
    if exist "%%d\MSBuild\17.0\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=%%d\MSBuild\17.0\Bin\MSBuild.exe"
        goto :found_msbuild
    )
    if exist "%%d\MSBuild\16.0\Bin\MSBuild.exe" (
        set "MSBUILD_PATH=%%d\MSBuild\16.0\Bin\MSBuild.exe"
        goto :found_msbuild
    )
)

REM 如果都找不到，报错
echo ERROR: MSBuild not found!
echo.
echo Please install one of the following:
echo 1. Visual Studio 2019/2022 (Community, Professional, or Enterprise)
echo 2. Visual Studio Build Tools 2019/2022
echo.
echo Download links:
echo - Visual Studio Community (Free): https://visualstudio.microsoft.com/vs/community/
echo - Build Tools: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
echo.
echo Installation tips:
echo - When installing Visual Studio, select ".NET desktop development" workload
echo - When installing Build Tools, select ".NET desktop build tools"
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
"%MSBUILD_PATH%" /p:Configuration=Release /p:Platform=AnyCPU /v:minimal SimpleML.csproj
if errorlevel 1 (
    echo.
    echo ERROR: Build failed
    echo Please check the error messages above
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
    echo 2. Output directory exists: bin\Release\
    echo 3. File permissions
    echo.
    echo You can also try building in Visual Studio:
    echo 1. Open SimpleML.csproj in Visual Studio
    echo 2. Press Ctrl+Shift+B to build
    echo 3. Rename bin\Release\SimpleML.dll to SimpleML.gha
    pause
    exit /b 1
)

pause
