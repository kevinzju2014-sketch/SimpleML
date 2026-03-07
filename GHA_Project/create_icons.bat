@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo 正在生成组件图标...
python generate_icons_simple.py
if errorlevel 1 (
    echo.
    echo 错误: 无法运行脚本
    echo 请确保已安装 Pillow 库: pip install Pillow
    pause
    exit /b 1
)
echo.
echo 图标生成完成！
pause
