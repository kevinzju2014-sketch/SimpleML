# 验证 GHA 文件内容的脚本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GHA 文件内容验证工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ghaPath = "D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha"

if (-not (Test-Path $ghaPath)) {
    Write-Host "✗ GHA 文件不存在: $ghaPath" -ForegroundColor Red
    Write-Host "  请先构建项目！" -ForegroundColor Yellow
    exit 1
}

Write-Host "GHA 文件信息:" -ForegroundColor Yellow
$fileInfo = Get-Item $ghaPath
Write-Host "  路径: $ghaPath" -ForegroundColor Gray
Write-Host "  大小: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
Write-Host "  修改时间: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
Write-Host ""

if ($fileInfo.Length -lt 100KB) {
    Write-Host "  ⚠ 警告: 文件大小较小，可能组件没有被包含" -ForegroundColor Yellow
    Write-Host "    正常大小应该约为 400-500 KB" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ 文件大小正常" -ForegroundColor Green
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "必须使用 ILSpy 检查文件内容" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "步骤 1: 下载 ILSpy" -ForegroundColor Yellow
Write-Host "  下载地址: https://github.com/icsharpcode/ILSpy/releases" -ForegroundColor White
Write-Host "  或搜索: ILSpy GitHub" -ForegroundColor White
Write-Host ""

Write-Host "步骤 2: 打开 GHA 文件" -ForegroundColor Yellow
Write-Host "  1. 打开 ILSpy" -ForegroundColor White
Write-Host "  2. 文件 → 打开 → 选择以下文件:" -ForegroundColor White
Write-Host "     $ghaPath" -ForegroundColor Gray
Write-Host ""

Write-Host "步骤 3: 检查以下类是否存在" -ForegroundColor Yellow
Write-Host ""
Write-Host "  必须存在的类:" -ForegroundColor Cyan
Write-Host "    ✓ SimpleML.SimpleMLPlugin" -ForegroundColor Green
Write-Host "      - 继承自: GH_AssemblyInfo" -ForegroundColor Gray
Write-Host "      - 命名空间: SimpleML" -ForegroundColor Gray
Write-Host ""
Write-Host "    ✓ SimpleML.TestComponent" -ForegroundColor Green
Write-Host "      - 继承自: GH_Component" -ForegroundColor Gray
Write-Host "      - 命名空间: SimpleML" -ForegroundColor Gray
Write-Host ""
Write-Host "    ✓ SimpleML.Components.About.AboutComponent" -ForegroundColor Green
Write-Host "      - 继承自: GH_Component" -ForegroundColor Gray
Write-Host "      - 命名空间: SimpleML.Components.About" -ForegroundColor Gray
Write-Host ""
Write-Host "    ✓ SimpleML.Components.DataInput.ReadCSVComponent" -ForegroundColor Green
Write-Host "      - 继承自: GH_Component" -ForegroundColor Gray
Write-Host "      - 命名空间: SimpleML.Components.DataInput" -ForegroundColor Gray
Write-Host ""

Write-Host "步骤 4: 检查结果" -ForegroundColor Yellow
Write-Host ""
Write-Host "  如果所有类都存在:" -ForegroundColor Cyan
Write-Host "    → 说明组件已正确编译到程序集中" -ForegroundColor Green
Write-Host "    → 问题可能在加载或注册阶段" -ForegroundColor Yellow
Write-Host "    → 检查 Rhino 错误日志" -ForegroundColor Yellow
Write-Host ""
Write-Host "  如果类不存在:" -ForegroundColor Cyan
Write-Host "    → 说明组件没有被编译到程序集中" -ForegroundColor Red
Write-Host "    → 检查组件类实现" -ForegroundColor Yellow
Write-Host "    → 检查编译错误" -ForegroundColor Yellow
Write-Host ""

Write-Host "步骤 5: 检查 Rhino 错误日志" -ForegroundColor Yellow
Write-Host "  1. 打开 Rhino" -ForegroundColor White
Write-Host "  2. 查看底部命令行窗口" -ForegroundColor White
Write-Host "  3. 启动 Grasshopper 时查看错误信息" -ForegroundColor White
Write-Host "  4. 记录所有错误信息" -ForegroundColor White
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "诊断完成" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "请使用 ILSpy 检查 GHA 文件，然后告诉我结果。" -ForegroundColor Yellow
Write-Host ""
