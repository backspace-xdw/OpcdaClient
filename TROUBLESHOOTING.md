# OPC DA Client 故障排除指南

## 快速检查清单

- [ ] Visual Studio 以**管理员身份**运行
- [ ] 项目平台设置为 **x86** (不是 Any CPU)
- [ ] 已安装 OPC Core Components (32位)
- [ ] 至少安装了一个 OPC 服务器
- [ ] Windows 防火墙允许 OPC 通信

## 常见错误及解决方案

### 1. Class not registered (0x80040154)

**原因**: COM 组件未注册或平台不匹配

**解决方案**:
```powershell
# 1. 确认项目是 x86
# 2. 重新注册 OPC 组件
regsvr32 "C:\Windows\SysWOW64\opcproxy.dll"
regsvr32 "C:\Windows\SysWOW64\opccomn_ps.dll"
```

### 2. Access is denied (0x80070005)

**原因**: DCOM 权限问题

**解决方案**:
1. 运行 `dcomcnfg.exe`
2. 组件服务 → 计算机 → 我的电脑 → 右键属性
3. 默认属性标签：
   - 默认身份验证级别：无
   - 默认模拟级别：标识
4. COM 安全标签：
   - 编辑默认权限
   - 为你的用户账户添加所有权限

### 3. The RPC server is unavailable (0x800706BA)

**原因**: OPC 服务器未运行或防火墙阻止

**解决方案**:
```powershell
# 1. 检查服务
Get-Service | Where-Object {$_.Name -like "*OPC*"}

# 2. 添加防火墙例外
netsh advfirewall firewall add rule name="OPC DA" dir=in action=allow protocol=TCP localport=135
```

### 4. Cannot find OPC Server

**原因**: 服务器名称错误或未注册

**解决方案**:
```csharp
// 使用 OPC 枚举器查找可用服务器
Type typeofOPCserver = Type.GetTypeFromProgID("OPC.ServerList");
dynamic opcServerList = Activator.CreateInstance(typeofOPCserver);
Array servers = opcServerList.GetOPCServers();
foreach (string server in servers)
{
    Console.WriteLine($"Found: {server}");
}
```

### 5. BadImageFormatException

**原因**: 32/64 位不匹配

**解决方案**:
- 项目属性 → 生成 → 平台目标 → x86
- 不要使用 "Any CPU"

## 诊断工具

### 1. OPC Expert
免费的 OPC 客户端，用于测试连接
- 如果 OPC Expert 能连接，但你的程序不能，说明是代码问题
- 如果 OPC Expert 也不能连接，说明是环境问题

### 2. Process Monitor
监控程序访问注册表和文件
```
过滤器设置：
- Process Name contains "YourApp"
- Operation is "RegOpenKey" or "RegQueryValue"
```

### 3. Event Viewer
查看详细错误信息
- Windows 日志 → 应用程序
- Windows 日志 → 系统
- 应用程序和服务日志 → Microsoft → Windows → DCOM

### 4. DCOM 配置检查脚本
```powershell
# 检查 DCOM 设置
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Ole" | Format-List

# 列出所有 OPC 相关的 COM 组件
Get-WmiObject Win32_ClassicCOMClassSetting | Where-Object {$_.ProgId -like "*OPC*"} | Select-Object ProgId, CLSID
```

## 调试技巧

### 1. 启用详细日志
```csharp
// 在 OpcDaClient 构造函数中添加
System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("opc_debug.log"));
System.Diagnostics.Trace.AutoFlush = true;
```

### 2. 测试本地连接
```csharp
// 先测试本地连接
client.Connect("YourServer", "localhost");

// 如果成功，再测试远程
client.Connect("YourServer", "RemotePC");
```

### 3. 使用 try-catch 获取详细信息
```csharp
try
{
    client.Connect(serverName, host);
}
catch (COMException comEx)
{
    Console.WriteLine($"COM Error: 0x{comEx.ErrorCode:X8}");
    Console.WriteLine($"Message: {comEx.Message}");
    
    // 查找 Windows 错误描述
    var win32Ex = new System.ComponentModel.Win32Exception(comEx.ErrorCode);
    Console.WriteLine($"Windows Error: {win32Ex.Message}");
}
```

## 环境验证脚本

创建 `Verify-OpcEnvironment.ps1`:
```powershell
Write-Host "=== OPC 环境验证 ===" -ForegroundColor Cyan

# 1. 检查 OS 版本
$os = Get-WmiObject Win32_OperatingSystem
Write-Host "OS: $($os.Caption) $($os.OSArchitecture)"

# 2. 检查 .NET Framework
$dotnet = Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release -ErrorAction SilentlyContinue
Write-Host ".NET Framework: $($dotnet.Release)"

# 3. 检查 OPC 组件
$opcDlls = @(
    "$env:WINDIR\System32\opcproxy.dll",
    "$env:WINDIR\System32\opccomn_ps.dll",
    "$env:WINDIR\SysWOW64\opcproxy.dll",
    "$env:WINDIR\SysWOW64\opccomn_ps.dll"
)

foreach ($dll in $opcDlls) {
    if (Test-Path $dll) {
        Write-Host "Found: $dll" -ForegroundColor Green
    } else {
        Write-Host "Missing: $dll" -ForegroundColor Red
    }
}

# 4. 检查 DCOM
$dcom = Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Ole" -Name EnableDCOM
Write-Host "DCOM Enabled: $($dcom.EnableDCOM)"

# 5. 列出 OPC 服务器
Write-Host "`nRegistered OPC Servers:" -ForegroundColor Yellow
$servers = Get-WmiObject -Query "SELECT * FROM Win32_ClassicCOMClassSetting WHERE ProgId LIKE '%OPC%' AND ProgId NOT LIKE '%OPC.%'"
$servers | Select-Object -Property ProgId | Format-Table
```

## 如果所有方法都失败

1. **使用 OPC UA 替代**
   - OPC UA 是新标准，跨平台，更容易配置
   - NuGet: `OPCFoundation.NetStandard.Opc.Ua`

2. **使用 OPC Gateway**
   - 如 Kepware OPC Gateway
   - 将 OPC DA 转换为其他协议

3. **虚拟机测试**
   - 在干净的 Windows 10 虚拟机中测试
   - 这样可以排除系统配置问题

记住：OPC DA 是 1990 年代的技术，基于 COM/DCOM，配置复杂是正常的。保持耐心！