@echo off
REM 简单的 GHA 文件创建脚本
REM 如果自动构建没有生成 .gha 文件，可以使用此脚本手动创建

echo ========================================
echo SimpleML GHA 文件创建工具
echo ========================================
echo.

cd /d "%~dp0"

REM 检查 Release 配置
set "DLL_PATH=bin\Release\SimpleML.dll"
set "GHA_PATH=bin\Release\SimpleML.gha"

if exist "%DLL_PATH%" (
    echo 找到 Release DLL: %DLL_PATH%
    goto :create_gha
)

REM 检查 Debug 配置
set "DLL_PATH=bin\Debug\SimpleML.dll"
set "GHA_PATH=bin\Debug\SimpleML.gha"

if exist "%DLL_PATH%" (
    echo 找到 Debug DLL: %DLL_PATH%
    goto :create_gha
)

echo 错误: 找不到 DLL 文件！
echo 请先构建项目（Release 或 Debug 配置）
pause
exit /b 1

:create_gha
echo.
echo 创建 GHA 文件...
if exist "%GHA_PATH%" (
    del /F /Q "%GHA_PATH%"
    echo 已删除旧的 GHA 文件
)

copy /Y "%DLL_PATH%" "%GHA_PATH%"

if exist "%GHA_PATH%" (
    echo.
    echo ========================================
    echo GHA 文件已成功创建！
    echo ========================================
    echo 位置: %GHA_PATH%
    echo.
    dir "%GHA_PATH%"
    echo.
    echo 下一步:
    echo 1. 将 GHA 文件复制到: %%APPDATA%%\Grasshopper\Libraries\
    echo 2. 完全关闭 Rhino
    echo 3. 重新启动 Rhino 并打开 Grasshopper
) else (
    echo 错误: GHA 文件创建失败！
)

pause
