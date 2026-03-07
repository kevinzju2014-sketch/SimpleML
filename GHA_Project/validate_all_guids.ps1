# 验证所有组件的 GUID 格式

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "验证所有组件的 GUID 格式" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "D:\Helio\250928_机器学习课程\myML\GHA_Project"
$componentFiles = Get-ChildItem -Path "$projectPath\Components" -Filter "*.cs" -Recurse
$componentFiles += Get-ChildItem -Path $projectPath -Filter "*Component.cs" -File

$errors = @()
$valid = @()

foreach ($file in $componentFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # 查找所有 GUID
    if ($content -match 'new Guid\("([^"]+)"\)') {
        $guids = [regex]::Matches($content, 'new Guid\("([^"]+)"\)')
        
        foreach ($match in $guids) {
            $guidString = $match.Groups[1].Value
            
            # 检查 GUID 格式
            # GUID 应该只包含十六进制字符：0-9, A-F, 和连字符
            $invalidChars = $guidString -replace '[0-9A-Fa-f\-]', ''
            
            if ($invalidChars.Length -gt 0) {
                $errors += [PSCustomObject]@{
                    File = $file.Name
                    GUID = $guidString
                    InvalidChars = $invalidChars
                }
            } else {
                # 验证 GUID 格式是否正确（32个十六进制字符，用连字符分隔）
                $guidWithoutDashes = $guidString -replace '-', ''
                if ($guidWithoutDashes.Length -eq 32) {
                    $valid += [PSCustomObject]@{
                        File = $file.Name
                        GUID = $guidString
                    }
                } else {
                    $errors += [PSCustomObject]@{
                        File = $file.Name
                        GUID = $guidString
                        InvalidChars = "长度不正确（应该是32个十六进制字符）"
                    }
                }
            }
        }
    }
}

Write-Host "验证结果:" -ForegroundColor Yellow
Write-Host ""

if ($errors.Count -gt 0) {
    Write-Host "发现 $($errors.Count) 个错误的 GUID:" -ForegroundColor Red
    Write-Host ""
    foreach ($error in $errors) {
        Write-Host "  文件: $($error.File)" -ForegroundColor Yellow
        Write-Host "  GUID: $($error.GUID)" -ForegroundColor Gray
        Write-Host "  问题: 包含无效字符 '$($error.InvalidChars)'" -ForegroundColor Red
        Write-Host ""
    }
} else {
    Write-Host "  ✓ 所有 GUID 格式正确！" -ForegroundColor Green
    Write-Host "  验证了 $($valid.Count) 个 GUID" -ForegroundColor Gray
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "验证完成" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($errors.Count -gt 0) {
    Write-Host "请修复上述错误的 GUID，然后重新构建项目。" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "所有 GUID 都正确，可以继续构建项目。" -ForegroundColor Green
}
