# 检查所有组件的 GUID 是否有重复

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "检查重复的 GUID" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "D:\Helio\250928_机器学习课程\myML\GHA_Project"
$componentFiles = Get-ChildItem -Path "$projectPath\Components" -Filter "*.cs" -Recurse
$componentFiles += Get-ChildItem -Path $projectPath -Filter "*Component.cs" -File

$guidMap = @{}

foreach ($file in $componentFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # 查找组件类名
    if ($content -match 'public class (\w+Component)') {
        $className = $matches[1]
    } else {
        continue
    }
    
    # 查找 GUID
    if ($content -match 'ComponentGuid.*new Guid\("([^"]+)"\)') {
        $guid = $matches[1]
        
        if ($guidMap.ContainsKey($guid)) {
            Write-Host "✗ 发现重复的 GUID: $guid" -ForegroundColor Red
            Write-Host "  组件1: $($guidMap[$guid])" -ForegroundColor Yellow
            Write-Host "  组件2: $className ($($file.Name))" -ForegroundColor Yellow
            Write-Host ""
        } else {
            $guidMap[$guid] = "$className ($($file.Name))"
        }
    }
}

Write-Host "验证结果:" -ForegroundColor Yellow
Write-Host "  检查了 $($componentFiles.Count) 个组件文件" -ForegroundColor Gray
Write-Host "  找到了 $($guidMap.Count) 个唯一的 GUID" -ForegroundColor Gray
Write-Host ""

$duplicates = @()
foreach ($guid in $guidMap.Keys) {
    $components = @()
    foreach ($file in $componentFiles) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match 'ComponentGuid.*new Guid\("' + [regex]::Escape($guid) + '"\)') {
            if ($content -match 'public class (\w+Component)') {
                $components += $matches[1]
            }
        }
    }
    if ($components.Count -gt 1) {
        $duplicates += [PSCustomObject]@{
            GUID = $guid
            Components = $components
        }
    }
}

if ($duplicates.Count -gt 0) {
    Write-Host "发现 $($duplicates.Count) 个重复的 GUID:" -ForegroundColor Red
    Write-Host ""
    foreach ($dup in $duplicates) {
        Write-Host "  GUID: $($dup.GUID)" -ForegroundColor Yellow
        foreach ($comp in $dup.Components) {
            Write-Host "    - $comp" -ForegroundColor Gray
        }
        Write-Host ""
    }
    Write-Host "请修复这些重复的 GUID！" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✓ 所有 GUID 都是唯一的！" -ForegroundColor Green
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "检查完成" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
