# SimpleML GHA 构建脚本 (PowerShell)
# 用于重新生成 GHA 文件

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SimpleML GHA Builder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = Join-Path $PSScriptRoot "SimpleML.csproj"
$outputPath = Join-Path $PSScriptRoot "bin\Release"
$dllPath = Join-Path $outputPath "SimpleML.dll"
$ghaPath = Join-Path $outputPath "SimpleML.gha"

# 检查项目文件是否存在
if (-not (Test-Path $projectPath)) {
    Write-Host "错误: 找不到项目文件: $projectPath" -ForegroundColor Red
    exit 1
}

Write-Host "项目文件: $projectPath" -ForegroundColor Green
Write-Host ""

# 查找 MSBuild
$msbuildPath = $null

# 尝试 Visual Studio 2022
$vs2022Paths = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)

foreach ($path in $vs2022Paths) {
    if (Test-Path $path) {
        $msbuildPath = $path
        break
    }
}

# 尝试 Visual Studio 2019
if (-not $msbuildPath) {
    $vs2019Paths = @(
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    )
    
    foreach ($path in $vs2019Paths) {
        if (Test-Path $path) {
            $msbuildPath = $path
            break
        }
    }
}

# 尝试使用 dotnet build
if (-not $msbuildPath) {
    $dotnetPath = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnetPath) {
        Write-Host "使用 .NET SDK 构建..." -ForegroundColor Yellow
        Write-Host ""
        
        Write-Host "[1/2] 构建项目..." -ForegroundColor Cyan
        & dotnet build $projectPath -c Release
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "构建失败!" -ForegroundColor Red
            exit 1
        }
        
        Write-Host ""
        goto :create_gha
    }
}

# 使用 MSBuild
if ($msbuildPath) {
    Write-Host "找到 MSBuild: $msbuildPath" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "[1/2] 构建项目..." -ForegroundColor Cyan
    & $msbuildPath $projectPath /p:Configuration=Release /p:Platform=AnyCPU /v:minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "构建失败!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
} else {
    Write-Host "错误: 找不到 MSBuild 或 .NET SDK!" -ForegroundColor Red
    Write-Host ""
    Write-Host "请安装以下之一:" -ForegroundColor Yellow
    Write-Host "1. Visual Studio 2019/2022"
    Write-Host "2. Visual Studio Build Tools 2022"
    Write-Host "3. .NET SDK 6.0 或更高版本"
    Write-Host ""
    exit 1
}

:create_gha
# 创建 GHA 文件
Write-Host "[2/2] 创建 GHA 文件..." -ForegroundColor Cyan

if (Test-Path $dllPath) {
    Copy-Item $dllPath $ghaPath -Force
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "构建成功完成!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "GHA 文件位置: $ghaPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "安装说明:" -ForegroundColor Cyan
    Write-Host "1. 复制 SimpleML.gha 到: %APPDATA%\Grasshopper\Libraries\" -ForegroundColor White
    Write-Host "2. 复制 myML 文件夹到: %APPDATA%\Grasshopper\UserObjects\SimpleML\myML" -ForegroundColor White
    Write-Host "3. 重启 Grasshopper" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "错误: 找不到 DLL 文件!" -ForegroundColor Red
    Write-Host "预期位置: $dllPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "请检查:" -ForegroundColor Yellow
    Write-Host "1. 构建是否成功完成"
    Write-Host "2. 输出目录是否存在"
    Write-Host "3. 文件权限"
    Write-Host ""
    exit 1
}
