# GHA 文件加载问题排查指南

## 问题：GHA文件无法加载

当您将GHA文件放到Grasshopper的Libraries文件夹后，重启Rhino但插件没有启动，请按照以下步骤排查：

---

## 快速检查清单

### ✅ 步骤1: 检查文件位置

**正确的安装位置**：
```
%APPDATA%\Grasshopper\Libraries\SimpleML.gha
```

**检查方法**：
1. 按 `Win + R`，输入 `%APPDATA%\Grasshopper\Libraries`
2. 确认 `SimpleML.gha` 文件存在
3. 检查文件大小（不应该为0字节）

**常见错误**：
- ❌ 放在了 `Libraries` 的子文件夹中
- ❌ 文件名拼写错误
- ❌ 文件扩展名不是 `.gha`

---

### ✅ 步骤2: 检查文件完整性

**检查GHA文件是否有效**：
1. 右键 `SimpleML.gha` → `属性`
2. 检查文件大小（通常应该 > 100 KB）
3. 如果文件大小为0或很小，说明构建失败

**验证方法**：
- 尝试用文本编辑器打开（应该显示乱码，如果是纯文本说明文件损坏）
- 检查文件修改时间（应该是最近构建的时间）

---

### ✅ 步骤3: 查看Grasshopper错误日志

**查看错误日志**：
1. 打开 Rhino
2. 打开 Grasshopper
3. 查看 `工具` → `选项` → `Grasshopper` → `日志`
4. 或查看 Rhino 的命令行窗口（如果有错误会显示）

**常见错误信息**：
- `无法加载程序集` - 依赖项问题
- `找不到类型` - 组件注册问题
- `版本不兼容` - .NET Framework版本问题

---

### ✅ 步骤4: 检查依赖项

**必需的依赖项**：
- Grasshopper.dll（Rhino 8自带）
- GH_IO.dll（Rhino 8自带）
- RhinoCommon.dll（Rhino 8自带）

