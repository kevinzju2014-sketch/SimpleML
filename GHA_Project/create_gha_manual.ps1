# 手动创建 GHA 文件的脚本
# 如果自动构建没有生成 .gha 文件，可以使用此脚本手动创建

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "手动创建 GHA 文件工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "D:\Helio\250928_机器学习课程\myML\GHA_Project"
$dllPath = Join-Path $projectPath "bin\Release\SimpleML.dll"
$ghaPath = Join-Path $projectPath "bin\Release\SimpleML.gha"

# 检查 Debug 配置
$dllPathDebug = Join-Path $projectPath "bin\Debug\SimpleML.dll"
$ghaPathDebug = Join-Path $projectPath "bin\Debug\SimpleML.gha"

Write-Host "查找 DLL 文件..." -ForegroundColor Yellow

# 优先检查 Release
if (Test-Path $dllPath) {
    Write-Host "  ✓ 找到 Release DLL: $dllPath" -ForegroundColor Green
    $sourceDll = $dllPath
    $targetGha = $ghaPath
} elseif (Test-Path $dllPathDebug) {
    Write-Host "  ✓ 找到 Debug DLL: $dllPathDebug" -ForegroundColor Green
    $sourceDll = $dllPathDebug
    $targetGha = $ghaPathDebug
} else {
    Write-Host "  ✗ 找不到 DLL 文件！" -ForegroundColor Red
    Write-Host "    请先构建项目（Release 或 Debug 配置）" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "DLL 文件信息:" -ForegroundColor Yellow
$dllInfo = Get-Item $sourceDll
Write-Host "  路径: $sourceDll" -ForegroundColor Gray
Write-Host "  大小: $([math]::Round($dllInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
Write-Host "  修改时间: $($dllInfo.LastWriteTime)" -ForegroundColor Gray
Write-Host ""

# 创建 GHA 文件
Write-Host "创建 GHA 文件..." -ForegroundColor Yellow
try {
    # 如果 GHA 文件已存在，先删除
    if (Test-Path $targetGha) {
        Write-Host "  删除旧的 GHA 文件..." -ForegroundColor Gray
        Remove-Item $targetGha -Force
    }
    
    # 复制 DLL 为 GHA
    Copy-Item $sourceDll $targetGha -Force
    Write-Host "  ✓ GHA 文件已创建" -ForegroundColor Green
    
    # 验证
    $ghaInfo = Get-Item $targetGha
    Write-Host ""
    Write-Host "GHA 文件信息:" -ForegroundColor Yellow
    Write-Host "  路径: $targetGha" -ForegroundColor Gray
    Write-Host "  大小: $([math]::Round($ghaInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "  修改时间: $($ghaInfo.LastWriteTime)" -ForegroundColor Gray
    
    # 检查大小是否匹配
    if ($dllInfo.Length -eq $ghaInfo.Length) {
        Write-Host "  ✓ 文件大小匹配" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ 文件大小不匹配！" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "完成！GHA 文件已创建" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "下一步:" -ForegroundColor Yellow
    Write-Host "1. 将 GHA 文件复制到: %APPDATA%\Grasshopper\Libraries\" -ForegroundColor White
    Write-Host "2. 完全关闭 Rhino" -ForegroundColor White
    Write-Host "3. 重新启动 Rhino 并打开 Grasshopper" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host "  ✗ 创建失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
