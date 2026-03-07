# 安装脚本修复说明

## 问题描述

安装脚本在 `INSTALL_PACKAGE` 目录中运行时，无法找到 `myML` 文件夹，因为 `myML` 文件夹位于 `INSTALL_PACKAGE` 的父目录中。

## 解决方案

已修复 `install.bat` 和 `install.sh` 脚本，现在它们会：

1. **自动检测脚本位置**：获取脚本所在目录
2. **切换到正确目录**：自动切换到包含 `myML` 文件夹的目录
3. **检查文件存在**：在复制前检查 `myML` 文件夹是否存在
4. **提供清晰的错误信息**：如果文件不存在，显示详细的错误信息

## 使用方法

### 方法1: 从SimpleML_Package目录运行（推荐）

1. 解压安装包到任意位置
2. 进入 `SimpleML_Package` 目录
3. 运行 `Install\install.bat`（Windows）或 `bash Install/install.sh`（Mac/Linux）

### 方法2: 从Install目录运行

1. 进入 `SimpleML_Package\Install` 目录
2. 运行 `install.bat`（Windows）或 `bash install.sh`（Mac/Linux）
3. 脚本会自动找到 `myML` 文件夹

## 修复内容

### install.bat 修复

- ✅ 添加了脚本目录检测
- ✅ 自动切换到包含myML的目录
- ✅ 添加了文件存在性检查
- ✅ 改进了错误提示信息

### install.sh 修复

- ✅ 添加了脚本目录检测
- ✅ 自动切换到包含myML的目录
- ✅ 添加了文件存在性检查
- ✅ 改进了错误提示信息

## 测试安装

修复后，请按以下步骤测试：

1. **解压安装包**
   ```
   SimpleML_Package/
   ├── myML/
   ├── Install/
   │   ├── install.bat
   │   └── install.sh
   └── README.md
   ```

2. **运行安装脚本**
   - Windows: 双击 `Install\install.bat`
   - Mac/Linux: 运行 `bash Install/install.sh`

3. **验证安装**
   - 检查文件是否复制到正确位置
   - 在Grasshopper中测试功能

## 如果仍然遇到问题

### 手动安装步骤

1. **安装Python依赖**：
   ```bash
   pip install scikit-learn numpy pandas joblib openpyxl
   ```

2. **手动复制文件**：
   - Windows:
     ```batch
     xcopy /E /I /Y "myML" "%APPDATA%\Grasshopper\UserObjects\SimpleML\myML"
     ```
   - Mac/Linux:
     ```bash
     cp -r myML ~/.grasshopper/UserObjects/SimpleML/
     ```

3. **验证安装**：
   在Grasshopper Python组件中：
   ```python
   import sys
   sys.path.append(r'[安装路径]\myML')
   from components import read_excel
   print("SimpleML installed successfully!")
   ```

---

**修复日期**：2026年1月26日
