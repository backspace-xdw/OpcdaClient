# OPC Components 自动安装脚本
# 需要以管理员身份运行

param(
    [switch]$DownloadOnly = $false
)

Write-Host "=== OPC DA Client 组件安装脚本 ===" -ForegroundColor Cyan
Write-Host ""

# 检查管理员权限
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Host "错误: 此脚本需要管理员权限!" -ForegroundColor Red
    Write-Host "请右键点击 PowerShell，选择'以管理员身份运行'" -ForegroundColor Yellow
    pause
    exit 1
}

# 创建下载目录
$downloadPath = "$env:TEMP\OpcInstall"
if (!(Test-Path $downloadPath)) {
    New-Item -ItemType Directory -Path $downloadPath | Out-Null
}

Write-Host "下载目录: $downloadPath" -ForegroundColor Gray
Write-Host ""

# 下载函数
function Download-File {
    param(
        [string]$url,
        [string]$outputPath
    )
    
    try {
        Write-Host "下载: $url" -ForegroundColor Yellow
        $client = New-Object System.Net.WebClient
        $client.DownloadFile($url, $outputPath)
        Write-Host "完成: $outputPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "下载失败: $_" -ForegroundColor Red
        return $false
    }
}

# 1. 下载 OPC Core Components
Write-Host "1. 准备 OPC Core Components..." -ForegroundColor Cyan
$opcCoreUrl = "https://opcfoundation.org/UA/.NETStack/OPCUA%20Core%20Components%20Redistributable%20(x86).msi"
$opcCorePath = "$downloadPath\OpcCoreComponents_x86.msi"

# 注意：实际URL可能需要更新
Write-Host "提示: 请手动从以下地址下载 OPC Core Components:" -ForegroundColor Yellow
Write-Host "https://opcfoundation.org/products/view/opc-core-components-redistributable" -ForegroundColor White
Write-Host "选择 x86 (32位) 版本" -ForegroundColor White
Write-Host ""

# 2. 下载 Graybox OPC Wrapper (包含模拟器)
Write-Host "2. 下载 Graybox OPC DA Auto Wrapper..." -ForegroundColor Cyan
$grayboxUrl = "http://www.gray-box.net/download/graybox_opc_da_auto_wrapper.msi"
$grayboxPath = "$downloadPath\graybox_opc_da_auto_wrapper.msi"

if (Download-File -url $grayboxUrl -outputPath $grayboxPath) {
    Write-Host "Graybox 下载成功" -ForegroundColor Green
}

if ($DownloadOnly) {
    Write-Host ""
    Write-Host "下载完成。文件位于: $downloadPath" -ForegroundColor Green
    exit 0
}

# 3. 安装组件
Write-Host ""
Write-Host "3. 安装组件..." -ForegroundColor Cyan
Write-Host ""

# 检查并安装 OPC Core Components
if (Test-Path $opcCorePath) {
    Write-Host "安装 OPC Core Components..." -ForegroundColor Yellow
    Start-Process msiexec.exe -ArgumentList "/i `"$opcCorePath`" /quiet /norestart" -Wait
    Write-Host "OPC Core Components 安装完成" -ForegroundColor Green
}
else {
    Write-Host "未找到 OPC Core Components，请手动下载" -ForegroundColor Yellow
}

# 安装 Graybox
if (Test-Path $grayboxPath) {
    Write-Host "安装 Graybox OPC DA Auto Wrapper..." -ForegroundColor Yellow
    Start-Process msiexec.exe -ArgumentList "/i `"$grayboxPath`" /quiet /norestart" -Wait
    Write-Host "Graybox 安装完成" -ForegroundColor Green
}

# 4. 配置 DCOM
Write-Host ""
Write-Host "4. 配置 DCOM 权限..." -ForegroundColor Cyan

# 为 Everyone 添加 DCOM 权限
$dcomCommand = @"
# 启用分布式 COM
reg add "HKLM\SOFTWARE\Microsoft\Ole" /v EnableDCOM /t REG_SZ /d Y /f
reg add "HKLM\SOFTWARE\Microsoft\Ole" /v LegacyAuthenticationLevel /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\Ole" /v LegacyImpersonationLevel /t REG_DWORD /d 2 /f
"@

Invoke-Expression $dcomCommand
Write-Host "DCOM 基本配置完成" -ForegroundColor Green

# 5. 注册 OPC 服务器
Write-Host ""
Write-Host "5. 检查 OPC 服务器注册..." -ForegroundColor Cyan

# 列出已注册的 OPC 服务器
Write-Host "已注册的 OPC 服务器:" -ForegroundColor Yellow
$opcServers = Get-WmiObject -Query "SELECT * FROM Win32_ClassicCOMClassSetting WHERE ProgId LIKE '%OPC%'"
$opcServers | Select-Object -Property ProgId, Description | Format-Table

# 6. 创建测试快捷方式
Write-Host ""
Write-Host "6. 创建测试环境..." -ForegroundColor Cyan

$testScript = @"
# OPC DA Client 测试脚本
Write-Host 'OPC DA Client 测试' -ForegroundColor Cyan
Write-Host '==================='
Write-Host ''
Write-Host '提示：'
Write-Host '1. 确保 Visual Studio 以管理员身份运行'
Write-Host '2. 项目平台设置为 x86'
Write-Host '3. 如果连接失败，检查 Windows 事件日志'
Write-Host ''
pause
"@

$testScriptPath = "$downloadPath\Test-OpcClient.ps1"
$testScript | Out-File -FilePath $testScriptPath -Encoding UTF8

Write-Host ""
Write-Host "=== 安装完成 ===" -ForegroundColor Green
Write-Host ""
Write-Host "下一步操作：" -ForegroundColor Cyan
Write-Host "1. 重启计算机（推荐）"
Write-Host "2. 以管理员身份打开 Visual Studio 2019"
Write-Host "3. 打开 OpcDaClient.sln"
Write-Host "4. 确保平台设置为 x86"
Write-Host "5. 运行项目"
Write-Host ""
Write-Host "测试服务器名称：" -ForegroundColor Yellow
Write-Host "- Graybox.Simulator.1"
Write-Host "- Matrikon.OPC.Simulation.1 (如果已安装)"
Write-Host ""

pause