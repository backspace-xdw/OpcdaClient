using System;
using System.Collections.Generic;
using System.Threading;

namespace OpcDaClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("OPC DA Client Sample Application");
            Console.WriteLine("================================\n");
            
            using (IOpcDaClient client = new OpcDaClient())
            {
                try
                {
                    client.ServerStatusChanged += Client_ServerStatusChanged;
                    client.DataChanged += Client_DataChanged;
                    
                    Console.Write("Enter OPC Server ProgID (e.g., 'Matrikon.OPC.Simulation.1'): ");
                    string serverProgId = Console.ReadLine();
                    
                    Console.Write("Enter Host (default: localhost): ");
                    string host = Console.ReadLine();
                    if (string.IsNullOrEmpty(host))
                        host = "localhost";
                    
                    Console.WriteLine($"\nConnecting to {serverProgId} on {host}...");
                    client.Connect(serverProgId, host);
                    
                    Console.WriteLine("Connected successfully!\n");
                    
                    bool running = true;
                    while (running)
                    {
                        Console.WriteLine("\nSelect an operation:");
                        Console.WriteLine("1. Browse server");
                        Console.WriteLine("2. Browse items");
                        Console.WriteLine("3. Read item");
                        Console.WriteLine("4. Write item");
                        Console.WriteLine("5. Subscribe to items");
                        Console.WriteLine("6. Get server status");
                        Console.WriteLine("0. Exit");
                        Console.Write("Choice: ");
                        
                        var choice = Console.ReadLine();
                        
                        switch (choice)
                        {
                            case "1":
                                BrowseServer(client);
                                break;
                            case "2":
                                BrowseItems(client);
                                break;
                            case "3":
                                ReadItem(client);
                                break;
                            case "4":
                                WriteItem(client);
                                break;
                            case "5":
                                SubscribeToItems(client);
                                break;
                            case "6":
                                GetServerStatus(client);
                                break;
                            case "0":
                                running = false;
                                break;
                            default:
                                Console.WriteLine("Invalid choice!");
                                break;
                        }
                    }
                    
                    Console.WriteLine("\nDisconnecting...");
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        static void BrowseServer(IOpcDaClient client)
        {
            Console.WriteLine("\nBrowsing server branches:");
            var branches = client.BrowseServer();
            
            if (branches.Count == 0)
            {
                Console.WriteLine("No branches found.");
            }
            else
            {
                foreach (var branch in branches)
                {
                    Console.WriteLine($"  - {branch}");
                }
            }
        }
        
        static void BrowseItems(IOpcDaClient client)
        {
            Console.Write("\nEnter branch name (or press Enter for root): ");
            string branch = Console.ReadLine();
            
            Console.WriteLine($"\nBrowsing items in '{(string.IsNullOrEmpty(branch) ? "root" : branch)}':");
            var items = client.BrowseItems(branch);
            
            if (items.Count == 0)
            {
                Console.WriteLine("No items found.");
            }
            else
            {
                foreach (var item in items)
                {
                    Console.WriteLine($"  - {item.Name} (ID: {item.ItemId}, Access: {item.AccessRights})");
                }
            }
        }
        
        static void ReadItem(IOpcDaClient client)
        {
            Console.Write("\nEnter item ID to read: ");
            string itemId = Console.ReadLine();
            
            try
            {
                var value = client.ReadItem(itemId);
                Console.WriteLine($"Value: {value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading item: {ex.Message}");
            }
        }
        
        static void WriteItem(IOpcDaClient client)
        {
            Console.Write("\nEnter item ID to write: ");
            string itemId = Console.ReadLine();
            
            Console.Write("Enter value to write: ");
            string valueStr = Console.ReadLine();
            
            try
            {
                object value = valueStr;
                
                if (double.TryParse(valueStr, out double doubleValue))
                    value = doubleValue;
                else if (int.TryParse(valueStr, out int intValue))
                    value = intValue;
                else if (bool.TryParse(valueStr, out bool boolValue))
                    value = boolValue;
                
                client.WriteItem(itemId, value);
                Console.WriteLine("Value written successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing item: {ex.Message}");
            }
        }
        
        static void SubscribeToItems(IOpcDaClient client)
        {
            Console.Write("\nEnter item IDs to subscribe (comma-separated): ");
            string itemIdsStr = Console.ReadLine();
            var itemIds = itemIdsStr.Split(',');
            
            for (int i = 0; i < itemIds.Length; i++)
            {
                itemIds[i] = itemIds[i].Trim();
            }
            
            Console.Write("Enter update rate in ms (default: 1000): ");
            string rateStr = Console.ReadLine();
            int updateRate = string.IsNullOrEmpty(rateStr) ? 1000 : int.Parse(rateStr);
            
            try
            {
                var subscriptionId = client.Subscribe(itemIds, updateRate);
                Console.WriteLine($"Subscribed successfully! Subscription ID: {subscriptionId}");
                Console.WriteLine("Data changes will be displayed below. Press Enter to continue...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing: {ex.Message}");
            }
        }
        
        static void GetServerStatus(IOpcDaClient client)
        {
            var status = client.GetServerStatus();
            
            Console.WriteLine("\nServer Status:");
            Console.WriteLine($"  State: {status.State}");
            Console.WriteLine($"  Start Time: {status.StartTime}");
            Console.WriteLine($"  Current Time: {status.CurrentTime}");
            Console.WriteLine($"  Last Update Time: {status.LastUpdateTime}");
            Console.WriteLine($"  Vendor Info: {status.VendorInfo}");
        }
        
        static void Client_ServerStatusChanged(object sender, OpcServerStatusEventArgs e)
        {
            Console.WriteLine($"\n[Server Status Changed] State: {e.Status.State}");
        }
        
        static void Client_DataChanged(object sender, OpcDataChangedEventArgs e)
        {
            Console.WriteLine("\n[Data Changed]");
            foreach (var kvp in e.Values)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value.Value} (Quality: {kvp.Value.Quality}, Time: {kvp.Value.Timestamp})");
            }
        }
    }
}