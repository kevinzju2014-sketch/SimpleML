# 修复 GUID 格式错误和文件路径问题

## 发现的问题

### 问题 1: GUID 格式错误
**错误信息**：`Guid string should only contain hexadecimal characters.`

**原因**：`TestComponent` 的 GUID 格式不正确：
```csharp
// 错误 ❌
new Guid("TEST-COMPONENT-GUID-123456789012")

// 正确 ✅
new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")
```

GUID 只能包含十六进制字符（0-9, A-F），不能包含 "TEST" 这样的字母。

**已修复**：✅ 已更新 `TestComponent.cs`，使用正确的 GUID 格式。

### 问题 2: 文件路径错误
**错误路径**：`C:\Users\Administrator\AppData\Roaming\Grasshopper\Libraries\Release\SimpleML.gha`

**正确路径**：`C:\Users\Administrator\AppData\Roaming\Grasshopper\Libraries\SimpleML.gha`

**问题**：文件被放在了 `Libraries\Release\` 子文件夹中，应该在 `Libraries\` 根目录。

## 修复步骤

### 步骤 1: 重新构建项目

1. 在 Visual Studio 中：
   - `生成` → `清理解决方案`
   - `生成` → `重新生成解决方案`
   - 确认没有错误

2. 验证 GHA 文件：
   - 位置：`bin\Release\SimpleML.gha`
   - 大小：应该约 405KB

### 步骤 2: 修复文件位置

**删除错误位置的文件**：
```
C:\Users\Administrator\AppData\Roaming\Grasshopper\Libraries\Release\SimpleML.gha
```

**复制到正确位置**：
```
从: D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha
到: C:\Users\Administrator\AppData\Roaming\Grasshopper\Libraries\SimpleML.gha
```

**快速操作**：
1. 按 `Win + R`
2. 输入：`%APPDATA%\Grasshopper\Libraries`
3. 按回车
4. 删除 `Release` 文件夹（如果存在）
5. 确保 `SimpleML.gha` 直接在 `Libraries` 文件夹中

### 步骤 3: 验证修复

1. **完全关闭 Rhino**
2. **重新启动 Rhino**
3. **打开 Grasshopper**
4. **搜索组件**：
   - 按 `Tab` 键或双击画布
   - 输入 "Test SimpleML" 或 "About"
   - 应该能找到组件了！

## 自动修复脚本

运行以下 PowerShell 脚本自动修复：

```powershell
# 关闭 Rhino
Get-Process -Name "Rhino*" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

# 删除错误位置的文件
$wrongPath = "$env:APPDATA\Grasshopper\Libraries\Release\SimpleML.gha"
if (Test-Path $wrongPath) {
    Remove-Item $wrongPath -Force
    Write-Host "已删除错误位置的文件" -ForegroundColor Green
}

# 删除 Release 文件夹（如果为空）
$releaseFolder = "$env:APPDATA\Grasshopper\Libraries\Release"
if (Test-Path $releaseFolder) {
    Remove-Item $releaseFolder -Force -ErrorAction SilentlyContinue
}

# 复制到正确位置
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

Write-Host "GHA 文件已复制到正确位置: $target" -ForegroundColor Green
Write-Host "请重新启动 Rhino" -ForegroundColor Yellow
```

## 验证清单

修复后，确认：

- [ ] 重新构建了项目（使用修复后的 GUID）
- [ ] 删除了错误位置的文件（`Libraries\Release\`）
- [ ] GHA 文件在正确位置（`Libraries\SimpleML.gha`）
- [ ] 完全关闭并重新启动了 Rhino
- [ ] 在 Grasshopper 中能搜索到组件

## 已修复的内容

1. ✅ **TestComponent GUID**：修复为正确的十六进制格式
2. ✅ **文件位置**：确保文件在 `Libraries\` 根目录，不在子文件夹中

---

*最后更新：2026-01-27*
