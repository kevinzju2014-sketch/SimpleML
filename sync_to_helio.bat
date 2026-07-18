@echo off
REM Sync this SimpleML repo into D:\Helio\250928_机器学习课程
REM Run on your Windows Rhino PC (Rhino can be closed).
setlocal EnableExtensions EnableDelayedExpansion

set "TARGET=D:\Helio\250928_机器学习课程"
set "BRANCH=cursor/optimize-usability-crossplatform-abc6"
set "REPO=https://github.com/kevinzju2014-sketch/SimpleML.git"

echo === Sync SimpleML -^> %TARGET% ===

if not exist "D:\Helio" mkdir "D:\Helio"

where git >nul 2>nul
if errorlevel 1 (
  echo ERROR: git not found. Install Git for Windows first.
  pause
  exit /b 1
)

if exist "%TARGET%\.git" (
  echo Updating existing git repo...
  pushd "%TARGET%"
  git fetch origin
  git checkout %BRANCH%
  git pull origin %BRANCH%
  popd
) else (
  if exist "%TARGET%" (
    echo Backing up old folder to %TARGET%_backup_old ...
    if exist "%TARGET%_backup_old" rmdir /s /q "%TARGET%_backup_old"
    move "%TARGET%" "%TARGET%_backup_old"
  )
  echo Cloning %REPO% ...
  git clone -b %BRANCH% "%REPO%" "%TARGET%"
)

echo.
echo Cleaning leftover install/history folders if present...
if exist "%TARGET%\INSTALL_PACKAGE" rmdir /s /q "%TARGET%\INSTALL_PACKAGE"
if exist "%TARGET%\releases" rmdir /s /q "%TARGET%\releases"
if exist "%TARGET%\dist" rmdir /s /q "%TARGET%\dist"
if exist "%TARGET%\yak" rmdir /s /q "%TARGET%\yak"
if exist "%TARGET%\myML\INSTALL_PACKAGE" rmdir /s /q "%TARGET%\myML\INSTALL_PACKAGE"

echo.
echo DONE.
echo Project root: %TARGET%
echo Next: open this folder in Cursor/VS, or build GHA_Project.
echo   dotnet build "%TARGET%\GHA_Project\SimpleML.csproj" -c Release -p:RhinoMajorVersion=7
pause
