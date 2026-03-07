# 生成GHA文件指南

## 前置条件检查

### 1. 图标文件准备

运行以下脚本准备图标文件：
```bash
python prepare_icons.py
```

然后验证图标文件：
```bash
python verify_icons.py
```

确保所有23个必需图标文件都存在。

### 2. 项目配置检查

- ✅ `SimpleML.csproj` 已配置图标资源
- ✅ 所有组件已更新图标加载代码
- ✅ `IconLoader.cs` 和 `ComponentIconMap.cs` 已创建

## 构建GHA文件

### 方法1：使用Visual Studio（推荐）

1. 打开 `SimpleML.csproj` 文件
2. 选择 **Release** 配置
3. 右键项目 → **生成** (Build)
4. GHA文件将自动生成在 `bin\Release\SimpleML.gha`

### 方法2：使用MSBuild命令行

```batch
cd "D:\Helio\250928_机器学习课程\myML\GHA_Project"
msbuild SimpleML.csproj /p:Configuration=Release /t:Build
```

### 方法3：使用项目中的构建脚本

```batch
cd "D:\Helio\250928_机器学习课程\myML\GHA_Project"
build.bat
```

## 构建输出

构建成功后，GHA文件位置：
```
bin\Release\SimpleML.gha
```

## 安装GHA文件

1. 将 `SimpleML.gha` 文件复制到 Grasshopper 的组件库目录：
   - 默认位置：`C:\Users\[用户名]\AppData\Roaming\Grasshopper\Libraries\`
   - 或：`C:\ProgramData\Grasshopper\Libraries\`

2. 重启 Grasshopper 或使用 `File → Special Folders → Components Folder` 查看

3. 在 Grasshopper 中，组件应该出现在 **SimpleML** 分类下

## 验证安装

1. 打开 Grasshopper
2. 查看组件面板，应该能看到 SimpleML 分类
3. 检查组件是否显示图标
4. 测试一个简单的组件（如 Read CSV）

## 故障排除

### 图标未显示
- 检查图标文件是否在 `icons` 目录中
- 检查 `.csproj` 文件中的路径是否正确（`icons\*.png`）
- 检查组件代码中的图标加载逻辑

### 构建失败
- 检查所有组件文件是否包含 `using SimpleML.Core;`
- 检查 `IconLoader.cs` 和 `ComponentIconMap.cs` 是否正确编译
- 检查是否有语法错误

### GHA文件未生成
- 检查构建输出目录 `bin\Release\`
- 检查构建日志中的错误信息
- 确保构建配置为 Release

## 下一步

构建完成后：
1. ✅ 验证GHA文件已生成
2. ✅ 安装到Grasshopper
3. ✅ 测试组件功能
4. ✅ 检查图标显示
