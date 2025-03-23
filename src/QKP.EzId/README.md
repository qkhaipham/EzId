EzId is a lightweight .NET library for generating unique, sortable, and human-friendly readable identifiers that look like `070-47XF6Q8-YPB`. It implements a 64 bit long ID generation algorithm inspired by Twitter Snowflake
and comes packed with a readonly struct that encodes it in a 15-character base32 string.

## Usage example ###

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
