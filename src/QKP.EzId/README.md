# EzId

EzId is a lightweight .NET library for generating unique, sortable, and human-friendly readable identifiers (using Crockford Base32 encoding). It supports both 96-bit (inspired on MongoDB's ObjectID ) and 64-bit ID formats ( inspired by Twitter Snowflake ) and provides source generators for custom strongly-typed IDs with customization support for separators.

96-bit IDs: each process uses a random generated 40-bit generatorID and a 24-bit sequence (random start incrementing per ID). This approach makes coordination unnecessary, with collision odds negligible across distributed systems.

64-bit IDs: require manually assigning unique generator IDs (0–1023) for each concurrent process to avoid collisions. Each generator can emit up to 4,096 IDs per millisecond.

Example IDs:
- 96-bit: `070AB-47XF6Q8NH0-YPA46` (22 chars, dash separators)
- 96-bit: `070AB_47XF6Q8NH0_YPA46` (22 chars, underscore separators)
- 96-bit: `070AB47XF6Q8NH0YPA46` (20 chars, no separators)
- 64-bit: `070-47XF6Q8-YPA` (15 chars, dash separators)
- 64-bit: `070_47XF6Q8_YPA` (15 chars, underscore separators)
- 64-bit: `07047XF6Q8YPA` (13 chars, no separators)

## Installation

```shell
dotnet add package QKP.EzId
```

For source generation support (recommended):
```shell
dotnet add package QKP.EzId.SourceGenerator
```

## Usage

### EzId ( 96-bit ID )

```csharp
using QKP.EzId;

EzId id = EzId.GetNextId();
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
