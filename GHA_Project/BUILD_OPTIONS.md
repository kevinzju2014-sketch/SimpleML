# SimpleML GHA 构建选项

## 问题：找不到 .NET SDK

如果遇到 "No .NET SDKs were found" 错误，可以使用以下替代方案：

## 方案1: 使用 Visual Studio（推荐）

### 步骤1: 安装 Visual Studio

下载并安装 Visual Studio 2019 或 2022：
- **Community版**（免费）：https://visualstudio.microsoft.com/vs/community/
- 安装时选择 **".NET 桌面开发"** 工作负载

### 步骤2: 使用 Visual Studio 构建

1. **打开项目**：
   - 双击 `SimpleML.csproj` 文件
   - 或在 Visual Studio 中：`文件` → `打开` → `项目/解决方案`

2. **构建项目**：
   - 选择 `Release` 配置
   - 右键项目 → `生成`
   - 或按 `Ctrl+Shift+B`

3. **复制 GHA 文件**：
   - 构建完成后，将 `bin\Release\SimpleML.dll` 重命名为 `SimpleML.gha`

### 步骤3: 使用 Developer Command Prompt

1. 打开 **Visual Studio Developer Command Prompt**：
   - 开始菜单 → Visual Studio → Developer Command Prompt

2. 进入项目目录：
   ```batch
   cd "D:\Helio\250928_机器学习课程\myML\GHA_Project"
   ```

3. 运行构建脚本：
   ```batch
   build_with_vs.bat
   ```

## 方案2: 使用 Visual Studio Build Tools

如果不想安装完整的 Visual Studio：

1. **下载 Build Tools**：
   - https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022

2. **安装时选择**：
   - ".NET 桌面生成工具"
   - "MSBuild"

3. **使用构建脚本**：
   - 运行 `build.bat`（会自动检测 Build Tools）

## 方案3: 安装 .NET SDK

1. **下载 .NET SDK**：
   - https://dotnet.microsoft.com/download
   - 选择 .NET 6.0 或更高版本

2. **安装后**：
   - 运行 `build.bat`

## 方案4: 手动使用 MSBuild

如果已安装 Visual Studio 或 Build Tools：

1. **找到 MSBuild 路径**：
   - Visual Studio 2022: `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe`
   - Visual Studio 2019: `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`

2. **运行命令**：
   ```batch
   "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" /p:Configuration=Release SimpleML.csproj
   ```

3. **复制文件**：
   ```batch
   copy bin\Release\SimpleML.dll bin\Release\SimpleML.gha
   ```

## 推荐方案

**最简单的方法**：安装 Visual Studio Community（免费），然后：
1. 打开 `SimpleML.csproj`
2. 按 `Ctrl+Shift+B` 构建
3. 重命名 DLL 为 GHA

## 验证安装

构建成功后，应该生成以下文件：
```
bin\Release\SimpleML.dll
bin\Release\SimpleML.gha
```

## 故障排除

### 问题1: 找不到 MSBuild

**解决方案**：
- 确保已安装 Visual Studio 或 Build Tools
- 检查路径是否正确
- 尝试使用 Developer Command Prompt

### 问题2: 找不到 Grasshopper.dll

**解决方案**：
- 检查 Rhino 8 是否已安装
- 确认路径：`C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll`
- 如果路径不同，修改 `SimpleML.csproj` 中的路径

### 问题3: 编译错误

**解决方案**：
- 检查 Visual Studio 错误列表
- 确保所有引用都正确
- 检查 .NET Framework 版本（需要 .NET Framework 4.8）

## 快速检查清单

- [ ] Visual Studio 或 Build Tools 已安装
- [ ] Rhino 8 已安装
- [ ] Grasshopper.dll 路径正确
- [ ] 项目文件可以正常打开
- [ ] 没有编译错误

---

**提示**：如果仍有问题，请查看 Visual Studio 的输出窗口获取详细错误信息。
