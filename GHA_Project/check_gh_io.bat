@echo off
echo Checking for GH_IO.dll...
echo.

if exist "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll" (
    echo [FOUND] C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll
) else (
    echo [NOT FOUND] C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll
)

if exist "C:\Program Files\Rhino 8\System\GH_IO.dll" (
    echo [FOUND] C:\Program Files\Rhino 8\System\GH_IO.dll
) else (
    echo [NOT FOUND] C:\Program Files\Rhino 8\System\GH_IO.dll
)

echo.
echo Searching in Grasshopper directory...
dir "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll" 2>nul
if errorlevel 1 (
    echo GH_IO.dll not found in expected location.
    echo Please check the actual location and update SimpleML.csproj
)

pause
