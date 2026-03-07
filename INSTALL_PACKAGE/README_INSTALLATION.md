# SimpleML 插件安装包创建和使用指南

## 概述

SimpleML插件可以通过以下方式分发：
1. **ZIP安装包**（推荐）- 包含所有文件和安装脚本
2. **.gha文件** - Grasshopper Assembly文件（需要SDK开发）
3. **用户对象库** - .ghuser文件集合

## 快速创建安装包

### 步骤1: 运行创建脚本

```bash
cd INSTALL_PACKAGE
python create_package.py
```

这将创建：
- `SimpleML_Package/` 文件夹（包含所有文件）
- `SimpleML_v1.0.0_YYYYMMDD.zip` 压缩包

### 步骤2: 测试安装包

1. 在干净的系统中解压ZIP文件
2. 运行 `Install/install.bat`（Windows）或 `Install/install.sh`（Mac/Linux）
3. 验证安装是否成功

### 步骤3: 分发

将ZIP文件上传到分发平台或发送给用户。

## 安装包结构

```
SimpleML_Package/
├── README.md                 # 主安装说明
├── myML/                     # 核心代码库
│   ├── components/          # 组件层
│   ├── core/                # 核心功能层
│   ├── examples/            # 示例文件
│   └── requirements.txt     # Python依赖
├── Install/                  # 安装脚本
│   ├── install.bat          # Windows安装脚本
│   ├── install.sh           # Mac/Linux安装脚本
│   └── requirements.txt     # Python依赖列表
└── Docs/                     # 文档文件夹
    └── [所有文档文件]
```

## 用户安装步骤

### Windows用户

1. **下载安装包**
   - 下载 `SimpleML_v1.0.0.zip`

2. **解压文件**
   - 解压到任意位置

3. **运行安装脚本**
   - 双击 `Install\install.bat`
   - 按照提示完成安装

4. **重启Grasshopper**
   - 关闭并重新打开Grasshopper

5. **使用插件**
   - 在Grasshopper中，右键画布 → User Objects → SimpleML
   - 选择需要的电池拖到画布上

### Mac/Linux用户

1. **下载安装包**
   - 下载 `SimpleML_v1.0.0.zip`

2. **解压文件**
   - 解压到任意位置

3. **运行安装脚本**
   ```bash
   cd SimpleML_Package
   bash Install/install.sh
   ```

4. **重启Grasshopper**

5. **使用插件**

## 手动安装（如果自动安装失败）

### 步骤1: 安装Python依赖

```bash
pip install scikit-learn numpy pandas joblib openpyxl
```

### 步骤2: 复制文件

**Windows**:
```batch
xcopy /E /I /Y "myML" "%APPDATA%\Grasshopper\UserObjects\SimpleML\myML"
```

**Mac/Linux**:
```bash
cp -r myML ~/.grasshopper/UserObjects/SimpleML/
```

### 步骤3: 配置Python路径

在Grasshopper的Python组件中，添加：

```python
import sys
sys.path.append(r'[myML文件夹的完整路径]')
```

## 验证安装

### 测试1: 导入模块

在Grasshopper Python组件中：

```python
import sys
sys.path.append(r'[myML路径]')

from components import read_excel
print("SimpleML installed successfully!")
```

### 测试2: 运行示例

运行 `examples/` 文件夹中的示例文件。

## 卸载

### Windows

1. 删除文件夹：
   ```
   %APPDATA%\Grasshopper\UserObjects\SimpleML
   ```

2. （可选）卸载Python依赖：
   ```bash
   pip uninstall scikit-learn numpy pandas joblib openpyxl
   ```

### Mac/Linux

1. 删除文件夹：
   ```bash
   rm -rf ~/.grasshopper/UserObjects/SimpleML
   ```

2. （可选）卸载Python依赖

## 故障排除

### 问题1: Python未找到

**解决方案**:
- 确保Python已安装并在PATH中
- Windows: 重新安装Python并选择"Add to PATH"
- Mac/Linux: 使用 `python3` 命令

### 问题2: 依赖安装失败

**解决方案**:
- 检查网络连接
- 使用国内镜像：
  ```bash
  pip install -i https://pypi.tuna.tsinghua.edu.cn/simple scikit-learn numpy pandas joblib openpyxl
  ```

### 问题3: 模块导入错误

**解决方案**:
- 检查myML文件夹路径是否正确
- 确认在Python组件中添加了路径
- 检查文件权限

### 问题4: 用户对象不显示

**解决方案**:
- 重启Grasshopper
- 检查文件是否复制到正确位置
- 检查文件权限

## 系统要求

- **操作系统**: Windows 10+, macOS 10.14+, Linux
- **Rhino**: Rhino 8
- **Grasshopper**: 最新版本
- **Python**: 3.7 或更高版本
- **依赖库**: 见 `Install/requirements.txt`

## 支持

如有问题，请：
1. 查看 `Docs/` 文件夹中的文档
2. 检查故障排除部分
3. 提交Issue或联系支持

---

**SimpleML v1.0.0**
**创建日期**: 2026年1月26日
