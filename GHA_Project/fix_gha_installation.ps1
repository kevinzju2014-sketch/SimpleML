# 自动修复 GHA 文件安装脚本
# 用于自动复制 SimpleML.gha 到正确位置

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SimpleML GHA 文件自动安装工具" -ForegroundColor Cyan
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
Write-Host ""

# 步骤 2: 检查 Rhino 是否正在运行
Write-Host "步骤 2: 检查 Rhino 进程..." -ForegroundColor Yellow
$rhinoProcesses = Get-Process -Name "Rhino*" -ErrorAction SilentlyContinue
if ($rhinoProcesses.Count -gt 0) {
    Write-Host "  ⚠ 发现 $($rhinoProcesses.Count) 个 Rhino 进程正在运行" -ForegroundColor Yellow
    Write-Host "    需要关闭 Rhino 才能更新插件" -ForegroundColor Yellow
    Write-Host ""
    
    $response = Read-Host "是否要关闭所有 Rhino 进程？(Y/N)"
    if ($response -eq "Y" -or $response -eq "y") {
        Write-Host "  正在关闭 Rhino 进程..." -ForegroundColor Gray
        $rhinoProcesses | Stop-Process -Force
        Start-Sleep -Seconds 2
        Write-Host "  ✓ Rhino 进程已关闭" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ 请手动关闭 Rhino 后重新运行此脚本" -ForegroundColor Yellow
        exit 1
    }
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

# 步骤 4: 检查并删除旧文件
Write-Host "步骤 4: 检查旧文件..." -ForegroundColor Yellow
if (Test-Path $targetFile) {
    $oldFile = Get-Item $targetFile
    Write-Host "  发现旧文件:" -ForegroundColor Gray
    Write-Host "    大小: $([math]::Round($oldFile.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "    修改时间: $($oldFile.LastWriteTime)" -ForegroundColor Gray
    
    # 检查文件是否被锁定
    try {
        $fileStream = [System.IO.File]::Open($targetFile, 'Open', 'ReadWrite', 'None')
        $fileStream.Close()
        Write-Host "  ✓ 文件未被锁定，可以删除" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ 文件被锁定！" -ForegroundColor Red
        Write-Host "    请确保完全关闭 Rhino 和 Grasshopper" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "  删除旧文件..." -ForegroundColor Gray
    Remove-Item $targetFile -Force
    Write-Host "  ✓ 旧文件已删除" -ForegroundColor Green
} else {
    Write-Host "  ✓ 目标位置没有旧文件" -ForegroundColor Green
}
Write-Host ""

# 步骤 5: 复制新文件
Write-Host "步骤 5: 复制新文件..." -ForegroundColor Yellow
try {
    Copy-Item $sourceFile $targetFile -Force
    Write-Host "  ✓ 文件已复制" -ForegroundColor Green
    Write-Host "    目标: $targetFile" -ForegroundColor Gray
    
    # 验证文件
    $newFile = Get-Item $targetFile
    Write-Host "    大小: $([math]::Round($newFile.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "    修改时间: $($newFile.LastWriteTime)" -ForegroundColor Gray
    
    # 检查文件大小是否匹配
    if ($sourceInfo.Length -eq $newFile.Length) {
        Write-Host "  ✓ 文件大小匹配" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ 文件大小不匹配！" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ✗ 复制失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 步骤 6: 检查是否有其他 SimpleML 相关文件
Write-Host "步骤 6: 检查其他相关文件..." -ForegroundColor Yellow
$otherFiles = Get-ChildItem -Path $targetDir -Filter "*SimpleML*" -ErrorAction SilentlyContinue
$dllFiles = $otherFiles | Where-Object { $_.Extension -eq ".dll" }
if ($dllFiles.Count -gt 0) {
    Write-Host "  ⚠ 发现 .dll 文件（应该只有 .gha 文件）:" -ForegroundColor Yellow
    foreach ($file in $dllFiles) {
        Write-Host "    - $($file.Name)" -ForegroundColor Gray
    }
    Write-Host "    建议：删除这些 .dll 文件，只保留 .gha 文件" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ 没有发现多余的 .dll 文件" -ForegroundColor Green
}
Write-Host ""

# 完成
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "安装完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "下一步:" -ForegroundColor Yellow
Write-Host "1. 启动 Rhino" -ForegroundColor White
Write-Host "2. 打开 Grasshopper" -ForegroundColor White
Write-Host "3. 按 Tab 键，输入 'SimpleML' 搜索组件" -ForegroundColor White
Write-Host "4. 如果仍然无法加载，请检查 Rhino 命令行窗口的错误信息" -ForegroundColor White
Write-Host ""
