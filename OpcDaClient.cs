using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OPCAutomation;

namespace OpcDaClient
{
    public class OpcDaClient : IOpcDaClient
    {
        private OPCServer _opcServer;
        private Dictionary<int, OPCGroup> _opcGroups;
        private Dictionary<int, string[]> _subscriptions;
        private bool _disposed;
        
        public bool IsConnected { get; private set; }
        
        public event EventHandler<OpcServerStatusEventArgs> ServerStatusChanged;
        public event EventHandler<OpcDataChangedEventArgs> DataChanged;
        
        public OpcDaClient()
        {
            _opcGroups = new Dictionary<int, OPCGroup>();
            _subscriptions = new Dictionary<int, string[]>();
        }
        
        public void Connect(string serverProgId, string host = "localhost")
        {
            try
            {
                _opcServer = new OPCServer();
                _opcServer.Connect(serverProgId, host);
                IsConnected = true;
                
                var status = GetServerStatus();
                ServerStatusChanged?.Invoke(this, new OpcServerStatusEventArgs { Status = status });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to OPC server: {ex.Message}", ex);
            }
        }
        
        public void Disconnect()
        {
            if (_opcServer != null && IsConnected)
            {
                foreach (var group in _opcGroups.Values)
                {
                    try
                    {
                        _opcServer.OPCGroups.Remove(group.Name);
                    }
                    catch { }
                }
                
                _opcGroups.Clear();
                _subscriptions.Clear();
                
                _opcServer.Disconnect();
                IsConnected = false;
            }
        }
        
        public List<string> BrowseServer()
        {
            EnsureConnected();
            
            var servers = new List<string>();
            OPCBrowser browser = _opcServer.CreateBrowser();
            
            browser.MoveToRoot();
            browser.ShowBranches();
            
            foreach (string branch in browser)
            {
                servers.Add(branch);
            }
            
            return servers;
        }
        
        public List<OpcItem> BrowseItems(string branch = "")
        {
            EnsureConnected();
            
            var items = new List<OpcItem>();
            OPCBrowser browser = _opcServer.CreateBrowser();
            
            if (!string.IsNullOrEmpty(branch))
            {
                browser.MoveToRoot();
                browser.MoveDown(branch);
            }
            
            browser.ShowLeafs(true);
            
            foreach (string item in browser)
            {
                items.Add(new OpcItem
                {
                    ItemId = browser.GetItemID(item),
                    Name = item,
                    AccessPath = browser.AccessPaths,
                    AccessRights = OpcAccessRights.ReadWrite,
                    DataType = typeof(object)
                });
            }
            
            return items;
        }
        
        public object ReadItem(string itemId)
        {
            return ReadItems(new[] { itemId })[itemId];
        }
        
        public Dictionary<string, object> ReadItems(string[] itemIds)
        {
            EnsureConnected();
            
            var results = new Dictionary<string, object>();
            var tempGroup = _opcServer.OPCGroups.Add($"TempRead_{Guid.NewGuid()}");
            
            try
            {
                tempGroup.IsActive = true;
                tempGroup.IsSubscribed = false;
                
                var opcItems = tempGroup.OPCItems;
                var clientHandles = new int[itemIds.Length];
                var serverHandles = new int[itemIds.Length];
                
                for (int i = 0; i < itemIds.Length; i++)
                {
                    var opcItem = opcItems.AddItem(itemIds[i], i + 1);
                    clientHandles[i] = i + 1;
                    serverHandles[i] = opcItem.ServerHandle;
                }
                
                object values, errors;
                tempGroup.SyncRead(
                    (short)OPCAutomation.OPCDataSource.OPCCache,
                    itemIds.Length,
                    ref serverHandles[0],
                    out values,
                    out errors);
                
                var valueArray = (Array)values;
                
                for (int i = 0; i < itemIds.Length; i++)
                {
                    results[itemIds[i]] = valueArray.GetValue(i + 1);
                }
            }
            finally
            {
                _opcServer.OPCGroups.Remove(tempGroup.Name);
            }
            
            return results;
        }
        
        public void WriteItem(string itemId, object value)
        {
            WriteItems(new Dictionary<string, object> { { itemId, value } });
        }
        
