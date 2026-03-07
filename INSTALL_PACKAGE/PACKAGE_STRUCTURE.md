# SimpleML 安装包结构

## 最终安装包结构

```
SimpleML_Package/
├── README.md                    # 安装说明
├── myML/                        # 核心代码库
│   ├── components/              # 组件层
│   ├── core/                    # 核心功能层
│   ├── examples/                # 示例文件
│   └── requirements.txt         # Python依赖
│
├── Install/                     # 安装脚本
│   ├── install.bat              # Windows安装脚本
│   ├── install.sh               # Mac/Linux安装脚本
│   └── requirements.txt         # Python依赖列表
│
├── UserObjects/                 # 用户对象模板（可选）
│   └── [.ghuser files]
│
└── Docs/                        # 文档
    ├── README_SIMPLEML.md
    ├── SIMPLEML_CATEGORIES.md
    ├── SIMPLEML_GRASSHOPPER_SETUP.md
    └── [其他文档]
```

## 创建安装包的步骤

### 方法1: 手动创建

1. 创建 `SimpleML_Package` 文件夹
2. 复制 `myML` 文件夹到 `SimpleML_Package/`
3. 创建 `Install` 文件夹，复制安装脚本
4. 创建 `Docs` 文件夹，复制文档
5. 创建 `README.md` 安装说明
6. 压缩成ZIP文件

### 方法2: 使用脚本（推荐）

运行 `create_package.py` 脚本自动创建安装包。

## 分发文件

最终分发的文件应该是：
- `SimpleML_v1.0.0_YYYYMMDD.zip` - 完整的安装包

用户下载后：
1. 解压ZIP文件
2. 运行 `Install/install.bat`（Windows）或 `Install/install.sh`（Mac/Linux）
3. 按照提示完成安装

## 安装位置

安装后，文件将位于：

**Windows**:
- 代码: `%APPDATA%\Grasshopper\UserObjects\SimpleML\myML\`
- 用户对象: `%APPDATA%\Grasshopper\UserObjects\`

**Mac/Linux**:
- 代码: `~/.grasshopper/UserObjects/SimpleML/myML/`
- 用户对象: `~/.grasshopper/UserObjects/`
