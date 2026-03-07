# SimpleML 手动创建安装包指南

## 步骤1: 准备文件结构

创建以下文件夹结构：

```
SimpleML_Package/
├── myML/
├── Install/
├── Docs/
└── README.md
```

## 步骤2: 复制核心代码

将整个 `myML` 文件夹复制到 `SimpleML_Package/myML/`

**排除的文件**：
- `__pycache__/` 文件夹
- `*.pyc` 文件
- `.git/` 文件夹（如果有）
- `*.gh` 和 `*.ghx` 文件（Grasshopper文件）

## 步骤3: 创建安装脚本

### Install/install.bat (Windows)

```batch
@echo off
echo SimpleML Plugin Installer
echo.

REM 检查Python
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed
    pause
    exit /b 1
)

REM 安装依赖
echo Installing dependencies...
pip install scikit-learn numpy pandas joblib openpyxl

REM 创建目录
set "TARGET=%APPDATA%\Grasshopper\UserObjects\SimpleML"
mkdir "%TARGET%"

REM 复制文件
xcopy /E /I /Y "myML" "%TARGET%\myML"

echo.
echo Installation completed!
echo Please restart Grasshopper.
pause
```

### Install/install.sh (Mac/Linux)

```bash
#!/bin/bash
echo "SimpleML Plugin Installer"

# 检查Python
if ! command -v python3 &> /dev/null; then
    echo "ERROR: Python 3 is not installed"
    exit 1
fi

# 安装依赖
echo "Installing dependencies..."
pip3 install scikit-learn numpy pandas joblib openpyxl

# 创建目录
TARGET="$HOME/.grasshopper/UserObjects/SimpleML"
mkdir -p "$TARGET"

# 复制文件
cp -r myML "$TARGET/"

echo ""
echo "Installation completed!"
echo "Please restart Grasshopper."
```

### Install/requirements.txt

```
scikit-learn>=1.0.0
numpy>=1.20.0
pandas>=1.3.0
joblib>=1.0.0
openpyxl>=3.0.0
```

## 步骤4: 复制文档

将以下文档复制到 `Docs/` 文件夹：

- README_SIMPLEML.md
- SIMPLEML_CATEGORIES.md
- SIMPLEML_GRASSHOPPER_SETUP.md
- COMPONENTS_EXPLANATION.md
- WORKFLOW_DIAGRAM.md
- DATASET_GUIDE.md
- ALGORITHMS_GUIDE.md
- LOAD_MODEL_GUIDE.md

## 步骤5: 创建主README

创建 `SimpleML_Package/README.md`：

```markdown
# SimpleML Plugin Installation Package

## Quick Install

### Windows
1. Double-click `Install\install.bat`
2. Follow the prompts

### Mac/Linux
1. Open terminal
2. Run: `bash Install/install.sh`

## Manual Install

1. Install Python dependencies:
   ```bash
   pip install -r Install/requirements.txt
   ```

2. Copy `myML` folder to:
   - Windows: `%APPDATA%\Grasshopper\UserObjects\SimpleML\`
   - Mac/Linux: `~/.grasshopper/UserObjects/SimpleML/`

3. Restart Grasshopper

## Documentation

See `Docs/` folder for detailed documentation.

## System Requirements

- Rhino 8
- Grasshopper
- Python 3.7+
- See `Install/requirements.txt` for Python dependencies

---
SimpleML v1.0.0
```

## 步骤6: 创建ZIP文件

1. 选择 `SimpleML_Package` 文件夹
2. 右键 → 发送到 → 压缩(zipped)文件夹
3. 重命名为 `SimpleML_v1.0.0_YYYYMMDD.zip`

## 步骤7: 测试安装包

1. 在干净的系统中解压ZIP文件
2. 运行安装脚本
3. 验证安装是否成功
4. 测试Grasshopper中的功能

## 分发清单

- [x] SimpleML_Package文件夹
- [x] myML核心代码
- [x] Install安装脚本
- [x] Docs文档
- [x] README.md
- [x] ZIP压缩包

## 版本命名

使用格式：`SimpleML_v主版本.次版本.修订版本_日期.zip`

示例：`SimpleML_v1.0.0_20260126.zip`

---

**创建日期**：2026年1月26日
