@echo off
setlocal EnableExtensions
echo === SimpleML Installer (Windows / Rhino 7+) ===

set "ROOT=%~dp0"
set "ROOT=%ROOT:~0,-1%"

if defined PYTHON_PATH (
  set "PY=%PYTHON_PATH%"
) else (
  where python >nul 2>&1 && set "PY=python" || set "PY="
)
if not defined PY (
  echo ERROR: 未找到 python。请安装 Python 3.9+ 或设置 PYTHON_PATH
  exit /b 1
)

echo Python: %PY%
"%PY%" -m pip install --upgrade pip
"%PY%" -m pip install -r "%ROOT%\requirements.txt"
if errorlevel 1 (
  echo WARN: pip 安装可能失败，请检查网络与权限
)

set "TARGET=%APPDATA%\Grasshopper\Libraries"
if not exist "%TARGET%" mkdir "%TARGET%"
set "DEST=%TARGET%\SimpleML"
if not exist "%DEST%" mkdir "%DEST%"
if not exist "%DEST%\myML" mkdir "%DEST%\myML"

echo Copying Python package...
xcopy /E /I /Y "%ROOT%\components" "%DEST%\myML\components\" >nul
xcopy /E /I /Y "%ROOT%\core" "%DEST%\myML\core\" >nul
copy /Y "%ROOT%\requirements.txt" "%DEST%\myML\requirements.txt" >nul
if exist "%ROOT%\examples" xcopy /E /I /Y "%ROOT%\examples" "%DEST%\myML\examples\" >nul

set "GHA=%ROOT%\GHA_Project\bin\Release\SimpleML.gha"
if not exist "%GHA%" set "GHA=%ROOT%\GHA_Project\bin\Debug\SimpleML.gha"
if exist "%GHA%" (
  copy /Y "%GHA%" "%DEST%\SimpleML.gha" >nul
  copy /Y "%GHA%" "%TARGET%\SimpleML.gha" >nul
  echo Installed GHA -^> %TARGET%\SimpleML.gha
) else (
  echo WARN: 未找到已编译 SimpleML.gha
  echo 请先: dotnet build GHA_Project\SimpleML.csproj -c Release -p:RhinoMajorVersion=7
)

echo.
echo 建议设置用户环境变量:
echo   SIMPLEML_PATH=%DEST%\myML
echo   PYTHON_PATH=^(你的 python.exe 完整路径^)
echo.
echo 完成。请重启 Rhino，运行「环境体检」或「新手向导」。
endlocal
