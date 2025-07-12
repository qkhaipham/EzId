# EzId

EzId is a lightweight .NET library for generating unique, sortable, and human-friendly readable identifiers (using Crockford Base32 encoding). It supports both 96-bit (inspired on MongoDB's ObjectID ) and 64-bit ID formats ( inspired by Twitter Snowflake ) and provides source generators for custom strongly-typed IDs with customization support for separators.

Note:

96-bit IDs: each process uses a randomly generator ID and a 24-bit sequence (random start incrementing per ID). This approach makes coordination unnecessary, with collision odds negligible across distributed systems.

64-bit IDs: require manually assigning unique generator IDs (0–1023) for each concurrent process to avoid collisions. Each generator can emit up to 4,096 IDs per millisecond.

Example IDs:
- 96-bit: `070AB-47XF6Q8NH0-YPA46` (22 chars, dash separators)
- 96-bit: `070AB_47XF6Q8NH0_YPA46` (22 chars, underscore separators)
- 96-bit: `070AB47XF6Q8NH0YPA46` (20 chars, no separators)
- 64-bit: `070-47XF6Q8-YPA` (15 chars, dash separators)
- 64-bit: `070_47XF6Q8_YPA` (15 chars, underscore separators)
- 64-bit: `07047XF6Q8YPA` (13 chars, no separators)

---

[![Main workflow](https://github.com/qkhaipham/ezid/actions/workflows/main.yml/badge.svg)](https://github.com/qkhaipham/ezid/actions/workflows/main.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=qkhaipham_EzId&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=qkhaipham_EzId)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=qkhaipham_EzId&metric=coverage)](https://sonarcloud.io/component_measures?id=qkhaipham_EzId&metric=coverage)

|                            |                                                                                                                                                                                                                                                      |
|----------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `QKP.EzId`                 | [![NuGet](https://img.shields.io/nuget/v/QKP.EzId.svg)](https://www.nuget.org/packages/QKP.EzId/) [![NuGet](https://img.shields.io/nuget/dt/QKP.EzId.svg)](https://www.nuget.org/packages/QKP.EzId/)                                                 |
| `QKP.EzId.SourceGenerator` | [![NuGet](https://img.shields.io/nuget/v/QKP.EzId.SourceGenerator.svg)](https://www.nuget.org/packages/QKP.EzId.SourceGenerator) [![NuGet](https://img.shields.io/nuget/dt/QKP.EzId.SourceGenerator.svg)](https://www.nuget.org/packages/QKP.EzId.SourceGenerator/) |

## Features

- Generates unique 64-bit and 96-bit identifiers
- Source generator for custom strongly-typed ID structs
- Thread-safe ID generation
- IDs are sortable by creation time
- Human-friendly readable format (ID's are encoded with Crockford Base32) with customizable separators

## Installation

Install the core library:

```bash
dotnet add package QKP.EzId
```

For source generator support (recommended for custom ID types):

```bash
dotnet add package QKP.EzId.SourceGenerator
```

## Usage

### EzId ( 96-bit ID )

```csharp
using QKP.EzId;

EzId id = EzId.Generate();
string idString = id.ToString(); // e.g. "070AB-47XF6Q8NH0-YPA40"
EzId parsedId = EzId.Parse(idString);
```

### CompactEzId ( 64-bit ID )

```csharp
using QKP.EzId;

// Create a CompactEzIdGenerator with a unique generator ID (0-1023)
var generator = new CompactEzIdGenerator<CompactEzId>(generatorId: 1);

// Generate a new ID
CompactEzId id = generator.GetNextId();

// Convert to string
string idString = id.ToString(); // e.g. "070-47XF6Q8-YP0"

// Parse from string
CompactEzId parsedId = CompactEzId.Parse(idString);
```

### Important: Generator ID

The `generatorId` parameter is crucial for preventing ID collisions across different generators. It must be:

- A unique number between 0 and 1023 (10 bits)
- Consistent for each generator instance
- Different for each concurrent generator in your distributed system

For example:

```csharp
// Example for distributed system
var node1Generator = new CompactEzIdGenerator<CompactEzId>(generatorId: 1);  // For Node 1
var node2Generator = new CompactEzIdGenerator<CompactEzId>(generatorId: 2);  // For Node 2
```

### Source Generated Custom ID Types

You can create your own strongly-typed IDs using the source generator. Annotate a partial struct with `[EzIdType]` or customize with constructor arguments:

```csharp
using QKP.EzId;

// Default: 96-bit, dash separators at positions [5, 15]
// eg. XXXXX-XXXXXXXXXX-XXXXX
[EzIdType]
public partial struct ProductId { }

// Custom: 96-bit, dash separators at positions [2, 18]
// eg. XX-XXXXXXXXXXXXXXXX-XX
[EzIdType(IdBitSize.Bits96, SeparatorOptions.Dash, [2, 18])]
public partial struct PriceId { }

// Custom 64-bit, underscore separators at positions [3, 10]
// eg. XXX_XXXXXXX_XXX
[EzIdType(IdBitSize.Bits64, SeparatorOptions.Underscore, [3, 10])]
public partial struct SessionId { }

// Custom 64-bit, no separators (64-bit)
// eg. XXXXXXXXXXXXX
[EzIdType(IdBitSize.Bits64, SeparatorOptions.None, [])]
public partial struct UserId { }
```

#### Usage

```csharp
var productId = ProductId.Generate();
string productId = id.ToString(); // eg. "070AB-47XF6Q8NH0-YPA40"
SessionId parsedProductId = ProductId.Parse(productId);

var generator = new CompactEzIdGenerator<SessionId>(generatorId: 1);
SessionId sessionId = generator.GetNextId(); 
string sessionIdString = id.ToString(); // eg. "070_47XF6Q8_YP0"
SessionId parsedSessionId = SessionId.Parse(sessionIdString);
```

#### JSON Serialization

Source generated ID types automatically include JSON converters for System.Text.Json:

```csharp
// System.Text.Json
var product = new Product { Id = productId, Name = "Example" };
string json = JsonSerializer.Serialize(product);
var deserializedProduct = JsonSerializer.Deserialize<Product>(json);
```

## Why EzId vs. GUID v4 and GUID v7

EzId is tailored for scenarios demanding concise, sortable, and human-friendly identifiers, whereas GUIDs are general-purpose with fixed format and length. The table below highlights the key distinctions:

| Feature            | EzId (96‑bit)                                                          | CompactEzId (64‑bit)                                               | GUID v4                                        | GUID v7                                                        |
| ------------------ | ---------------------------------------------------------------------- | ------------------------------------------------------------------ | ---------------------------------------------- | -------------------------------------------------------------- |
| **Readability**    | 22 chars ( 20 base32 characters + 2 separators)        | 15 chars ( 13 base32 characters + 2 separators )      | 36 chars (32 hex + 4 hyphens)                  | 36 chars (32 hex + 4 hyphens)                                  |
| **Sortability**    | Embeds second precision timestamp: lexicographical/chronological order          | Embeds ms-precision timestamp: lexicographical/chronological order               | None (fully random)                            | Embeds ms-precision timestamp; lexicographical/chronological order |
| **Throughput**     | Up to 16,777,216 IDs/sec per process                                   | 4,096 IDs/ms per generator (max 1,024 generators)                  | High, but no built-in coordination or sequence | High, but no built-in coordination or sequence                 |
| **Collision Risk** | Negligible because of 40-bit random generatorId and 24-bit sequence across IDs | Negligible when generator IDs are unique and sequences initialized | Astronomically low (≈1 in 2¹²²; practically impossible)                                    | Astronomically low (≈1 in 2⁷⁴; practically impossible)                                                    |
| **Storage**        | 96 bits raw; 22 chars encoded                                    | 64 bits raw; 15 chars encoded                                | 128 bits raw; 36 chars encoded                 | 128 bits raw; 36 chars encoded                                 |

**Choosing the right ID**:\*\*:

- Use **EzId** for high‑performance, human‑readable, and time‑sortable IDs in distributed systems.
- Use **GUID v4** when you need a simple globally unique ID without ordering requirements.
- Use **GUID v7** when you want a sortable UUID but can tolerate its longer, less readable format.


### ID Structure

#### 96-bit ID (EzId and custom 96-bit types)
- 32 bits: timestamp (seconds since UNIX epoch)
- 40 bits: generator ID (random per process)
- 24 bits: sequence (random start, increments per ID)

##### ID Generation

- Generate up to 16,777,216 IDs per second

#### 64-bit ID (CompactEzId and custom 64-bit types)
- 1 bit unused
- 41 bits for timestamp (milliseconds since epoch)
- 10 bits for generator ID (0-1023)
- 12 bits for sequence number (0-4095)

##### ID Generation

- Supports up to 1024 concurrent generators
- Generates up to 4096 unique IDs per millisecond per generator

Both formats are sortable by creation time and support high concurrency.

### Why Base32 (Crockford)

Base32 (Crockford) is chosen for encoding IDs in EzId because it offers:

- Compactness: reduces the character length compared to hexadecimal or Base64, making IDs shorter and more manageable.
- Readability: omits visually similar characters (I, L, O, U), preventing misinterpretation in human transcription.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the Apache License - see the [LICENSE](LICENSE) file for details.
