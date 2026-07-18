@echo off
cd /d "%~dp0"
dotnet build SimpleML.csproj -c Release -p:RhinoMajorVersion=7
if errorlevel 1 (
  echo Fallback: NuGet Rhino refs...
  dotnet build SimpleML.csproj -c Release -p:UseNuGetRhino=true
)
echo.
echo Output: %~dp0bin\Release\SimpleML.gha
pause
