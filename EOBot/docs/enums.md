# Enums

[EOBot documentation](../README.md)

## Declaring enums

```
enum Color { Red, Green = 5, Blue }

enum Mode {
    Idle,
    Fight = -1,
}

$c = Color::Blue
print($c)                     // Color::Blue
print($c.$name)               // Blue
print($c.$value)              // 6
print($c.$type)               // Color
print($c == 6)                // True
print($c === 6)               // False
print($c is Color)            // True
print(Color::Blue + 1)        // 7
```

- Members are numbered from 0; a member without an explicit value is one more than the previous member. Explicit values may be negative. A trailing comma is allowed. Declarations may be on one line or span multiple lines.
- Enum declarations are processed at load time and must be at the top level. Duplicate member names or reusing an existing enum or alias name is an error.
- Reference members with `EnumName::Member`.
- Enum values have the members `$name` (string), `$value` (int), and `$type` (string). Members can only be accessed on a variable, not directly on a `Name::Member` expression.
- Enum values can be dictionary keys (stored as the string `"Name::Member"`), compared to ints and to values of other enums with `==`, and passed to functions that take ints (for example `Face(Direction::Up)`).
- Enum values can be ordered with `<`, `<=`, `>`, `>=` against ints and values of any enum, by value.
- `$x is Name` and `===` compare enum types: mapped .NET enums match by .NET type (so after `map LoginReply` and `map LoginReply to Reply`, `Reply::Ok is LoginReply` is `true`), and script-declared enums match by name.

## Mapping .NET enums

Public .NET enums from the loaded assemblies (EOLib and the Endless Online SDK) can be imported so that game return codes can be compared by name.

| Statement | Effect |
| --- | --- |
| `map Type` | Imports the enum named `Type`. Fails if the name is ambiguous across namespaces. |
| `map Type to Alias` | Imports `Type` under the name `Alias`. |
| `from Name.Space map Type` | Imports `Type` from a specific namespace. |
| `from Name.Space map Type to Alias` | Imports `Type` from a namespace under the name `Alias`. |
| `from Name.Space map *` | Imports every enum in the namespace. An alias is not allowed. |

Map statements are processed at load time and must be at the top level.

```
from Moffat.EndlessOnline.SDK.Protocol.Net.Server map *
map Direction

$reply = Login($user, $pass)
if ($reply != LoginReply::Ok) {
    error("login failed: " + $reply)
}
Face(Direction::Right)
```

Commonly used enums (only int-backed enums are supported):

| Enum | Namespace | Members |
| --- | --- | --- |
| `AccountReply` | `Moffat.EndlessOnline.SDK.Protocol.Net.Server` | `Exists`=1, `NotApproved`=2, `Created`=3, `ChangeFailed`=5, `Changed`=6, `RequestDenied`=7 |
| `LoginReply` | `Moffat.EndlessOnline.SDK.Protocol.Net.Server` | `WrongUser`=1, `WrongUserPassword`=2, `Ok`=3, `Banned`=4, `LoggedIn`=5, `Busy`=6 |
| `CharacterReply` | `Moffat.EndlessOnline.SDK.Protocol.Net.Server` | `Exists`=1, `Full`=2, `Full3`=3, `NotApproved`=4, `Ok`=5, `Deleted`=6 |
| `Direction` | `Moffat.EndlessOnline.SDK.Protocol` | `Down`=0, `Left`=1, `Up`=2, `Right`=3 |
| `AdminLevel` | `Moffat.EndlessOnline.SDK.Protocol` | `Player`=0, `Spy`=1, `LightGuide`=2, `Guardian`=3, `GameMaster`=4, `HighGameMaster`=5 |

Game functions return plain ints, so compare them with `==`/`!=` rather than `===`/`!==`.
