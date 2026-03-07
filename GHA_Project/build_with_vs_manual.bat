@echo off
REM 手动指定MSBuild路径的构建脚本
REM 如果自动检测失败，请编辑此文件，设置MSBUILD_PATH变量

echo ========================================
echo SimpleML GHA Builder (Manual MSBuild Path)
echo ========================================
echo.

REM ============================================
REM 请在这里设置您的MSBuild路径
REM ============================================
REM 取消注释下面一行，并设置正确的路径：
REM set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

REM 或者使用Developer Command Prompt中的msbuild（如果在PATH中）
set "MSBUILD_PATH=msbuild"

REM ============================================

if "%MSBUILD_PATH%"=="" (
    echo ERROR: MSBUILD_PATH is not set!
    echo.
    echo Please edit this file and set MSBUILD_PATH to your MSBuild location.
    echo.
    echo To find MSBuild location:
    echo 1. Run find_msbuild.bat
    echo 2. Or open Visual Studio Developer Command Prompt and type: where msbuild
    echo.
    pause
    exit /b 1
)

echo Using MSBuild at: %MSBUILD_PATH%
echo.

REM 验证MSBuild是否存在
if not "%MSBUILD_PATH%"=="msbuild" (
    if not exist "%MSBUILD_PATH%" (
        echo ERROR: MSBuild not found at: %MSBUILD_PATH%
        echo.
        echo Please check the path and update MSBUILD_PATH in this file.
        pause
        exit /b 1
    )
)

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
) else (
    echo ERROR: DLL not found after build
    echo Expected location: bin\Release\SimpleML.dll
    pause
    exit /b 1
)

pause
