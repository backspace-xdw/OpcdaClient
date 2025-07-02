# OPC DA Client for C# (.NET Framework)

This is a C# implementation of an OPC DA (Data Access) client for Visual Studio 2019, targeting .NET Framework 4.7.2.

## Features

- Connect to OPC DA servers
- Browse server namespace and items
- Read/Write single or multiple items
- Subscribe to data changes with customizable update rates
- Monitor server status
- Full COM interop support

## Requirements

- Visual Studio 2019
- .NET Framework 4.7.2
- Windows operating system (OPC DA is Windows-only)
- OPC DA server installed on the machine or network

## Project Structure

```
OpcDaClient/
├── OpcDaClient.sln          # Visual Studio solution file
├── OpcDaClient.csproj       # Project file
├── IOpcDaClient.cs          # Interface and data models
├── OpcDaClient.cs           # Main implementation
├── Program.cs               # Console application sample
└── README.md                # This file
```

## Installation

1. Open `OpcDaClient.sln` in Visual Studio 2019
2. Restore NuGet packages (will install OPCFoundation.NetStandard.Opc.Da)
3. Build the solution (x86 platform recommended for OPC compatibility)

## Usage Example

```csharp
using (IOpcDaClient client = new OpcDaClient())
{
    // Connect to server
    client.Connect("Matrikon.OPC.Simulation.1", "localhost");
    
    // Read a single item
    var value = client.ReadItem("Random.Int4");
    Console.WriteLine($"Value: {value}");
    
    // Subscribe to data changes
    client.DataChanged += (sender, e) =>
    {
        foreach (var item in e.Values)
        {
            Console.WriteLine($"{item.Key}: {item.Value.Value}");
        }
    };
    
    client.Subscribe(new[] { "Random.Int4", "Random.Real8" }, 1000);
    
    // Keep running...
    Console.ReadLine();
    
    // Disconnect
    client.Disconnect();
}
```

## Key Classes

### IOpcDaClient
Main interface providing all OPC DA client functionality.

### OpcDaClient
Implementation of IOpcDaClient using OPCAutomation COM interop.

### OpcItem
Represents an OPC item with properties like ItemId, Name, AccessRights, etc.

### OpcItemValue
Contains value, quality, and timestamp for OPC data.

## Notes

- The project is configured for x86 platform to ensure OPC COM compatibility
- Make sure to run Visual Studio as Administrator if connecting to local OPC servers
- Always dispose of the client properly to release COM resources

## Common OPC Servers for Testing

- Matrikon OPC Server for Simulation
- KEPServerEX
- RSLinx Classic OPC Server
- Schneider OPC Factory Server