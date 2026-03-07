#!/bin/bash

echo "========================================"
echo "SimpleML Plugin Installer"
echo "========================================"
echo ""

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# 智能检测myML文件夹位置
# 情况1: 脚本在 INSTALL_PACKAGE 目录中，myML在父目录
PACKAGE_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
if [ -d "$PACKAGE_DIR/myML" ]; then
    MYML_SOURCE="$PACKAGE_DIR/myML"
    cd "$PACKAGE_DIR"
# 情况2: 脚本在 Install 目录中，myML在父目录的父目录
elif [ -d "$(cd "$SCRIPT_DIR/../.." && pwd)/myML" ]; then
    PACKAGE_DIR2="$(cd "$SCRIPT_DIR/../.." && pwd)"
    MYML_SOURCE="$PACKAGE_DIR2/myML"
    cd "$PACKAGE_DIR2"
# 情况3: 当前目录就是myML目录
elif [ -d "components" ] && [ -d "core" ]; then
    MYML_SOURCE="$(pwd)"
# 情况4: myML在当前目录的父目录
elif [ -d "../myML" ]; then
    MYML_SOURCE="$(cd .. && pwd)/myML"
    cd ..
else
    echo "ERROR: myML folder not found!"
    echo ""
    echo "Script location: $SCRIPT_DIR"
    echo "Current directory: $(pwd)"
    echo ""
    echo "Please make sure:"
    echo "1. You are running from SimpleML_Package directory, OR"
    echo "2. The myML folder is accessible from the current location"
    exit 1
fi

echo "Found myML folder at: $MYML_SOURCE"
echo ""

# 检查Python是否安装
if ! command -v python3 &> /dev/null; then
    echo "ERROR: Python 3 is not installed"
    echo "Please install Python 3.7+ and try again"
    exit 1
fi

echo "[1/4] Checking Python installation..."
python3 --version
echo ""

echo "[2/4] Installing Python dependencies..."
pip3 install scikit-learn numpy pandas joblib openpyxl
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install dependencies"
    echo "You can try installing manually: pip3 install -r Install/requirements.txt"
    exit 1
fi
echo ""

# 获取用户对象文件夹路径（Mac/Linux）
USER_OBJECTS="$HOME/.grasshopper/UserObjects"
SIMPLEML_PATH="$USER_OBJECTS/SimpleML"

echo "[3/4] Creating directories..."
mkdir -p "$USER_OBJECTS"
mkdir -p "$SIMPLEML_PATH"
echo ""

echo "[4/4] Copying files..."
echo "Source: $MYML_SOURCE"
echo "Destination: $SIMPLEML_PATH/myML"
echo ""

# 复制myML文件夹
cp -r "$MYML_SOURCE" "$SIMPLEML_PATH/myML"
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to copy files"
    echo "Source: $MYML_SOURCE"
    echo "Destination: $SIMPLEML_PATH/myML"
    exit 1
fi

# 复制用户对象文件（如果存在）
if [ -d "UserObjects" ] && [ "$(ls -A UserObjects/*.ghuser 2>/dev/null)" ]; then
    echo "Copying user objects..."
    cp UserObjects/*.ghuser "$USER_OBJECTS/"
fi

echo ""
echo "========================================"
echo "Installation completed successfully!"
echo "========================================"
echo ""
echo "SimpleML has been installed to:"
echo "$SIMPLEML_PATH/myML"
echo ""
echo "Please restart Grasshopper to use the plugin."
echo ""
echo "To use SimpleML in Grasshopper Python components, add:"
echo "  import sys"
echo "  sys.path.append(r'$SIMPLEML_PATH/myML')"
echo ""
