# EzId.SourceGenerator

Source generator companion for [EzId](https://github.com/qkhaipham/ezid) that provides compile-time strongly-typed ID generation.

## Requirements

- Main package: `QKP.EzId` (automatically included as a dependency)
- **.NET SDK 5.0+** 
- **Roslyn 3.8+**

## Features

- Compile-time generation of strongly-typed IDs
- Type-safe equality and comparison operations
- Built-in JSON serialization support
- String parsing and formatting
- Configurable ID Format ( bitSize and separators)

## Usage

1. Install both packages:
```shell
dotnet add package QKP.EzId
dotnet add package QKP.EzId.SourceGenerator
```

2. Define your ID types:
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

3. Use the source-generated implementations:
```csharp
var productId = ProductId.GetNextId();
string productId = id.ToString(); // eg. "070AB-47XF6Q8NH0-YPA40"
SessionId parsedProductId = ProductId.Parse(productId);

var generator = new CompactEzIdGenerator<SessionId>(generatorId: 1);
SessionId sessionId = generator.GetNextId(); 
string sessionIdString = id.ToString(); // eg. "070_47XF6Q8_YP0"
SessionId parsedSessionId = SessionId.Parse(sessionIdString);
```

## Generated Features
- Generator support via CompactEzIdGenerator<T>
- Parse() and TryParse() methods
- ToString() with configured separators
- Equality operators and comparisons
- JSON serialization support

## Troubleshooting

If source generation isn't working:
1. Ensure both packages are installed
2. Type must be a struct marked with `[EzIdType]` attribute
3. Type must be declared as `partial`
4. Check build output for any generator diagnostics
