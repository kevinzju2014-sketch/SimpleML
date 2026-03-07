# 快速修复指南 - GHA 插件无法加载

## 当前状态
- ✅ GHA 文件已生成（405KB，大小正常）
- ❌ Grasshopper 无法加载插件

## 🔧 快速修复步骤

### 步骤 1: 运行诊断脚本
```powershell
cd "D:\Helio\250928_机器学习课程\myML\GHA_Project"
.\diagnose_gha_loading.ps1
```

### 步骤 2: 确保文件在正确位置

**检查文件位置**:
- 源文件: `D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha`
- 目标位置: `%APPDATA%\Grasshopper\Libraries\SimpleML.gha`

**快速打开目标文件夹**:
- 按 `Win + R`
- 输入: `%APPDATA%\Grasshopper\Libraries`
- 按回车

### 步骤 3: 完全关闭 Rhino

**重要**: 必须完全关闭所有 Rhino 进程！

1. 关闭所有 Rhino 窗口
2. 检查任务管理器，确保没有 `Rhino.exe` 或 `Grasshopper.exe` 进程
3. 如果有，结束这些进程

### 步骤 4: 复制/更新 GHA 文件

**方法 1: 手动复制**
1. 打开源文件夹: `D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release`
2. 复制 `SimpleML.gha` 文件
3. 打开目标文件夹: `%APPDATA%\Grasshopper\Libraries`
4. 如果已存在 `SimpleML.gha`，先删除它
5. 粘贴新文件

**方法 2: 使用 PowerShell**
```powershell
# 完全关闭 Rhino（如果正在运行）
Get-Process -Name "Rhino*" -ErrorAction SilentlyContinue | Stop-Process -Force

# 等待 2 秒
Start-Sleep -Seconds 2

# 复制文件
$source = "D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha"
$target = "$env:APPDATA\Grasshopper\Libraries\SimpleML.gha"

# 确保目标文件夹存在
$targetDir = Split-Path $target
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}

# 删除旧文件（如果存在）
if (Test-Path $target) {
    Remove-Item $target -Force
}

# 复制新文件
Copy-Item $source $target -Force

Write-Host "文件已复制到: $target" -ForegroundColor Green
```

### 步骤 5: 清除 Grasshopper 缓存（可选）

如果文件已正确复制但仍无法加载，尝试清除缓存：

```powershell
# Grasshopper 缓存位置
$cachePath = "$env:APPDATA\Grasshopper"

# 查找缓存文件
$cacheFiles = Get-ChildItem -Path $cachePath -Recurse -Filter "*.cache" -ErrorAction SilentlyContinue
if ($cacheFiles.Count -gt 0) {
    Write-Host "找到 $($cacheFiles.Count) 个缓存文件" -ForegroundColor Yellow
    # 注意：删除缓存可能会影响其他设置，请谨慎操作
}
```

### 步骤 6: 重新启动并验证

1. **重新启动 Rhino**
2. **打开 Grasshopper**
3. **验证插件加载**:
   - 按 `Tab` 键
   - 输入 "SimpleML"
   - 应该能看到所有组件

### 步骤 7: 检查错误日志

如果仍然无法加载：

1. **查看 Rhino 命令行窗口**（Rhino 底部）
   - 启动 Grasshopper 时查看是否有错误信息
   - 常见错误：
     - `Failed to load assembly: SimpleML`
     - `Could not load type: SimpleMLPlugin`
     - `无法加载程序集`

2. **检查 Grasshopper 日志**:
   - 打开 Grasshopper
   - `文件` → `特殊文件夹` → `组件文件夹`
   - 查看是否有加载错误

## 🚨 常见问题

### 问题 1: 文件已复制但组件不显示

**可能原因**:
- 文件被锁定（Rhino 仍在运行）
- 文件损坏
- 缓存问题

**解决方案**:
1. 确保完全关闭 Rhino
2. 删除目标文件，重新复制
3. 重启 Rhino

### 问题 2: 显示"无法加载程序集"

**可能原因**:
- 缺少依赖项
- .NET Framework 版本不匹配
- 文件损坏

**解决方案**:
1. 检查 Rhino 8 是否已安装
2. 检查 .NET Framework 4.8 是否已安装
3. 重新构建 GHA 文件

### 问题 3: 文件大小不匹配

**解决方案**:
- 确保复制的是最新构建的文件
- 检查源文件和目标文件的大小是否相同
- 如果不同，重新复制

## 📋 检查清单

在报告问题前，请确认：

- [ ] Rhino 8 已安装
- [ ] .NET Framework 4.8 已安装
- [ ] 完全关闭了 Rhino 和 Grasshopper
- [ ] GHA 文件在正确位置: `%APPDATA%\Grasshopper\Libraries\SimpleML.gha`
- [ ] 文件大小正确（约 405KB）
- [ ] 文件扩展名是 `.gha`（不是 `.dll`）
- [ ] 已重新启动 Rhino
- [ ] 检查了 Rhino 命令行窗口的错误信息

## 🔍 高级诊断

如果上述步骤都无法解决问题，请：

1. **使用 ILSpy 检查 GHA 文件**:
   - 下载: https://github.com/icsharpcode/ILSpy
   - 打开 `SimpleML.gha`
   - 检查是否包含所有组件类

2. **检查项目配置**:
   - 确认 `SimpleML.csproj` 中的引用路径正确
   - 确认目标框架是 .NET Framework 4.8

3. **尝试最小化测试**:
   - 创建一个最简单的测试组件
   - 构建并测试是否能加载

---

*最后更新：2026-01-27*
