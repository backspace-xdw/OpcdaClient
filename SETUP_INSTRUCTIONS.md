# OPC DA Client 设置说明

## Visual Studio 2019 配置要求

### 1. 项目设置
- **目标框架**: .NET Framework 4.7.2
- **平台目标**: x86 (32位) - **这是必须的！**
- **语言版本**: C# 7.3

### 2. 不需要 NuGet 包！
这个 OPC DA Client 使用的是 Windows COM 组件，**不需要安装任何 NuGet 包**。

### 3. 必需的系统组件

在运行此项目之前，你需要确保系统中安装了以下组件之一：

#### 选项 A: OPC Expert 或 OPC Core Components
1. 下载并安装 **OPC Core Components Redistributable** (免费)
   - 下载地址：https://www.opcexpert.com/downloads/software/opc-core-components-redistributable-x64/
   - 选择 x86 (32位) 版本
   - 这会安装必需的 OPCAutomation.dll

#### 选项 B: 使用已安装的 OPC 服务器
如果你已经安装了任何 OPC 服务器软件（如 KEPServerEX、Matrikon OPC Server 等），通常它们会自带 OPCAutomation 组件。

### 4. Visual Studio 2019 中的设置步骤

1. **打开项目**
   ```
   打开 OpcDaClient.sln
   ```

2. **确认平台设置**
   - 在 Visual Studio 顶部，确保平台选择为 "x86" 而不是 "Any CPU"
   - 如果没有 x86 选项：
     - 点击配置管理器
     - 在平台列下拉选择 <新建...>
     - 选择 x86

3. **COM 引用会自动添加**
   - 项目文件已经包含了 OPCAutomation COM 引用
   - Visual Studio 会自动生成互操作程序集

4. **构建项目**
   - 直接按 F5 或点击"启动"即可

### 5. 常见问题解决

#### 问题：找不到 OPCAutomation
**解决方案**：
1. 安装 OPC Core Components（见上面的选项 A）
2. 或者手动添加 COM 引用：
   - 右键项目 → 添加 → 引用
   - 选择 COM 标签
   - 搜索 "OPC Automation 2.0"
   - 勾选并确定

#### 问题：BadImageFormatException 错误
**解决方案**：
- 确保项目设置为 x86 平台
- 不要使用 Any CPU

#### 问题：拒绝访问错误
**解决方案**：
- 以管理员身份运行 Visual Studio
- 确保当前用户有 DCOM 权限

### 6. 测试 OPC 服务器

推荐的测试用 OPC 服务器：
1. **Matrikon OPC Simulation Server** (免费)
   - 下载地址：https://www.matrikonopc.com/opc-server/index.aspx
   - 服务器名称：`Matrikon.OPC.Simulation.1`

2. **Graybox OPC DA Auto Wrapper** (免费)
   - 包含测试服务器
   - 服务器名称：`Graybox.Simulator.1`

### 7. 运行示例

项目包含的 Program.cs 已经有完整的示例代码：
```csharp
// 连接到 Matrikon 模拟服务器
client.Connect("Matrikon.OPC.Simulation.1", "localhost");

// 读取数据
var value = client.ReadItem("Random.Int4");
```

## 总结

1. **不需要 NuGet 包**
2. **必须使用 x86 平台**
3. **需要安装 OPC Core Components 或 OPC 服务器**
4. **以管理员身份运行可能需要**

如果遇到任何问题，请检查：
- 是否安装了 OPC 组件
- 是否设置为 x86 平台
- 是否有足够的权限