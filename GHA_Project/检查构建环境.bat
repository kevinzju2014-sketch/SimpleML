@echo off
chcp 65001 >nul
echo ========================================
echo SimpleML 构建环境检查
echo ========================================
echo.

set "ERROR_COUNT=0"

echo [1/5] 检查 Visual Studio / MSBuild...
set "MSBUILD_FOUND=0"

if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    echo   ✅ 找到 Visual Studio 2022 Community
    set "MSBUILD_FOUND=1"
    goto :check_rhino
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    echo   ✅ 找到 Visual Studio 2022 Professional
    set "MSBUILD_FOUND=1"
    goto :check_rhino
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    echo   ✅ 找到 Visual Studio 2022 Enterprise
    set "MSBUILD_FOUND=1"
    goto :check_rhino
)

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    echo   ✅ 找到 Visual Studio 2019 Community
    set "MSBUILD_FOUND=1"
    goto :check_rhino
)

where dotnet >nul 2>&1
if not errorlevel 1 (
    echo   ✅ 找到 .NET SDK (dotnet CLI)
    set "MSBUILD_FOUND=1"
    goto :check_rhino
)

echo   ❌ 未找到 MSBuild 或 .NET SDK
echo   请安装 Visual Studio 2019/2022 或 .NET SDK
set /a ERROR_COUNT+=1

:check_rhino
echo.
echo [2/5] 检查 Rhino 8 安装...
if exist "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll" (
    echo   ✅ Grasshopper.dll 存在
) else (
    echo   ❌ 找不到 Grasshopper.dll
    echo   路径: C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll
    set /a ERROR_COUNT+=1
)

if exist "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll" (
    echo   ✅ GH_IO.dll 存在
) else (
    echo   ❌ 找不到 GH_IO.dll
    echo   路径: C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll
    set /a ERROR_COUNT+=1
)

if exist "C:\Program Files\Rhino 8\System\RhinoCommon.dll" (
    echo   ✅ RhinoCommon.dll 存在
) else (
    echo   ❌ 找不到 RhinoCommon.dll
    echo   路径: C:\Program Files\Rhino 8\System\RhinoCommon.dll
    set /a ERROR_COUNT+=1
)

echo.
echo [3/5] 检查项目文件...
if exist "SimpleML.csproj" (
    echo   ✅ SimpleML.csproj 存在
) else (
    echo   ❌ 找不到项目文件
    set /a ERROR_COUNT+=1
)

if exist "Components" (
    echo   ✅ Components 文件夹存在
) else (
    echo   ❌ 找不到 Components 文件夹
    set /a ERROR_COUNT+=1
)

if exist "SimpleMLPlugin.cs" (
    echo   ✅ SimpleMLPlugin.cs 存在
) else (
    echo   ❌ 找不到 SimpleMLPlugin.cs
    set /a ERROR_COUNT+=1
)

echo.
echo [4/5] 检查 Python 代码...
if exist "..\components" (
    echo   ✅ components 文件夹存在
) else (
    echo   ⚠️  找不到 components 文件夹（可能不影响构建）
)

if exist "..\core" (
    echo   ✅ core 文件夹存在
) else (
    echo   ⚠️  找不到 core 文件夹（可能不影响构建）
)

echo.
echo [5/5] 检查 .NET Framework 4.8...
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release >nul 2>&1
if not errorlevel 1 (
    echo   ✅ .NET Framework 4.x 已安装
) else (
    echo   ⚠️  无法确认 .NET Framework 4.8 安装状态
    echo   请手动检查：https://dotnet.microsoft.com/download/dotnet-framework/net48
)

echo.
echo ========================================
if %ERROR_COUNT% EQU 0 (
    echo ✅ 环境检查通过！可以开始构建
    echo.
    echo 下一步：
    echo 1. 在 Visual Studio 中打开 SimpleML.csproj
    echo 2. 选择 Release 配置
    echo 3. 生成 → 生成 SimpleML
    echo.
    echo 或运行: build.bat
) else (
    echo ❌ 发现 %ERROR_COUNT% 个问题，请先解决
    echo.
    echo 请参考：构建准备清单.md
)
echo ========================================
echo.

pause
