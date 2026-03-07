# SimpleML 安装问题排查指南

## 问题：找不到myML文件夹

### 错误信息
```
ERROR: myML folder not found!
Current directory: D:\Helio\250928_机器学习课程\myML
```

### 原因分析

这个错误通常发生在以下情况：
1. 从错误的目录运行安装脚本
2. 安装包结构不正确
3. myML文件夹不在预期位置

### 解决方案

#### 方案1: 使用快速安装脚本（推荐）

如果您在 `myML` 目录中，可以使用快速安装脚本：

1. 将 `QUICK_INSTALL.bat` 复制到 `myML` 目录
2. 双击运行 `QUICK_INSTALL.bat`

#### 方案2: 手动安装

1. **安装Python依赖**：
   ```bash
   pip install scikit-learn numpy pandas joblib openpyxl
   ```

2. **手动复制文件**：
   ```batch
   xcopy /E /I /Y "D:\Helio\250928_机器学习课程\myML" "%APPDATA%\Grasshopper\UserObjects\SimpleML\myML"
   ```

#### 方案3: 从正确的位置运行

**正确的目录结构应该是**：
```
SimpleML_Package/
├── myML/              ← myML文件夹在这里
├── Install/
│   └── install.bat   ← 安装脚本在这里
└── README.md
```

**运行方式**：
1. 进入 `SimpleML_Package` 目录
2. 运行 `Install\install.bat`

#### 方案4: 修改安装脚本路径

如果myML文件夹在其他位置，可以修改安装脚本：

1. 打开 `install.bat`
2. 找到 `MYML_SOURCE` 设置
3. 修改为myML的实际路径：
   ```batch
   set "MYML_SOURCE=D:\Helio\250928_机器学习课程\myML"
   ```

## 常见安装场景

### 场景1: 从myML目录直接安装

如果您在 `myML` 目录中：

1. **使用快速安装脚本**：
   - 复制 `QUICK_INSTALL.bat` 到 `myML` 目录
   - 双击运行

2. **或手动安装**：
   ```batch
   pip install scikit-learn numpy pandas joblib openpyxl
   xcopy /E /I /Y "%CD%" "%APPDATA%\Grasshopper\UserObjects\SimpleML\myML"
   ```

### 场景2: 从SimpleML_Package目录安装

如果已解压安装包：

1. 进入 `SimpleML_Package` 目录
2. 运行 `Install\install.bat`

### 场景3: 从Install目录安装

如果直接在 `Install` 目录中：

1. 脚本会自动查找父目录的 `myML` 文件夹
2. 如果找不到，请确保目录结构正确

## 验证安装

安装完成后，验证安装：

1. **检查文件是否复制**：
   ```
   %APPDATA%\Grasshopper\UserObjects\SimpleML\myML
   ```

2. **在Grasshopper中测试**：
   ```python
   import sys
   sys.path.append(r'%APPDATA%\Grasshopper\UserObjects\SimpleML\myML')
   from components import read_excel
   print("SimpleML installed successfully!")
   ```

## 手动安装步骤（完整）

如果自动安装失败，可以手动安装：

### 步骤1: 安装Python依赖

```bash
pip install scikit-learn numpy pandas joblib openpyxl
```

### 步骤2: 创建目标目录

```batch
mkdir "%APPDATA%\Grasshopper\UserObjects\SimpleML"
```

### 步骤3: 复制myML文件夹

```batch
xcopy /E /I /Y "D:\Helio\250928_机器学习课程\myML" "%APPDATA%\Grasshopper\UserObjects\SimpleML\myML"
```

**注意**：将路径替换为您的实际myML路径。

### 步骤4: 验证

检查文件是否复制成功：
```batch
dir "%APPDATA%\Grasshopper\UserObjects\SimpleML\myML"
```

应该看到 `components` 和 `core` 文件夹。

## 路径说明

### Windows默认安装路径

```
%APPDATA%\Grasshopper\UserObjects\SimpleML\myML
```

展开后通常是：
```
C:\Users\[用户名]\AppData\Roaming\Grasshopper\UserObjects\SimpleML\myML
```

### 在Grasshopper中使用

在Python组件中添加：
```python
import sys
sys.path.append(r'C:\Users\[用户名]\AppData\Roaming\Grasshopper\UserObjects\SimpleML\myML')
```

或使用环境变量：
```python
import sys
import os
simpleml_path = os.path.join(os.getenv('APPDATA'), 'Grasshopper', 'UserObjects', 'SimpleML', 'myML')
sys.path.append(simpleml_path)
```

---

**更新日期**：2026年1月26日
