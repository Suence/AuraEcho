# PowerLab 构建脚本

$bundleProject = "./PowerLab.Installer.Bundle.wixproj"

$Config = Read-Host "请输入构建配置 [1: Release (默认), 2: Debug]"
if ($Config -eq "2") { $Config = "Debug" } else { $Config = "Release" }

$Arch = Read-Host "请输入目标架构 [1: x64 (默认), 2: x86]"
if ($Arch -eq "2") { $Arch = "x86" } else { $Arch = "x64" }

$Mode = Read-Host "请输入构建模式 [1: Build (增量, 默认), 2: Rebuild (全量)]"
$isRebuild = if ($Mode -eq "2") { $true } else { $false }

$BundleVersion = "1.0.0"
$AppVersion = "1.0.0"
$DataMigratorVersion = "1.0.0"
$LauncherServiceVersion = "1.0.0"
$PluginInstallerVersion = "1.0.0"
$UpdaterServiceVersion = "1.0.0"

Write-Host "`n--------------------------------------------------" -ForegroundColor Gray
Write-Host "确认构建任务:" -ForegroundColor Cyan
Write-Host ">> 配置: $Config"
Write-Host ">> 架构: $Arch"
Write-Host ">> 模式: $(if($isRebuild){"Rebuild"}else{"Build"})"
Write-Host "--------------------------------------------------`n" -ForegroundColor Gray

if ($isRebuild) {
    Write-Host "正在清理旧的构建产物..." -ForegroundColor DarkGray
    dotnet clean $bundleProject -c $Config -p:Platform=$Arch
}

$buildArgs = @(
    $bundleProject,
    "-c", $Config,
    "-p:Platform=$Arch",
#    "/NoWarn:CS8618",
    "-p:WarningLevel=0",
    "-p:BundleVersion=$BundleVersion",
    "-p:AppVersion=$AppVersion",
    "-p:DataMigratorVersion=$DataMigratorVersion",
    "-p:LauncherServiceVersion=$LauncherServiceVersion",
    "-p:PluginInstallerVersion=$PluginInstallerVersion",
    "-p:UpdaterServiceVersion=$UpdaterServiceVersion"
)

dotnet build @buildArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[SUCCESS] 构建成功！" -ForegroundColor Green
} else {
    Write-Host "`n[ERROR] 构建失败。" -ForegroundColor Red
    exit 1
}
