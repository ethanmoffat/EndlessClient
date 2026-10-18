# Mapping .NET methods

[EOBot documentation](../README.md)

Public static methods of public .NET classes in the loaded assemblies can be imported as script functions.

| Statement | Effect |
| --- | --- |
| `from Name.Space map Type.Method` | Imports all overloads of `Type.Method` as the function `Method`. |
| `from Name.Space map Type.Method to name` | Imports them as the function `name`. |

The namespace is required. Map statements are processed at load time and must be at the top level. Mapping fails if the class or method does not exist, the method is not public and static, none of its overloads use supported types, or the name is already defined.

```
from System map Math.Max
from System map Math.Abs to abs

$res = abs(Max(-5, -3))
```

## Supported types

Overloads that use any other type are ignored.

| .NET type | Script type |
| --- | --- |
| `int`, `long`, `short`, `byte`, `sbyte`, `uint`, `ushort`, `ulong` | int. Values outside the target type's range are truncated (higher order bits are dropped, like a C# unchecked cast). Truncating a return value prints a warning. |
| `string` | string |
| `bool` | bool |
| enum | Arguments accept an int or an enum value mapped from the same .NET enum. Return values are enum values that are the same type (`is`, `===`) as values of any mapping of that .NET enum, even if it is not mapped. |
| `List<IVariable>` | array (the same array, not a copy) |
| `Dictionary<string, IVariable>` | dictionary (the same dictionary, not a copy) |
| `IVariable` or a specific variable type | any value of that type, passed through unchanged |
| `void`, `Task` | nothing (cannot be used as a value) |
| `Task<T>` | the awaited result, converted as `T` |

A `CancellationToken` parameter (in any position) is supplied by the interpreter and is cancelled when the script stops; it is not passed by the script. Optional parameters may be omitted.

## Overloads

The overload is chosen on each call. Overloads that accept the number of arguments and whose parameter types all accept the argument values are candidates. An exact match (an int to `int`, a string to `string`) is preferred over a widening one (an int to `long`, or an enum value to an integer parameter), which is preferred over a loose one (an int to an enum parameter), which is preferred over a truncating one (an int outside the parameter's range). If there is no candidate, or several candidates are equally good, the call is a script error that lists the available signatures.
