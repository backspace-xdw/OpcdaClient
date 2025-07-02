using System;
using System.Collections.Generic;

namespace OpcDaClient
{
    public interface IOpcDaClient : IDisposable
    {
        bool IsConnected { get; }
        event EventHandler<OpcServerStatusEventArgs> ServerStatusChanged;
        event EventHandler<OpcDataChangedEventArgs> DataChanged;
        
        void Connect(string serverProgId, string host = "localhost");
        void Disconnect();
        
        List<string> BrowseServer();
        List<OpcItem> BrowseItems(string branch = "");
        
        object ReadItem(string itemId);
        Dictionary<string, object> ReadItems(string[] itemIds);
        
        void WriteItem(string itemId, object value);
        void WriteItems(Dictionary<string, object> items);
        
        int Subscribe(string[] itemIds, int updateRate = 1000);
        void Unsubscribe(int subscriptionId);
        
        OpcServerStatus GetServerStatus();
    }
    
    public class OpcItem
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string AccessPath { get; set; }
        public OpcAccessRights AccessRights { get; set; }
        public Type DataType { get; set; }
    }
    
    public enum OpcAccessRights
    {
        None = 0,
        Readable = 1,
        Writable = 2,
        ReadWrite = 3
    }
    
    public class OpcServerStatus
    {
        public DateTime StartTime { get; set; }
        public DateTime CurrentTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public OpcServerState State { get; set; }
        public string VendorInfo { get; set; }
    }
    
    public enum OpcServerState
    {
        Running = 1,
        Failed = 2,
        NoConfig = 3,
        Suspended = 4,
        Test = 5,
        CommFault = 6
    }
    
    public class OpcServerStatusEventArgs : EventArgs
    {
        public OpcServerStatus Status { get; set; }
    }
    
    public class OpcDataChangedEventArgs : EventArgs
    {
        public Dictionary<string, OpcItemValue> Values { get; set; }
    }
    
    public class OpcItemValue
    {
        public object Value { get; set; }
        public OpcQuality Quality { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public enum OpcQuality
    {
        Bad = 0,
        Uncertain = 64,
        Good = 192,
        GoodLocalOverride = 216
    }
}