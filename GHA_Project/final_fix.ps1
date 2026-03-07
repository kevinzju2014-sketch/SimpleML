# 最终修复脚本 - 完全重新安装 GHA 文件

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SimpleML GHA 文件最终修复工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 配置路径
$sourceFile = "D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha"
$targetFile = "$env:APPDATA\Grasshopper\Libraries\SimpleML.gha"
$targetDir = Split-Path $targetFile

# 步骤 1: 检查源文件
Write-Host "步骤 1: 检查源文件..." -ForegroundColor Yellow
if (-not (Test-Path $sourceFile)) {
    Write-Host "  ✗ 源文件不存在: $sourceFile" -ForegroundColor Red
    Write-Host "    请先构建项目！" -ForegroundColor Yellow
    exit 1
}

$sourceInfo = Get-Item $sourceFile
Write-Host "  ✓ 源文件存在" -ForegroundColor Green
Write-Host "    路径: $sourceFile" -ForegroundColor Gray
Write-Host "    大小: $([math]::Round($sourceInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
Write-Host "    修改时间: $($sourceInfo.LastWriteTime)" -ForegroundColor Gray

if ($sourceInfo.Length -lt 100KB) {
    Write-Host "  ⚠ 警告: 文件大小较小，可能组件没有被包含" -ForegroundColor Yellow
}
Write-Host ""

# 步骤 2: 关闭 Rhino
Write-Host "步骤 2: 检查并关闭 Rhino..." -ForegroundColor Yellow
$rhinoProcesses = Get-Process -Name "Rhino*" -ErrorAction SilentlyContinue
if ($rhinoProcesses.Count -gt 0) {
    Write-Host "  发现 $($rhinoProcesses.Count) 个 Rhino 进程" -ForegroundColor Yellow
    Write-Host "  正在关闭..." -ForegroundColor Gray
    $rhinoProcesses | Stop-Process -Force
    Start-Sleep -Seconds 3
    Write-Host "  ✓ Rhino 进程已关闭" -ForegroundColor Green
} else {
    Write-Host "  ✓ Rhino 未运行" -ForegroundColor Green
}
Write-Host ""

# 步骤 3: 确保目标文件夹存在
Write-Host "步骤 3: 检查目标文件夹..." -ForegroundColor Yellow
if (-not (Test-Path $targetDir)) {
    Write-Host "  创建目标文件夹: $targetDir" -ForegroundColor Gray
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    Write-Host "  ✓ 文件夹已创建" -ForegroundColor Green
} else {
    Write-Host "  ✓ 目标文件夹存在: $targetDir" -ForegroundColor Green
}
Write-Host ""

# 步骤 4: 删除旧文件
Write-Host "步骤 4: 删除旧文件..." -ForegroundColor Yellow
if (Test-Path $targetFile) {
    try {
        Remove-Item $targetFile -Force
        Write-Host "  ✓ 旧文件已删除" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ 无法删除旧文件: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "    请手动删除: $targetFile" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "  ✓ 目标位置没有旧文件" -ForegroundColor Green
}
Write-Host ""

# 步骤 5: 复制新文件
Write-Host "步骤 5: 复制新文件..." -ForegroundColor Yellow
try {
    Copy-Item $sourceFile $targetFile -Force
    Write-Host "  ✓ 文件已复制" -ForegroundColor Green
    
    # 验证
    $targetInfo = Get-Item $targetFile
    Write-Host "    目标: $targetFile" -ForegroundColor Gray
    Write-Host "    大小: $([math]::Round($targetInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "    修改时间: $($targetInfo.LastWriteTime)" -ForegroundColor Gray
    
    # 检查大小是否匹配
    if ($sourceInfo.Length -eq $targetInfo.Length) {
        Write-Host "  ✓ 文件大小匹配" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ 文件大小不匹配！" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ✗ 复制失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 步骤 6: 检查是否有其他相关文件
Write-Host "步骤 6: 检查其他相关文件..." -ForegroundColor Yellow
$otherFiles = Get-ChildItem -Path $targetDir -Filter "*SimpleML*" -ErrorAction SilentlyContinue
$dllFiles = $otherFiles | Where-Object { $_.Extension -eq ".dll" }
if ($dllFiles.Count -gt 0) {
    Write-Host "  ⚠ 发现 .dll 文件（应该只有 .gha 文件）:" -ForegroundColor Yellow
    foreach ($file in $dllFiles) {
        Write-Host "    - $($file.Name)" -ForegroundColor Gray
    }
    Write-Host "    建议：删除这些 .dll 文件" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ 没有多余的 .dll 文件" -ForegroundColor Green
}
Write-Host ""

# 完成
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "安装完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "下一步操作:" -ForegroundColor Yellow
Write-Host "1. 启动 Rhino" -ForegroundColor White
Write-Host "2. 打开 Grasshopper" -ForegroundColor White
Write-Host "3. 在画布上双击（或按 Tab 键）" -ForegroundColor White
Write-Host "4. 搜索 'Test SimpleML' 或 'About'" -ForegroundColor White
Write-Host "5. 如果组件出现，说明已成功加载 ✓" -ForegroundColor Green
Write-Host "6. 如果组件不出现，请检查 Rhino 命令行窗口的错误信息" -ForegroundColor Yellow
Write-Host ""
Write-Host "重要提示:" -ForegroundColor Yellow
Write-Host "- 如果组件仍然搜索不到，请使用 ILSpy 检查 GHA 文件内容" -ForegroundColor Gray
Write-Host "- 确认 GHA 文件中包含所有组件类" -ForegroundColor Gray
Write-Host "- 查看 Rhino 命令行窗口是否有错误信息" -ForegroundColor Gray
Write-Host ""
