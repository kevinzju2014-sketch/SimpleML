# 验证 SimpleML 组件是否已加载的脚本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SimpleML 组件加载验证指南" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "请按照以下步骤验证组件是否已加载：" -ForegroundColor Yellow
Write-Host ""

Write-Host "步骤 1: 检查 GHA 文件位置" -ForegroundColor Yellow
$ghaPath = "$env:APPDATA\Grasshopper\Libraries\SimpleML.gha"
if (Test-Path $ghaPath) {
    $fileInfo = Get-Item $ghaPath
    Write-Host "  ✓ GHA 文件存在" -ForegroundColor Green
    Write-Host "    路径: $ghaPath" -ForegroundColor Gray
    Write-Host "    大小: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "    修改时间: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "  ✗ GHA 文件不存在！" -ForegroundColor Red
    Write-Host "    需要将文件复制到: $ghaPath" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "步骤 2: 在 Grasshopper 中验证组件" -ForegroundColor Yellow
Write-Host ""
Write-Host "  方法 1: 使用搜索功能" -ForegroundColor Cyan
Write-Host "    1. 打开 Rhino 和 Grasshopper" -ForegroundColor White
Write-Host "    2. 在 Grasshopper 画布上双击空白处（或按 Tab 键）" -ForegroundColor White
Write-Host "    3. 输入以下任一组件名称：" -ForegroundColor White
Write-Host "       - Read CSV" -ForegroundColor Gray
Write-Host "       - About" -ForegroundColor Gray
Write-Host "       - SimpleML" -ForegroundColor Gray
Write-Host "    4. 如果组件出现在搜索结果中 → 组件已加载 ✓" -ForegroundColor Green
Write-Host "    5. 如果组件不出现 → 组件未加载 ✗" -ForegroundColor Red
Write-Host ""

Write-Host "  方法 2: 检查标签栏" -ForegroundColor Cyan
Write-Host "    1. 在 Grasshopper 组件面板顶部查看标签栏" -ForegroundColor White
Write-Host "    2. 查找 'SimpleML' 标签" -ForegroundColor White
Write-Host "    3. 如果看到 SimpleML 标签 → 组件已加载并显示 ✓" -ForegroundColor Green
Write-Host "    4. 如果看不到 SimpleML 标签 → 可能是 Tab Layout 问题" -ForegroundColor Yellow
Write-Host ""

Write-Host "步骤 3: 如果组件已加载但不在标签栏中" -ForegroundColor Yellow
Write-Host ""
Write-Host "  这是 Tab Layout 问题，解决方法：" -ForegroundColor Cyan
Write-Host "    1. 在 Grasshopper 中，右键点击标签栏区域（组件面板顶部）" -ForegroundColor White
Write-Host "    2. 选择 'Reset Tab Layout' 或 'Default Tab Layout'" -ForegroundColor White
Write-Host "    3. 或者选择 'Show All Tabs'（如果可用）" -ForegroundColor White
Write-Host "    4. SimpleML 标签应该会显示出来" -ForegroundColor White
Write-Host ""

Write-Host "步骤 4: 如果组件完全找不到" -ForegroundColor Yellow
Write-Host ""
Write-Host "  检查以下内容：" -ForegroundColor Cyan
Write-Host "    1. 检查 Rhino 命令行窗口的错误信息" -ForegroundColor White
Write-Host "    2. 确认 GHA 文件在正确位置（见步骤 1）" -ForegroundColor White
Write-Host "    3. 完全关闭 Rhino，重新启动" -ForegroundColor White
Write-Host "    4. 如果问题持续，重新构建项目并重新安装 GHA 文件" -ForegroundColor White
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "验证完成" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "提示：" -ForegroundColor Yellow
Write-Host "  - 即使组件不在标签栏中，只要能在搜索中找到，就可以使用" -ForegroundColor Gray
Write-Host "  - 使用 Tab 键或双击画布可以快速搜索组件" -ForegroundColor Gray
Write-Host ""
