# 构建说明

## 快速构建

在 Windows 上双击运行 `Build.cmd` 即可自动构建项目。

## 手动构建步骤

### 1. 使用 Visual Studio 2019

1. 打开 `OpcDaClient.sln`
2. 在工具栏选择：
   - 配置：Release 或 Debug
   - 平台：x86（重要！）
3. 菜单：生成 → 生成解决方案
4. 或按快捷键：Ctrl+Shift+B

### 2. 使用命令行

```cmd
# 打开开发者命令提示符
# 开始菜单 → Visual Studio 2019 → Developer Command Prompt for VS 2019

# 导航到项目目录
cd path\to\OpcDaClient

# 构建
msbuild OpcDaClient.sln /p:Configuration=Release /p:Platform=x86
```

### 3. 使用 .NET CLI（如果安装了 .NET SDK）

```cmd
# 还原依赖
dotnet restore

# 构建
dotnet build -c Release
```

## 构建输出

成功构建后，可执行文件位于：
- Debug 版本：`bin\x86\Debug\OpcDaClient.exe`
- Release 版本：`bin\x86\Release\OpcDaClient.exe`

## 构建要求

- **必须**：.NET Framework 4.7.2 或更高版本
- **必须**：构建平台设置为 x86（32位）
- **推荐**：Visual Studio 2019 或更高版本
- **可选**：NuGet 命令行工具

## 常见构建错误

### 错误：找不到 .NET Framework 4.7.2

**解决方案**：
1. 下载 .NET Framework 4.7.2 Developer Pack
2. 安装后重启 Visual Studio

### 错误：无法解析 COM 引用

**解决方案**：
1. 确保安装了 OPC Core Components
2. 项目会自动生成 COM 互操作程序集

### 错误：平台不匹配

**解决方案**：
1. 确保所有项目都设置为 x86
2. 不要使用 "Any CPU"

## GitHub Actions 自动构建

项目已配置 GitHub Actions，每次推送都会自动构建。查看构建状态：
https://github.com/backspace-xdw/OpcdaClient/actions

## 验证构建

构建成功后，运行以下命令测试：

```cmd
# 在输出目录运行
cd bin\x86\Release
OpcDaClient.exe

# 应该看到输出：
# === OPC DA Client Example ===
# Attempting to connect to OPC Server...
```

如果看到连接错误，这是正常的（需要安装 OPC 服务器）。只要程序能启动，说明构建成功。