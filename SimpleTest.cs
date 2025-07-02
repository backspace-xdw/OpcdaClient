using System;
using System.Threading;

namespace OpcDaClient
{
    /// <summary>
    /// 简化的测试程序，用于快速验证 OPC 连接
    /// </summary>
    class SimpleTest
    {
        static void RunSimpleTest()
        {
            Console.WriteLine("=== OPC DA Client Simple Test ===\n");

            // 常见的 OPC 服务器列表
            string[] commonServers = {
                "Matrikon.OPC.Simulation.1",
                "Matrikon.OPC.Simulation",
                "Graybox.Simulator.1",
                "KEPware.KEPServerEx.V6",
                "Schneider-Aut.OFS",
                "RSLinx OPC Server"
            };

            Console.WriteLine("尝试连接到常见的 OPC 服务器...\n");

            IOpcDaClient client = null;
            string connectedServer = null;

            // 尝试连接到任何可用的服务器
            foreach (var server in commonServers)
            {
                try
                {
                    Console.Write($"尝试连接到 {server}... ");
                    client = new OpcDaClient();
                    client.Connect(server, "localhost");
                    connectedServer = server;
                    Console.WriteLine("成功！");
                    break;
                }
                catch
                {
                    Console.WriteLine("失败");
                    client?.Dispose();
                    client = null;
                }
            }

            if (client == null)
            {
                Console.WriteLine("\n未找到可用的 OPC 服务器。");
                Console.WriteLine("\n请确保：");
                Console.WriteLine("1. 已安装 OPC Core Components (x86)");
                Console.WriteLine("2. 已安装至少一个 OPC 服务器");
                Console.WriteLine("3. 以管理员身份运行此程序");
                Console.WriteLine("4. 项目平台设置为 x86");
                return;
            }

            try
            {
                Console.WriteLine($"\n已连接到: {connectedServer}");
                Console.WriteLine("\n=== 服务器信息 ===");
                
                var status = client.GetServerStatus();
                Console.WriteLine($"服务器状态: {status.ServerState}");
                Console.WriteLine($"供应商信息: {status.VendorInfo}");
                Console.WriteLine($"版本: {status.Version}");

                Console.WriteLine("\n=== 浏览项目 (前20个) ===");
                var items = client.BrowseItems();
                int count = 0;
                foreach (var item in items)
                {
                    Console.WriteLine($"{count + 1}. {item.ItemId}");
                    count++;
                    if (count >= 20) break;
                }

                if (count > 0)
                {
                    Console.WriteLine("\n=== 读取测试 ===");
                    // 尝试读取前5个项目
                    int readCount = Math.Min(5, count);
                    var itemsArray = new List<OpcItem>(items).GetRange(0, readCount);
                    
                    foreach (var item in itemsArray)
                    {
                        try
                        {
                            var value = client.ReadItem(item.ItemId);
                            Console.WriteLine($"{item.ItemId} = {value} (Quality: Good)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{item.ItemId} = 读取失败 ({ex.Message})");
                        }
                    }

                    Console.WriteLine("\n=== 订阅测试 (10秒) ===");
                    Console.WriteLine("订阅前3个项目的数据变化...");
                    
                    var subscribeItems = itemsArray.Take(3).Select(i => i.ItemId).ToArray();
                    
                    client.DataChanged += (sender, e) =>
                    {
                        Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] 数据更新:");
                        foreach (var kvp in e.Values)
                        {
                            Console.WriteLine($"  {kvp.Key} = {kvp.Value.Value}");
                        }
                    };

                    client.Subscribe(subscribeItems, 1000);
                    
                    Console.WriteLine("\n等待数据更新 (按任意键停止)...");
                    Console.ReadKey();
                }

                Console.WriteLine("\n=== 测试完成 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n测试过程中出错: {ex.Message}");
                Console.WriteLine($"详细信息: {ex.StackTrace}");
            }
            finally
            {
                client?.Dispose();
                Console.WriteLine("\n已断开连接。");
            }
        }

        // 可以将这个方法添加到 Program.cs 的 Main 方法中调用
        // 或者创建一个新的入口点
    }
}