# EzId.SourceGenerator

Source generator companion for [EzId](https://github.com/qkhaipham/ezid) that provides compile-time strongly-typed ID generation.

## Requirements

- Main package: `QKP.EzId` (automatically included as a dependency)
- .NET Standard 2.1 or higher

## Features

- Compile-time generation of strongly-typed IDs
- Type-safe equality and comparison operations
- Built-in JSON serialization support
- String parsing and formatting
- Zero runtime reflection
- Configurable separator formats

## Usage

1. Install both packages:
```shell
dotnet add package QKP.EzId
dotnet add package QKP.EzId.SourceGenerator
```

2. Define your ID types:
```csharp
// Default format with dash separators at positions [3, 10]: XXX-XXXXXXX-XXX
[EzIdType]
public partial struct OrderId { }

// Custom separator positions
[EzIdType(SeparatorOptions.Dash, new[] { 4, 8 })] // XXXX-XXXX-XXXXXXX
public partial struct ProductId { }

// No separators
[EzIdType(SeparatorOptions.None)]
public partial struct UserId { } // XXXXXXXXXXXXXX

// Underscore separators
[EzIdType(SeparatorOptions.Underscore, new[] { 3, 10 })] // XXX_XXXXXXX_XXX
public partial struct CustomerId { }
```

3. Create generators and use the implementations:
```csharp
// Create generators with unique IDs for each type
var orderGenerator = new CompactEzIdGenerator<OrderId>(generatorId: 1);
var productGenerator = new CompactEzIdGenerator<ProductId>(generatorId: 2);
var userGenerator = new CompactEzIdGenerator<UserId>(generatorId: 3);
var customerGenerator = new CompactEzIdGenerator<CustomerId>(generatorId: 4);

// Generate IDs
var orderId = orderGenerator.GetNextId();         // "070-47XF6Q8-YPA"
var productId = productGenerator.GetNextId();     // "0704-7XF6-Q8YPA"
var userId = userGenerator.GetNextId();           // "07047XF6Q8YPA"
var customerId = customerGenerator.GetNextId();   // "070_47XF6Q8_YPA"

// Parse from string (must match configured format)
var parsed = OrderId.Parse("070-47XF6Q8-YPA");

// Type-safe comparisons
bool areEqual = orderId == orderGenerator.GetNextId();    // false
bool invalid = orderId == productId;                      // Won't compile - different types!
```

## Generated Features
- Generator support via CompactEzIdGenerator<T>
- Parse() and TryParse() methods
- ToString() with configured separators
- Equality operators and comparisons
- JSON serialization support
- Implicit conversion from underlying EzId

## Important Notes
- Each generator instance must have a unique generatorId (0-1023)
- Generator instances should be reused, not created per ID
- Consider using dependency injection to manage generator instances

## Troubleshooting

If source generation isn't working:
1. Ensure both packages are installed
2. Type must be a struct marked with `[EzIdType]` attribute
3. Type must be declared as `partial`
4. Check build output for any generator diagnostics
5. Verify separator positions are between 0 and 12 when using separators
