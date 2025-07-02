# Windows 安装和调试指南

## 步骤 1：下载必需组件

### 1.1 OPC Core Components
1. 访问：https://opcfoundation.org/products/view/opc-core-components-redistributable
2. 或者使用 Graybox 提供的版本：
   - http://gray-box.net/download_daawrapper.php?lang=en
3. 选择 **x86 (32位)** 版本下载

### 1.2 测试用 OPC 服务器（选择其一）

#### 选项 A：Matrikon OPC Simulation Server（推荐）
- 下载地址：https://www.matrikonopc.com/opc-server/software-downloads/
- 免费，功能齐全，适合测试

#### 选项 B：Graybox Simulator
- 随 Graybox OPC DA Auto Wrapper 一起安装
- 更轻量级

## 步骤 2：安装顺序

### 2.1 安装 OPC Core Components
1. **以管理员身份运行**安装程序
2. 选择"Complete"安装
3. 安装完成后重启计算机

### 2.2 安装测试 OPC 服务器
1. 安装 Matrikon OPC Simulation Server
2. 安装过程中选择"Register OPC Server"
3. 记住服务器名称：`Matrikon.OPC.Simulation.1`

## 步骤 3：配置 DCOM（重要！）

1. **运行 dcomcnfg.exe**（以管理员身份）
2. 展开"组件服务" → "计算机" → "我的电脑" → "DCOM 配置"
3. 找到你的 OPC 服务器（如 "Matrikon.OPC.Simulation"）
4. 右键 → 属性 → 安全标签
5. 在"启动和激活权限"和"访问权限"中：
   - 选择"自定义"
   - 点击"编辑"
   - 添加"Everyone"或你的用户账户
   - 勾选所有权限

## 步骤 4：在 Visual Studio 2019 中打开项目

### 4.1 打开解决方案
1. 启动 Visual Studio 2019 **以管理员身份**
2. 打开 `OpcDaClient.sln`

### 4.2 检查项目配置
1. 右键项目 → 属性
2. 确认：
   - 目标框架：.NET Framework 4.7.2
   - 平台目标：x86

### 4.3 检查 COM 引用
1. 展开"引用"节点
2. 应该能看到 "OPCAutomation"
3. 如果没有：
   - 右键"引用" → "添加引用"
   - 选择"COM"标签
   - 搜索"OPC Automation 2.0"
   - 勾选并确定

## 步骤 5：修改测试代码

打开 `Program.cs`，确保服务器名称正确：

```csharp
static void Main(string[] args)
{
    try
    {
        using (IOpcDaClient client = new OpcDaClient())
        {
            Console.WriteLine("Connecting to OPC Server...");
            
            // 使用实际安装的服务器名称
            client.Connect("Matrikon.OPC.Simulation.1", "localhost");
            
            Console.WriteLine("Connected successfully!");
            
            // 浏览可用的项目
            Console.WriteLine("\nBrowsing available items:");
            var items = client.BrowseItems();
            foreach (var item in items.Take(10))
            {
                Console.WriteLine($"  {item.ItemId}");
            }
            
            // 读取一些值
            Console.WriteLine("\nReading values:");
            var testItems = new[] { 
                "Random.Int1", 
                "Random.Int2", 
                "Random.Real4", 
                "Random.String" 
            };
            
            foreach (var itemId in testItems)
            {
                try
                {
                    var value = client.ReadItem(itemId);
                    Console.WriteLine($"  {itemId} = {value}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {itemId} - Error: {ex.Message}");
                }
            }
            
            // 订阅数据变化
            Console.WriteLine("\nSubscribing to data changes...");
            client.DataChanged += (sender, e) =>
            {
                foreach (var kvp in e.Values)
                {
                    Console.WriteLine($"  Data changed: {kvp.Key} = {kvp.Value.Value}");
                }
            };
            
            client.Subscribe(new[] { "Random.Int1", "Random.Real4" }, 1000);
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        Console.ReadKey();
    }
}
```

## 步骤 6：运行和调试

### 6.1 第一次运行
1. 按 F5 或点击"启动"
2. 如果出现权限错误，确保以管理员身份运行 VS2019

### 6.2 常见错误和解决方案

#### 错误：Class not registered (0x80040154)
**解决**：
- 确保项目是 x86 平台
- 重新安装 OPC Core Components (x86)

#### 错误：Access denied (0x80070005)
**解决**：
- 以管理员身份运行
- 检查 DCOM 配置权限

#### 错误：Cannot find OPC Server
**解决**：
- 确认 OPC 服务器已安装并注册
- 使用 OPC Expert 客户端测试连接
- 检查服务器名称是否正确

## 步骤 7：验证成功标志

如果一切正常，你应该看到：
1. "Connected successfully!" 消息
2. 可用项目列表
3. 实时数据值
4. 周期性的数据更新

## 附加工具（可选）

### OPC Expert（免费 OPC 客户端）
- 用于测试和验证 OPC 服务器
- 下载：https://www.opcexpert.com/downloads/software/opc-expert/
- 可以用来验证服务器是否正常工作

### Process Explorer
- 用于检查 COM 组件加载情况
- 下载：https://docs.microsoft.com/en-us/sysinternals/downloads/process-explorer

## 调试技巧

1. **启用 COM 互操作调试**：
   - 项目属性 → 调试 → 启用本机代码调试

2. **查看详细错误**：
   - 在 catch 块中查看 `ex.InnerException`
   - 检查 Windows 事件日志

3. **使用 Fusion Log Viewer**：
   - 查看程序集绑定失败的原因
   - 运行 `fuslogvw.exe`

记住：OPC DA 是较老的技术，可能需要一些耐心来配置。如果遇到问题，通常是权限或 x86/x64 平台不匹配导致的。