# 修复 GUID 和文件路径问题的脚本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "修复 GUID 和文件路径问题" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 步骤 1: 关闭 Rhino
Write-Host "步骤 1: 关闭 Rhino..." -ForegroundColor Yellow
$rhinoProcesses = Get-Process -Name "Rhino*" -ErrorAction SilentlyContinue
if ($rhinoProcesses.Count -gt 0) {
    Write-Host "  发现 $($rhinoProcesses.Count) 个 Rhino 进程，正在关闭..." -ForegroundColor Gray
    $rhinoProcesses | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "  ✓ Rhino 已关闭" -ForegroundColor Green
} else {
    Write-Host "  ✓ Rhino 未运行" -ForegroundColor Green
}
Write-Host ""

# 步骤 2: 删除错误位置的文件
Write-Host "步骤 2: 删除错误位置的文件..." -ForegroundColor Yellow
$wrongPath = "$env:APPDATA\Grasshopper\Libraries\Release\SimpleML.gha"
if (Test-Path $wrongPath) {
    Remove-Item $wrongPath -Force
    Write-Host "  ✓ 已删除错误位置的文件: $wrongPath" -ForegroundColor Green
} else {
    Write-Host "  ✓ 错误位置没有文件" -ForegroundColor Green
}

# 删除 Release 文件夹（如果为空）
$releaseFolder = "$env:APPDATA\Grasshopper\Libraries\Release"
if (Test-Path $releaseFolder) {
    try {
        Remove-Item $releaseFolder -Force -Recurse -ErrorAction SilentlyContinue
        Write-Host "  ✓ 已删除 Release 文件夹" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠ 无法删除 Release 文件夹（可能不为空）" -ForegroundColor Yellow
    }
}
Write-Host ""

# 步骤 3: 复制到正确位置
Write-Host "步骤 3: 复制文件到正确位置..." -ForegroundColor Yellow
$source = "D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha"
$target = "$env:APPDATA\Grasshopper\Libraries\SimpleML.gha"

# 检查源文件
if (-not (Test-Path $source)) {
    Write-Host "  ✗ 源文件不存在: $source" -ForegroundColor Red
    Write-Host "    请先重新构建项目（使用修复后的 GUID）" -ForegroundColor Yellow
    exit 1
}

Write-Host "  源文件: $source" -ForegroundColor Gray
Write-Host "  目标: $target" -ForegroundColor Gray

# 确保目标文件夹存在
$targetDir = Split-Path $target
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    Write-Host "  ✓ 已创建目标文件夹" -ForegroundColor Green
}

# 删除旧文件（如果存在）
if (Test-Path $target) {
    Remove-Item $target -Force
    Write-Host "  ✓ 已删除旧文件" -ForegroundColor Green
}

# 复制新文件
Copy-Item $source $target -Force
Write-Host "  ✓ 文件已复制到正确位置" -ForegroundColor Green
Write-Host ""

# 验证
Write-Host "步骤 4: 验证文件..." -ForegroundColor Yellow
if (Test-Path $target) {
    $fileInfo = Get-Item $target
    Write-Host "  ✓ 文件存在" -ForegroundColor Green
    Write-Host "    位置: $target" -ForegroundColor Gray
    Write-Host "    大小: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "    修改时间: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "  ✗ 文件复制失败！" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 完成
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "修复完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "下一步操作:" -ForegroundColor Yellow
Write-Host "1. 重新启动 Rhino" -ForegroundColor White
Write-Host "2. 打开 Grasshopper" -ForegroundColor White
Write-Host "3. 在画布上双击（或按 Tab 键）" -ForegroundColor White
Write-Host "4. 搜索 'Test SimpleML' 或 'About'" -ForegroundColor White
Write-Host "5. 应该能找到组件了！" -ForegroundColor Green
Write-Host ""
Write-Host "重要提示:" -ForegroundColor Yellow
Write-Host "- 确保已重新构建项目（使用修复后的 GUID）" -ForegroundColor Gray
Write-Host "- 文件必须在 Libraries 根目录，不在子文件夹中" -ForegroundColor Gray
Write-Host ""
