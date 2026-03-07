# SimpleML 插件安装包

## 安装包内容

本安装包包含：
- SimpleML 核心代码库
- Grasshopper 用户对象文件（.ghuser）
- 安装脚本
- 使用文档

## 安装方法

### 方法1: 自动安装（推荐）

1. 双击运行 `install.bat`（Windows）或 `install.sh`（Mac/Linux）
2. 按照提示完成安装

### 方法2: 手动安装

#### 步骤1: 安装Python依赖

打开命令行，运行：

```bash
pip install scikit-learn numpy pandas joblib openpyxl
```

#### 步骤2: 复制文件

1. 将 `myML` 文件夹复制到以下位置之一：
   - `C:\Users\[用户名]\AppData\Roaming\Grasshopper\UserObjects\SimpleML\`
   - 或任何Grasshopper可以访问的位置

2. 将 `UserObjects` 文件夹中的 `.ghuser` 文件复制到：
   - `C:\Users\[用户名]\AppData\Roaming\Grasshopper\UserObjects\`

#### 步骤3: 配置Python路径

在Grasshopper的Python组件中，确保Python路径包含myML文件夹。

## 使用说明

安装完成后，在Grasshopper中：
1. 右键画布 → User Objects → SimpleML
2. 选择需要的电池拖到画布上
3. 参考文档了解每个电池的使用方法

## 系统要求

- Rhino 8
- Grasshopper
- Python 3.7+
- scikit-learn >= 1.0.0
- numpy >= 1.20.0
- pandas >= 1.3.0
- joblib >= 1.0.0
- openpyxl >= 3.0.0

## 卸载方法

1. 删除 `UserObjects` 文件夹中的 SimpleML 相关文件
2. 删除 `myML` 文件夹
3. （可选）卸载Python依赖包

## 故障排除

### 问题1: Python组件找不到模块
**解决方案**: 确保在Python组件中添加了正确的路径：
```python
import sys
sys.path.append(r'[myML文件夹的完整路径]')
```

### 问题2: 缺少依赖库
**解决方案**: 运行 `pip install -r requirements.txt`

### 问题3: 用户对象不显示
**解决方案**: 
1. 检查文件是否复制到正确位置
2. 重启Grasshopper
3. 检查文件权限

## 支持

如有问题，请查看文档或提交Issue。

---

**SimpleML v1.0.0**