**检查方法**：
1. 确认 Rhino 8 已正确安装
2. 检查路径：`C:\Program Files\Rhino 8\Plug-ins\Grasshopper\`
3. 确认这些DLL文件存在

---

### ✅ 步骤5: 检查.NET Framework版本

**要求**：
- .NET Framework 4.8 或更高版本

**检查方法**：
1. 打开 `控制面板` → `程序` → `程序和功能`
2. 查找 `.NET Framework 4.8` 或更高版本
3. 如果没有，下载安装：https://dotnet.microsoft.com/download/dotnet-framework/net48

---

### ✅ 步骤6: 检查组件注册

**问题**：如果缺少 `GH_ComponentLoader` 类，组件无法注册。

**解决方案**：
1. 检查 `SimpleMLPlugin.cs` 文件
2. 确认包含 `SimpleMLComponentLoader` 类
3. 确认所有组件都已注册

**已修复**：项目已更新，包含完整的组件加载器。

---

## 详细诊断步骤

### 方法1: 使用Grasshopper的组件浏览器

1. 打开 Grasshopper
2. 在组件面板中搜索 "SimpleML"
3. 如果找不到，说明插件未加载

### 方法2: 检查Rhino命令行

1. 打开 Rhino
2. 查看命令行窗口（底部）
3. 启动 Grasshopper 时，查看是否有错误信息
4. 常见错误：
   - `Failed to load assembly: SimpleML`
   - `Could not load type: SimpleMLPlugin`

### 方法3: 使用Grasshopper的插件管理器

1. 打开 Grasshopper
2. `文件` → `特殊文件夹` → `组件文件夹`
3. 查看是否有 `SimpleML.gha` 的加载错误

---

## 常见问题及解决方案

### 问题1: 文件已复制但组件不显示

**可能原因**：
- 组件加载器未正确注册
- 组件类有编译错误

**解决方案**：
1. ✅ 已更新 `SimpleMLPlugin.cs`，添加了 `SimpleMLComponentLoader` 类
2. 重新构建 GHA 文件
3. 替换旧的 GHA 文件
4. 重启 Rhino 和 Grasshopper

### 问题2: 显示"无法加载程序集"

**可能原因**：
- 缺少依赖项
- .NET Framework版本不匹配
- 文件损坏

**解决方案**：
1. 检查 Rhino 8 是否已安装
2. 检查 .NET Framework 4.8 是否已安装
3. 重新构建 GHA 文件
4. 确保构建时没有错误

### 问题3: 组件显示但无法使用

**可能原因**：
- Python 代码路径不正确
- myML 文件夹未正确安装

**解决方案**：
1. 检查 myML 文件夹位置：
   ```
   %APPDATA%\Grasshopper\UserObjects\SimpleML\myML
   ```
2. 检查环境变量 `SIMPLEML_PATH` 是否设置
3. 确认 Python 代码文件存在

### 问题4: 启动时崩溃

**可能原因**：
- 组件初始化错误
- 依赖项冲突

**解决方案**：
1. 查看 Rhino 错误日志
2. 检查是否有其他插件冲突
3. 尝试移除其他插件，只加载 SimpleML

---

## 修复后的构建步骤

### 1. 重新构建 GHA 文件

**在 Visual Studio 中**：
1. 打开 `SimpleML.csproj`
2. 选择 `Release` 配置
3. `生成` → `生成 SimpleML`
4. 检查 `bin\Release\SimpleML.gha` 是否已创建

### 2. 安装 GHA 文件

1. **复制 GHA 文件**：
   - 从：`bin\Release\SimpleML.gha`
   - 到：`%APPDATA%\Grasshopper\Libraries\SimpleML.gha`

2. **复制 Python 代码**（如果还没有）：
   - 从：`myML` 文件夹（项目根目录的上一级）
   - 到：`%APPDATA%\Grasshopper\UserObjects\SimpleML\myML`

### 3. 重启并验证

1. **完全关闭** Rhino 和 Grasshopper
2. **重新启动** Rhino
3. **打开** Grasshopper
4. **验证**：
   - 在组件面板中搜索 "SimpleML"
   - 应该能看到所有组件
   - 尝试拖拽一个组件到画布

---

## 验证插件已加载

### 方法1: 组件搜索

1. 在 Grasshopper 中按 `Tab` 键
2. 输入 "SimpleML"
3. 应该看到所有 SimpleML 组件

### 方法2: 组件面板

1. 在 Grasshopper 组件面板中
2. 查找 "SimpleML" 分类
3. 应该看到多个子分类：
   - 01 Data Input
   - 02 Data Analysis
   - 03 Dataset
   - 04 Model Training
   - 等等

### 方法3: 关于组件

1. 搜索 "About" 组件
2. 拖拽到画布
3. 如果显示插件信息，说明加载成功

---

## 如果仍然无法加载

### 检查清单

- [ ] GHA 文件在正确位置：`%APPDATA%\Grasshopper\Libraries\`
- [ ] 文件大小不为 0
- [ ] 文件扩展名是 `.gha`（不是 `.dll`）
- [ ] Rhino 8 已安装
- [ ] .NET Framework 4.8 已安装
- [ ] 已重新构建 GHA 文件（使用更新后的代码）
- [ ] 已完全重启 Rhino
- [ ] 检查了 Grasshopper 错误日志

### 获取更多信息

1. **查看 Rhino 日志**：
   - `文件` → `属性` → `日志`
   - 查找 SimpleML 相关错误

2. **使用 .NET 反编译工具**：
   - 使用 ILSpy 或 Reflector 打开 GHA 文件
   - 检查是否包含所有组件类
   - 检查是否包含 `SimpleMLComponentLoader` 类

3. **检查构建输出**：
   - 在 Visual Studio 中查看 `输出` 窗口
   - 确认构建时没有警告或错误

---

## 已修复的问题

### ✅ 添加了组件加载器

**问题**：缺少 `GH_ComponentLoader` 类，导致组件无法注册。

**修复**：
- 已更新 `SimpleMLPlugin.cs`
- 添加了 `SimpleMLComponentLoader` 类
- 注册了所有 38 个组件

### ✅ 确保所有组件正确命名空间

所有组件都已正确组织在命名空间中：
- `SimpleML.Components.DataInput`
- `SimpleML.Components.DataAnalysis`
- `SimpleML.Components.DatasetManagement`
- `SimpleML.Components.ModelTraining`
- `SimpleML.Components.ModelPrediction`
- `SimpleML.Components.ModelEvaluation`
- `SimpleML.Components.ModelManagement`
- `SimpleML.Components.About`

---

## 下一步

1. ✅ **重新构建** GHA 文件（使用更新后的代码）
2. ✅ **替换** 旧的 GHA 文件
3. ✅ **重启** Rhino 和 Grasshopper
4. ✅ **验证** 组件是否显示

如果问题仍然存在，请检查：
- Rhino 版本（需要 Rhino 8）
- .NET Framework 版本（需要 4.8+）
- 其他插件冲突

---

*最后更新：2026-01-27*
