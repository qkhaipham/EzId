# EzId

EzId is a lightweight .NET library for generating unique, sortable, and human-friendly readable identifiers that look for example like: `070-47XF6Q8-YPB`. It implements a 64 bit long ID generation algorithm inspired by Twitter Snowflake and comes packed with a readonly struct that encodes it in a 15-character base32 string.

---

[![Main workflow](https://github.com/qkhaipham/ezid/actions/workflows/main.yml/badge.svg)](https://github.com/qkhaipham/ezid/actions/workflows/main.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=qkhaipham_EzId&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=qkhaipham_EzId)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=qkhaipham_EzId&metric=coverage)](https://sonarcloud.io/component_measures?id=qkhaipham_EzId&metric=coverage)

| | |
|---|---|
| `QKP.EzId` | [![NuGet](https://img.shields.io/nuget/v/QKP.EzId.svg)](https://www.nuget.org/packages/QKP.EzId/) [![NuGet](https://img.shields.io/nuget/dt/QKP.EzId.svg)](https://www.nuget.org/packages/QKP.EzId/) |

## Features

- Generates unique 64-bit identifiers encoded in Crockford's base32 format
- Thread-safe ID generation
- Supports up to 1024 concurrent generators
- Generates up to 4096 unique IDs per millisecond per generator
- IDs are sortable by creation time
- Human-friendly readable format

## Installation

### Using .NET CLI

```bash
dotnet add package QKP.EzId
```

For source generation support (recommended):
```bash
dotnet add package QKP.EzId.SourceGenerator
```

## Usage

### Basic Usage

```csharp
using QKP.EzId;

// Create an EzIdGenerator with a unique generator ID (0-1023)
var generator = new EzIdGenerator<EzId>(generatorId: 1);

// Generate a new ID
EzId id = generator.GetNextId();

// Convert to string
string idString = id.ToString(); // Returns a 15-character base32 string eg. "070-47XF6Q8-YPA"

// Parse from string
EzId parsedId = EzId.Parse(idString);
```

### Source Generated ID Types

You can create your own strongly-typed IDs using the source generator:

```csharp
using QKP.EzId;

// Define a custom ID type
[EzIdType] // Default: dash separators at positions [3, 10]
public partial struct ProductId { }

// Use it just like the base EzId
var generator = new EzIdGenerator<ProductId>(generatorId: 1);
ProductId id = generator.GetNextId();
string idString = id.ToString(); // Returns e.g. "070-47XF6Q8-YPA"
ProductId parsedId = ProductId.Parse(idString);
```

#### Customization Options

The `EzIdType` attribute supports the following options:

- `Separator`: The type of separator to use (None, Dash, or Underscore)
- `SeparatorPositions`: Array of positions where separators should appear (0-12)

Examples:

```csharp
// Default format (XXX-XXXXXXX-XXX)
[EzIdType]
public partial struct OrderId { }

// Custom separator positions (XXXX-XXXX-XXXXXXX)
[EzIdType(SeparatorOptions.Dash, [4, 8])]
public partial struct ProductId { }

// No separators (XXXXXXXXXXXXXX)
[EzIdType(SeparatorOptions.None, [])]
public partial struct UserId { }

// Underscore separators (XXX_XXXXXXX_XXX)
[EzIdType(SeparatorOptions.Underscore, [3, 10])]
public partial struct SessionId { }
```

#### JSON Serialization

Source generated ID types automatically include JSON converters for System.Text.Json and Newtonsoft.Json:

```csharp
// System.Text.Json
var product = new Product { Id = productId, Name = "Example" };
string json = JsonSerializer.Serialize(product);
var deserializedProduct = JsonSerializer.Deserialize<Product>(json);
```

### Important: Generator ID

The `generatorId` parameter is crucial for preventing ID collisions across different generators. It must be:

- A unique number between 0 and 1023 (10 bits)
- Consistent for each generator instance
- Different for each concurrent generator in your distributed system

For example:
- In a distributed system, use different generator IDs for each server/node
- In a multi-threaded application, use different generator IDs for each thread
- In a microservice architecture, assign unique generator IDs to each service instance

```csharp
// Example for distributed system
var node1Generator = new EzIdGenerator<EzId>(generatorId: 1);  // For Node 1
var node2Generator = new EzIdGenerator<EzId>(generatorId: 2);  // For Node 2
```

### ID Structure

Each generated ID consists of:
- 1 bit unused
- 41 bits for timestamp (milliseconds since epoch)
- 10 bits for generator ID (0-1023)
- 12 bits for sequence number (0-4095)

This structure ensures that:
- IDs are sortable by creation time
- Each generator can create up to 4096 unique IDs per millisecond
- Up to 1024 generators can operate concurrently without collisions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the Apache License - see the [LICENSE](LICENSE) file for details.
