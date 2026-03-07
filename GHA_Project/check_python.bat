@echo off
echo ========================================
echo SimpleML Python环境检查工具
echo ========================================
echo.

echo [1/4] 检查系统PATH中的Python...
where python >nul 2>&1
if %errorlevel% equ 0 (
    echo 找到Python命令
    where python
    echo.
    python --version
    echo.
) else (
    echo 未找到Python命令
    echo.
)

echo [2/4] 检查PYTHON_PATH环境变量...
if defined PYTHON_PATH (
    echo PYTHON_PATH = %PYTHON_PATH%
    if exist "%PYTHON_PATH%" (
        echo Python文件存在: %PYTHON_PATH%
        "%PYTHON_PATH%" --version
    ) else (
        echo 警告: PYTHON_PATH指向的文件不存在
    )
) else (
    echo PYTHON_PATH环境变量未设置
)
echo.

echo [3/4] 检查常见Python安装路径...
set "python_paths[0]=C:\Python39\python.exe"
set "python_paths[1]=C:\Python310\python.exe"
set "python_paths[2]=C:\Python311\python.exe"
set "python_paths[3]=C:\Python312\python.exe"
set "python_paths[4]=%LOCALAPPDATA%\Programs\Python\Python39\python.exe"
set "python_paths[5]=%LOCALAPPDATA%\Programs\Python\Python310\python.exe"
set "python_paths[6]=%LOCALAPPDATA%\Programs\Python\Python311\python.exe"
set "python_paths[7]=%LOCALAPPDATA%\Programs\Python\Python312\python.exe"

for /L %%i in (0,1,7) do (
    call set "path=%%python_paths[%%i]%%"
    if exist "!path!" (
        echo 找到: !path!
        "!path!" --version 2>nul
        echo.
    )
)

echo [4/4] 检查Rhino Python环境...
set "rhino_python=%LOCALAPPDATA%\.rhinocode\py39-rh8\python.exe"
if exist "%rhino_python%" (
    echo 警告: 发现Rhino Python环境
    echo %rhino_python%
    echo SimpleML不应使用Rhino的Python环境
    echo.
)

echo ========================================
echo 检查完成
echo ========================================
echo.
echo 建议:
echo 1. 如果找到多个Python，确保PYTHON_PATH指向系统Python（不是Rhino的Python）
echo 2. 系统Python应该能够导入json模块: python -c "import json; print('OK')"
echo 3. 确保已安装SimpleML依赖: pip install scikit-learn pandas numpy joblib openpyxl
echo.
pause
