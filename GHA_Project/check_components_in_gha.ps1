# 检查 GHA 文件中是否包含组件类的脚本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "检查 GHA 文件中的组件" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ghaPath = "D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha"

if (-not (Test-Path $ghaPath)) {
    Write-Host "✗ GHA 文件不存在: $ghaPath" -ForegroundColor Red
    Write-Host "  请先构建项目" -ForegroundColor Yellow
    exit 1
}

Write-Host "GHA 文件信息:" -ForegroundColor Yellow
$fileInfo = Get-Item $ghaPath
Write-Host "  路径: $ghaPath" -ForegroundColor Gray
Write-Host "  大小: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
Write-Host "  修改时间: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
Write-Host ""

Write-Host "检查方法:" -ForegroundColor Yellow
Write-Host ""
Write-Host "方法 1: 使用 ILSpy（推荐）" -ForegroundColor Cyan
Write-Host "  1. 下载 ILSpy: https://github.com/icsharpcode/ILSpy/releases" -ForegroundColor White
Write-Host "  2. 打开 ILSpy" -ForegroundColor White
Write-Host "  3. 文件 → 打开 → 选择 SimpleML.gha" -ForegroundColor White
Write-Host "  4. 展开程序集，查看是否包含：" -ForegroundColor White
Write-Host "     - SimpleMLPlugin 类" -ForegroundColor Gray
Write-Host "     - TestComponent 类（如果已添加）" -ForegroundColor Gray
Write-Host "     - ReadCSVComponent 类" -ForegroundColor Gray
Write-Host "     - AboutComponent 类" -ForegroundColor Gray
Write-Host "     - 其他组件类" -ForegroundColor Gray
Write-Host ""

Write-Host "方法 2: 使用 .NET 反编译工具" -ForegroundColor Cyan
Write-Host "  使用任何 .NET 反编译工具（如 dotPeek, Reflector）" -ForegroundColor White
Write-Host "  打开 GHA 文件并检查类列表" -ForegroundColor White
Write-Host ""

Write-Host "方法 3: 检查文件大小" -ForegroundColor Cyan
Write-Host "  如果文件大小约为 405KB，说明可能包含了所有组件" -ForegroundColor White
Write-Host "  如果文件很小（< 50KB），可能组件没有被包含" -ForegroundColor Yellow
Write-Host ""

Write-Host "预期结果:" -ForegroundColor Yellow
Write-Host "  ✓ 应该能看到所有组件类" -ForegroundColor Green
Write-Host "  ✓ 每个组件类都应该有 ComponentGuid 属性" -ForegroundColor Green
Write-Host "  ✓ 应该能看到 SimpleMLPlugin 类" -ForegroundColor Green
Write-Host ""

Write-Host "如果组件类不在 GHA 文件中:" -ForegroundColor Yellow
Write-Host "  1. 检查 Visual Studio 中的错误列表" -ForegroundColor White
Write-Host "  2. 确认所有组件文件都在项目中（不是灰色的）" -ForegroundColor White
Write-Host "  3. 重新构建项目" -ForegroundColor White
Write-Host "  4. 检查构建输出是否有错误" -ForegroundColor White
Write-Host ""
