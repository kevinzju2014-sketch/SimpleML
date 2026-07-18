@echo off
REM One-click install SimpleML release onto THIS Windows Rhino PC
setlocal EnableExtensions
cd /d "%~dp0"

set "SRC=%~dp0SimpleML-1.2.0"
if not exist "%SRC%\SimpleML.gha" (
  if exist "%~dp0SimpleML-1.2.0.zip" (
    echo Unzipping SimpleML-1.2.0.zip ...
    powershell -NoProfile -Command "Expand-Archive -Force '%~dp0SimpleML-1.2.0.zip' '%~dp0'"
  )
)
if not exist "%SRC%\SimpleML.gha" (
  echo ERROR: SimpleML-1.2.0\SimpleML.gha not found.
  pause
  exit /b 1
)

set "DEST=%APPDATA%\Grasshopper\Libraries\SimpleML"
echo Installing to: %DEST%
if not exist "%DEST%" mkdir "%DEST%"
xcopy /E /I /Y "%SRC%\*" "%DEST%\" >nul
copy /Y "%SRC%\SimpleML.gha" "%APPDATA%\Grasshopper\Libraries\SimpleML.gha" >nul

where python >nul 2>nul
if %ERRORLEVEL%==0 (
  python -m pip install -r "%DEST%\myML\requirements.txt"
) else (
  where python3 >nul 2>nul
  if %ERRORLEVEL%==0 (
    python3 -m pip install -r "%DEST%\myML\requirements.txt"
  ) else (
    echo WARN: Python not found on PATH. Install deps manually later.
  )
)

echo.
echo DONE. Fully quit Rhino, reopen, open Grasshopper, run Health Check.
echo Path: %DEST%
pause
