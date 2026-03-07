# SimpleML .gha 文件创建指南

## 概述

本文档说明如何将SimpleML插件封装成.gha文件（Grasshopper Assembly文件）供其他人安装使用。

## 方法1: 使用Grasshopper User Objects（推荐）

### 步骤1: 创建用户对象

1. 在Grasshopper中创建Python Script组件
2. 配置代码和输入输出
3. 右键组件 → **Create User Object**
4. 设置：
   - Name: `SimpleML - [功能名称]`
   - Category: `SimpleML - [分类名称]`
   - Description: 功能描述
   - Icon: （可选）添加图标

### 步骤2: 保存用户对象

1. 保存为 `.ghuser` 文件
2. 保存到：`%APPDATA%\Grasshopper\UserObjects\`

### 步骤3: 创建用户对象库

将所有 `.ghuser` 文件组织到文件夹中：
```
UserObjects/
├── SimpleML - Data Input/
│   ├── Read CSV.ghuser
│   ├── Read Excel.ghuser
│   └── ...
├── SimpleML - Data Analysis/
│   └── ...
└── ...
```

### 步骤4: 打包分发

将以下内容打包成ZIP文件：
- `myML/` 文件夹（核心代码）
- `UserObjects/` 文件夹（用户对象文件）
- `Install/` 文件夹（安装脚本）
- `Docs/` 文件夹（文档）
- `README.md`（安装说明）

## 方法2: 创建.gha文件（需要Grasshopper SDK）

### 前提条件

- Visual Studio
- Grasshopper SDK
- .NET Framework 4.7+

### 步骤1: 创建Grasshopper插件项目

1. 创建新的Class Library项目
2. 添加Grasshopper SDK引用
3. 创建组件类

### 步骤2: 实现组件

```csharp
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SimpleML
{
    public class ReadCSVComponent : GH_Component
    {
        public ReadCSVComponent()
          : base("Read CSV", "ReadCSV",
              "Read CSV file",
              "SimpleML", "Data Input")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "F", "CSV file path", GH_ParamAccess.item);
            // 添加更多输入参数...
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "DataFrame", GH_ParamAccess.item);
            // 添加更多输出参数...
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 实现逻辑
            // 调用Python脚本或直接实现
        }

        protected override System.Drawing.Bitmap Icon => null; // 添加图标

        public override Guid ComponentGuid => new Guid("...");
    }
}
```

### 步骤3: 编译和打包

1. 编译项目生成.dll文件
2. 将.dll文件重命名为.gha
3. 复制到Grasshopper的Libraries文件夹

## 方法3: Python脚本包（最简单）

### 步骤1: 创建安装包结构

```
SimpleML_Package/
├── myML/                    # 核心代码
├── Install/                 # 安装脚本
│   ├── install.bat
│   ├── install.sh
│   └── requirements.txt
├── UserObjects/            # 用户对象模板
├── Docs/                   # 文档
└── README.md              # 安装说明
```

### 步骤2: 创建安装脚本

使用提供的 `install.bat` 和 `install.sh` 脚本

### 步骤3: 创建用户对象模板

使用 `create_ghuser_templates.py` 生成模板

### 步骤4: 打包分发

运行 `create_package.py` 创建ZIP安装包

## 推荐方案

对于SimpleML插件，**推荐使用方法3（Python脚本包）**，因为：

1. ✅ 不需要编译
2. ✅ 跨平台兼容
3. ✅ 易于更新和维护
4. ✅ 用户可以直接修改Python代码

## 分发清单

### 必需文件

- [x] `myML/` 文件夹（完整代码）
- [x] `Install/install.bat`（Windows安装脚本）
- [x] `Install/install.sh`（Mac/Linux安装脚本）
- [x] `Install/requirements.txt`（Python依赖）
- [x] `README.md`（安装说明）

### 可选文件

- [ ] `UserObjects/` 文件夹（预配置的用户对象）
- [ ] `Docs/` 文件夹（文档）
- [ ] 示例文件（.ghx文件）

## 安装包创建步骤

1. **运行创建脚本**：
   ```bash
   python create_package.py
   ```

2. **测试安装包**：
   - 在干净的系统中测试安装
   - 验证所有功能正常

3. **创建用户对象**（可选）：
   - 在Grasshopper中创建用户对象
   - 导出为.ghuser文件
   - 包含在安装包中

4. **打包分发**：
   - 创建ZIP文件
   - 添加版本号
   - 上传到分发平台

## 版本管理

建议使用语义化版本号：
- 格式：`SimpleML_v主版本.次版本.修订版本.zip`
- 示例：`SimpleML_v1.0.0.zip`

## 注意事项

1. **路径问题**：确保安装脚本中的路径正确
2. **权限问题**：确保安装脚本有写入权限
3. **Python版本**：明确要求Python 3.7+
4. **依赖版本**：在requirements.txt中指定版本范围
5. **文档完整性**：确保包含所有必要的文档

---

**创建日期**：2026年1月26日
**版本**：1.0.0
