@echo off
echo ========================================
echo SimpleML Plugin Installer
echo ========================================
echo.

REM 获取脚本所在目录（去掉末尾的反斜杠）
set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

REM 保存当前工作目录
set "ORIGINAL_DIR=%CD%"

REM 智能检测myML文件夹位置
REM 情况1: 当前目录就是myML目录（包含components和core文件夹）
cd /d "%ORIGINAL_DIR%"
if exist "components" if exist "core" (
    set "MYML_SOURCE=%CD%"
    goto :found_myml
)

REM 情况2: myML在当前目录
if exist "myML" (
    set "MYML_SOURCE=%CD%\myML"
    goto :found_myml
)

REM 情况3: 脚本在 INSTALL_PACKAGE 目录中，myML在父目录
cd /d "%SCRIPT_DIR%\.."
if exist "myML" (
    set "MYML_SOURCE=%CD%\myML"
    goto :found_myml
)

REM 情况4: 脚本在 Install 目录中，myML在父目录
cd /d "%SCRIPT_DIR%\..\.."
if exist "myML" (
    set "MYML_SOURCE=%CD%\myML"
    goto :found_myml
)

REM 情况5: 从原始目录向上查找
cd /d "%ORIGINAL_DIR%"
:search_up
if exist "myML" (
    set "MYML_SOURCE=%CD%\myML"
    goto :found_myml
)
if exist "components" if exist "core" (
    set "MYML_SOURCE=%CD%"
    goto :found_myml
)
cd /d ".."
if "%CD%"=="%CD:\..%" goto :not_found
if not "%CD%"=="%ORIGINAL_DIR%" goto :search_up

:not_found
REM 如果都找不到，报错
echo ERROR: myML folder not found!
echo.
echo Script location: %SCRIPT_DIR%
echo Original directory: %ORIGINAL_DIR%
echo Current directory: %CD%
echo.
echo Please make sure:
echo 1. You are running from SimpleML_Package directory, OR
echo 2. The myML folder is accessible from the current location
echo.
echo You can also manually specify the myML path by editing this script.
echo.
pause
exit /b 1

:found_myml
echo Found myML folder at: %MYML_SOURCE%
echo.

REM 检查Python是否安装
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.7+ and try again
    pause
    exit /b 1
)

echo [1/4] Checking Python installation...
python --version
echo.

echo [2/4] Installing Python dependencies...
pip install scikit-learn numpy pandas joblib openpyxl
if errorlevel 1 (
    echo ERROR: Failed to install dependencies
    echo You can try installing manually: pip install -r Install\requirements.txt
    pause
    exit /b 1
)
echo.

REM 获取用户对象文件夹路径
set "USER_OBJECTS=%APPDATA%\Grasshopper\UserObjects"
set "SIMPLEML_PATH=%USER_OBJECTS%\SimpleML"

echo [3/4] Creating directories...
if not exist "%USER_OBJECTS%" mkdir "%USER_OBJECTS%"
if not exist "%SIMPLEML_PATH%" mkdir "%SIMPLEML_PATH%"
echo.

echo [4/4] Copying files...

REM 复制myML文件夹
echo Copying myML folder...
echo Source: %MYML_SOURCE%
echo Destination: %SIMPLEML_PATH%\myML
echo.

xcopy /E /I /Y "%MYML_SOURCE%" "%SIMPLEML_PATH%\myML"
if errorlevel 1 (
    echo ERROR: Failed to copy files
    echo Source: %MYML_SOURCE%
    echo Destination: %SIMPLEML_PATH%\myML
    pause
    exit /b 1
)

REM 复制用户对象文件（如果存在）
if exist "UserObjects\*.ghuser" (
    echo Copying user objects...
    copy /Y "UserObjects\*.ghuser" "%USER_OBJECTS%\"
)

echo.
echo ========================================
echo Installation completed successfully!
echo ========================================
echo.
echo SimpleML has been installed to:
echo %SIMPLEML_PATH%\myML
echo.
echo Please restart Grasshopper to use the plugin.
echo.
echo To use SimpleML in Grasshopper Python components, add:
echo   import sys
echo   sys.path.append(r'%SIMPLEML_PATH%\myML')
echo.
pause
