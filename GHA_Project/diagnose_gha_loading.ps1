# GHA 文件加载诊断脚本
# 用于检查 SimpleML.gha 插件的安装和加载问题

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SimpleML GHA 插件加载诊断工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 检查 GHA 文件位置
Write-Host "1. 检查 GHA 文件位置..." -ForegroundColor Yellow
$ghaSource = "D:\Helio\250928_机器学习课程\myML\GHA_Project\bin\Release\SimpleML.gha"
$ghaTarget = "$env:APPDATA\Grasshopper\Libraries\SimpleML.gha"

Write-Host "  源文件位置: $ghaSource" -ForegroundColor Gray
if (Test-Path $ghaSource) {
    $sourceInfo = Get-Item $ghaSource
    Write-Host "  ✓ 源文件存在" -ForegroundColor Green
    Write-Host "    大小: $([math]::Round($sourceInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "    修改时间: $($sourceInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "  ✗ 源文件不存在！" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "  目标文件位置: $ghaTarget" -ForegroundColor Gray
if (Test-Path $ghaTarget) {
    $targetInfo = Get-Item $ghaTarget
    Write-Host "  ✓ 目标文件存在" -ForegroundColor Green
    Write-Host "    大小: $([math]::Round($targetInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "    修改时间: $($targetInfo.LastWriteTime)" -ForegroundColor Gray
    
    # 检查文件是否被锁定
    Write-Host ""
    Write-Host "  检查文件是否被锁定..." -ForegroundColor Gray
    try {
        $fileStream = [System.IO.File]::Open($ghaTarget, 'Open', 'ReadWrite', 'None')
        $fileStream.Close()
        Write-Host "  ✓ 文件未被锁定" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ 文件被锁定！可能正在被 Rhino/Grasshopper 使用" -ForegroundColor Red
        Write-Host "    请完全关闭 Rhino 和 Grasshopper 后重试" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ✗ 目标文件不存在！" -ForegroundColor Red
    Write-Host "    需要将文件复制到: $ghaTarget" -ForegroundColor Yellow
}

# 2. 检查文件大小是否匹配
Write-Host ""
Write-Host "2. 检查文件大小..." -ForegroundColor Yellow
if ((Test-Path $ghaSource) -and (Test-Path $ghaTarget)) {
    $sourceSize = (Get-Item $ghaSource).Length
    $targetSize = (Get-Item $ghaTarget).Length
    if ($sourceSize -eq $targetSize) {
        Write-Host "  ✓ 文件大小匹配" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ 文件大小不匹配！" -ForegroundColor Yellow
        Write-Host "    源文件: $sourceSize 字节" -ForegroundColor Gray
        Write-Host "    目标文件: $targetSize 字节" -ForegroundColor Gray
        Write-Host "    建议：重新复制文件" -ForegroundColor Yellow
    }
}

# 3. 检查 Grasshopper Libraries 文件夹
Write-Host ""
Write-Host "3. 检查 Grasshopper Libraries 文件夹..." -ForegroundColor Yellow
$librariesPath = "$env:APPDATA\Grasshopper\Libraries"
if (Test-Path $librariesPath) {
    Write-Host "  ✓ Libraries 文件夹存在: $librariesPath" -ForegroundColor Green
    
    # 列出所有 .gha 文件
    $ghaFiles = Get-ChildItem -Path $librariesPath -Filter "*.gha" -ErrorAction SilentlyContinue
    Write-Host "  找到 $($ghaFiles.Count) 个 GHA 文件:" -ForegroundColor Gray
    foreach ($file in $ghaFiles) {
        $status = if ($file.Name -eq "SimpleML.gha") { "← 这是 SimpleML" } else { "" }
        Write-Host "    - $($file.Name) ($([math]::Round($file.Length / 1KB, 2)) KB) $status" -ForegroundColor Gray
    }
} else {
    Write-Host "  ✗ Libraries 文件夹不存在！" -ForegroundColor Red
    Write-Host "    创建文件夹: $librariesPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $librariesPath -Force | Out-Null
    Write-Host "  ✓ 已创建文件夹" -ForegroundColor Green
}

# 4. 检查是否有其他 SimpleML 相关文件
Write-Host ""
Write-Host "4. 检查是否有其他 SimpleML 相关文件..." -ForegroundColor Yellow
$simpleMLFiles = Get-ChildItem -Path $librariesPath -Filter "*SimpleML*" -ErrorAction SilentlyContinue
if ($simpleMLFiles.Count -gt 0) {
    Write-Host "  找到以下文件:" -ForegroundColor Gray
    foreach ($file in $simpleMLFiles) {
        Write-Host "    - $($file.Name) ($([math]::Round($file.Length / 1KB, 2)) KB)" -ForegroundColor Gray
    }
    
    # 检查是否有 .dll 文件（应该只有 .gha）
    $dllFiles = $simpleMLFiles | Where-Object { $_.Extension -eq ".dll" }
    if ($dllFiles.Count -gt 0) {
        Write-Host "  ⚠ 发现 .dll 文件！应该只有 .gha 文件" -ForegroundColor Yellow
        Write-Host "    建议删除 .dll 文件，只保留 .gha 文件" -ForegroundColor Yellow
    }
}

# 5. 检查 Rhino 是否正在运行
Write-Host ""
Write-Host "5. 检查 Rhino 进程..." -ForegroundColor Yellow
$rhinoProcesses = Get-Process -Name "Rhino*" -ErrorAction SilentlyContinue
if ($rhinoProcesses.Count -gt 0) {
    Write-Host "  ⚠ Rhino 正在运行！" -ForegroundColor Yellow
    Write-Host "    找到 $($rhinoProcesses.Count) 个 Rhino 进程:" -ForegroundColor Gray
    foreach ($proc in $rhinoProcesses) {
        Write-Host "    - $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Gray
    }
    Write-Host "    建议：完全关闭 Rhino 后再安装/更新插件" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ Rhino 未运行" -ForegroundColor Green
}

# 6. 检查 .NET Framework 版本
Write-Host ""
Write-Host "6. 检查 .NET Framework 版本..." -ForegroundColor Yellow
$netFramework = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -ErrorAction SilentlyContinue
if ($netFramework) {
    $version = $netFramework.Version
    Write-Host "  ✓ .NET Framework 版本: $version" -ForegroundColor Green
    if ([version]$version -ge [version]"4.8.0") {
        Write-Host "  ✓ 版本符合要求（需要 4.8+）" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ 版本可能过低（建议 4.8+）" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ✗ 无法检测 .NET Framework 版本" -ForegroundColor Red
}

# 7. 检查 Rhino 8 安装路径
Write-Host ""
Write-Host "7. 检查 Rhino 8 安装路径..." -ForegroundColor Yellow
$rhinoPath = "C:\Program Files\Rhino 8"
if (Test-Path $rhinoPath) {
    Write-Host "  ✓ Rhino 8 路径存在: $rhinoPath" -ForegroundColor Green
    
    # 检查依赖项
    $grasshopperDll = "$rhinoPath\Plug-ins\Grasshopper\Grasshopper.dll"
    $ghIoDll = "$rhinoPath\Plug-ins\Grasshopper\GH_IO.dll"
    $rhinoCommonDll = "$rhinoPath\System\RhinoCommon.dll"
    
    Write-Host "  检查依赖项..." -ForegroundColor Gray
    if (Test-Path $grasshopperDll) {
        Write-Host "    ✓ Grasshopper.dll" -ForegroundColor Green
    } else {
        Write-Host "    ✗ Grasshopper.dll 不存在" -ForegroundColor Red
    }
    
    if (Test-Path $ghIoDll) {
        Write-Host "    ✓ GH_IO.dll" -ForegroundColor Green
    } else {
        Write-Host "    ✗ GH_IO.dll 不存在" -ForegroundColor Red
    }
    
    if (Test-Path $rhinoCommonDll) {
        Write-Host "    ✓ RhinoCommon.dll" -ForegroundColor Green
    } else {
        Write-Host "    ✗ RhinoCommon.dll 不存在" -ForegroundColor Red
    }
} else {
    Write-Host "  ⚠ Rhino 8 路径不存在: $rhinoPath" -ForegroundColor Yellow
    Write-Host "    如果 Rhino 安装在其他位置，请检查项目文件中的引用路径" -ForegroundColor Yellow
}

# 8. 提供修复建议
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "修复建议" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $ghaTarget)) {
    Write-Host "1. 复制 GHA 文件到目标位置:" -ForegroundColor Yellow
    Write-Host "   Copy-Item '$ghaSource' '$ghaTarget' -Force" -ForegroundColor White
    Write-Host ""
}

if ($rhinoProcesses.Count -gt 0) {
    Write-Host "2. 完全关闭 Rhino 和 Grasshopper" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "3. 重新启动 Rhino 并打开 Grasshopper" -ForegroundColor Yellow
Write-Host ""

Write-Host "4. 在 Grasshopper 中验证插件:" -ForegroundColor Yellow
Write-Host "   - 按 Tab 键，输入 'SimpleML'" -ForegroundColor White
Write-Host "   - 应该能看到所有组件" -ForegroundColor White
Write-Host ""

Write-Host "5. 如果仍然无法加载，检查 Rhino 命令行窗口的错误信息" -ForegroundColor Yellow
Write-Host ""

Write-Host "诊断完成！" -ForegroundColor Green
