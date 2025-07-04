EzId is a lightweight .NET library for generating unique, sortable, and human-friendly readable identifiers that look like `070-47XF6Q8-YPB`. It implements a 64 bit long ID generation algorithm inspired by Twitter Snowflake and comes packed with a readonly struct that encodes it in a 15-character base32 string.

## Installation

```shell
dotnet add package QKP.EzId
```

For source generation support (recommended):
```shell
dotnet add package QKP.EzId.SourceGenerator
```

## Usage

### Runtime ID Generation
```csharp
using QKP.EzId;

// Create an EzIdGenerator with a unique generator ID (0-1023)
var generator = new EzIdGenerator<EzId>(generatorId: 1);

// Generate a new ID
EzId id = generator.GetNextId();

// Convert to string
string idString = id.ToString(); // Returns a 15-character base32 string, eg. "070-47XF6Q8-YPA"

// Parse from string
EzId parsedId = EzId.Parse(idString);
```

### Source Generated IDs (Recommended)
```csharp
using QKP.EzId;

// Define your custom ID types with optional separator configuration
[EzId(SeparatorOptions.Dash, [3, 10]) // Default format: XXX-XXXXXXX-XXX
public partial struct OrderId { }

// Or use default dash separators at positions [3, 10]
[EzId]
public partial struct ProductId { }

// Or create IDs without separators
[EzId(SeparatorOptions.None, [])]
public partial struct UserId { }

[EzId(SeparatorOptions.Underscore, [3, 10]) // Default format: XXX-XXXXXXX-XXX
public partial struct PriceId { }

// Create generators with unique IDs for each type
var orderGenerator = new EzIdGenerator<OrderId>(generatorId: 1);
var productGenerator = new EzIdGenerator<ProductId>(generatorId: 2);
var userGenerator = new EzIdGenerator<UserId>(generatorId: 3);
var priceGenerator = new EzIdGenerator<PriceId>(generatorId: 3);

// Generate IDs
var orderId = orderGenerator.GetNextId();       // "070-47XF6Q8-YPA"
var productId = productGenerator.GetNextId();   // "070-47XF6Q8-YPA"
var userId = userGenerator.GetNextId();         // "07047XF6Q8YPA"
var priceId = priceGenerator.GetNextId();         // "070_47XF6Q8_YPA"

```