        public void WriteItems(Dictionary<string, object> items)
        {
            EnsureConnected();
            
            var tempGroup = _opcServer.OPCGroups.Add($"TempWrite_{Guid.NewGuid()}");
            
            try
            {
                tempGroup.IsActive = true;
                tempGroup.IsSubscribed = false;
                
                var opcItems = tempGroup.OPCItems;
                var itemIds = items.Keys.ToArray();
                var serverHandles = new int[itemIds.Length];
                var values = new object[itemIds.Length];
                
                for (int i = 0; i < itemIds.Length; i++)
                {
                    var opcItem = opcItems.AddItem(itemIds[i], i + 1);
                    serverHandles[i] = opcItem.ServerHandle;
                    values[i] = items[itemIds[i]];
                }
                
                object errors;
                tempGroup.SyncWrite(
                    itemIds.Length,
                    ref serverHandles[0],
                    ref values[0],
                    out errors);
            }
            finally
            {
                _opcServer.OPCGroups.Remove(tempGroup.Name);
            }
        }
        
        public int Subscribe(string[] itemIds, int updateRate = 1000)
        {
            EnsureConnected();
            
            var subscriptionId = _opcGroups.Count + 1;
            var groupName = $"Subscription_{subscriptionId}";
            var opcGroup = _opcServer.OPCGroups.Add(groupName);
            
            opcGroup.IsActive = true;
            opcGroup.IsSubscribed = true;
            opcGroup.UpdateRate = updateRate;
            
            var opcItems = opcGroup.OPCItems;
            for (int i = 0; i < itemIds.Length; i++)
            {
                opcItems.AddItem(itemIds[i], i + 1);
            }
            
            opcGroup.DataChange += OpcGroup_DataChange;
            
            _opcGroups[subscriptionId] = opcGroup;
            _subscriptions[subscriptionId] = itemIds;
            
            return subscriptionId;
        }
        
        public void Unsubscribe(int subscriptionId)
        {
            if (_opcGroups.ContainsKey(subscriptionId))
            {
                var group = _opcGroups[subscriptionId];
                group.DataChange -= OpcGroup_DataChange;
                _opcServer.OPCGroups.Remove(group.Name);
                _opcGroups.Remove(subscriptionId);
                _subscriptions.Remove(subscriptionId);
            }
        }
        
        public OpcServerStatus GetServerStatus()
        {
            EnsureConnected();
            
            return new OpcServerStatus
            {
                StartTime = _opcServer.StartTime,
                CurrentTime = _opcServer.CurrentTime,
                LastUpdateTime = _opcServer.LastUpdateTime,
                State = (OpcServerState)_opcServer.ServerState,
                VendorInfo = _opcServer.VendorInfo
            };
        }
        
        private void OpcGroup_DataChange(
            int transactionId,
            int numItems,
            ref Array clientHandles,
            ref Array itemValues,
            ref Array qualities,
            ref Array timeStamps)
        {
            var changedValues = new Dictionary<string, OpcItemValue>();
            
            for (int i = 1; i <= numItems; i++)
            {
                var clientHandle = (int)clientHandles.GetValue(i);
                var subscriptionId = _opcGroups.FirstOrDefault(x => x.Value.Name.Contains($"Subscription_{transactionId}")).Key;
                
                if (subscriptionId > 0 && _subscriptions.ContainsKey(subscriptionId))
                {
                    var itemIds = _subscriptions[subscriptionId];
                    if (clientHandle <= itemIds.Length)
                    {
                        var itemId = itemIds[clientHandle - 1];
                        changedValues[itemId] = new OpcItemValue
                        {
                            Value = itemValues.GetValue(i),
                            Quality = (OpcQuality)(int)qualities.GetValue(i),
                            Timestamp = (DateTime)timeStamps.GetValue(i)
                        };
                    }
                }
            }
            
            DataChanged?.Invoke(this, new OpcDataChangedEventArgs { Values = changedValues });
        }
        
        private void EnsureConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("OPC client is not connected");
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    
                    if (_opcServer != null)
                    {
                        Marshal.ReleaseComObject(_opcServer);
                        _opcServer = null;
                    }
                }
                
                _disposed = true;
            }
        }
    }
}