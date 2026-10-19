# Mapping .NET methods

[EOBot documentation](../README.md)

Public static methods and properties of public .NET classes in the loaded assemblies can be imported into scripts.

| Statement | Effect |
| --- | --- |
| `from Name.Space map Type.Method` | Imports all overloads of `Type.Method` as the function `Method`. |
| `from Name.Space map Type.Method to name` | Imports them as the function `name`. |
| `from Name.Space map Type.*` | Imports each public static method of `Type` as a function with the method's name. Methods without any overloads that use supported types are skipped. |
| `from Name.Space map Type` | Imports the class `Type` as a namespace: `Type::Method()`. See [Namespaces](#namespaces). |
| `from Name.Space map Type to name` | Imports the class as the namespace `name`: `name::Method()`. |
| `from Name.Space map object Type` | Imports the public static properties of `Type` as the read-only object `$Type`. See [Objects](#objects). |
| `from Name.Space map object Type to name` | Imports them as the read-only object `$name`. |

The namespace is required. Map statements are processed at load time and must be at the top level. Mapping fails if the class or method does not exist, the method is not public and static, none of its overloads use supported types, or the name is already defined. An alias cannot be used with `*`.

Nested public types are named by their path from the outer class: `from Name.Space map Outer.Inner` maps the nested enum `Outer.Inner` as `Inner` (or the nested class as a namespace), and `from Name.Space map Outer.Nested.Method` maps a method of the nested class `Outer.Nested`.

```
from System map Math.Max
from System map Math.Abs to abs

$res = abs(Max(-5, -3))
```

## Namespaces

Mapping a class without a method creates a namespace containing the class's public static methods (each with all of its supported overloads), its public nested enums, and its public nested classes as nested namespaces. Members are accessed with `::`. Mapping fails if the class has no members that can be used in scripts.

```
from System map Math
from System map Environment to env

$res = Math::Max(1, Math::Abs(-3))
$folder = env::SpecialFolder::Desktop
$dir = env::GetFolderPath($folder)
$isFolder = $folder is env::SpecialFolder    // true
```

A namespace is not a value: it cannot be assigned, printed, or called, and `Math::Max` without a call is an error.

## Objects

`map object` creates a read-only object whose members are the class's public static properties, with the property names unchanged. A property is read each time its member is accessed, so the object always shows the current value.

```
from System map object Environment

print($Environment.$MachineName)
print($Environment.$ProcessorCount)
```

Properties without a public getter, indexers, and properties whose types are not [supported](#supported-types) are skipped. Mapping fails if no properties remain. Neither the object nor its members can be assigned.

## Supported types

Overloads that use any other type are ignored.

| .NET type | Script type |
| --- | --- |
| `int`, `long`, `short`, `byte`, `sbyte`, `uint`, `ushort`, `ulong` | int. Values outside the target type's range are truncated (higher order bits are dropped, like a C# unchecked cast). Truncating a return value prints a warning. |
| `string` | string |
| `bool` | bool |
| enum | Arguments accept an int or an enum value mapped from the same .NET enum. Return values are enum values that are the same type (`is`, `===`) as values of any mapping of that .NET enum, even if it is not mapped. |
| `List<IVariable>` and the interfaces it implements (`IList<IVariable>`, `IEnumerable<IVariable>`, ...) | array (the same array, not a copy) |
| other collections of a supported element type (`T[]`, `List<T>`, `IReadOnlyList<T>`, `ISet<T>`, `Queue<T>`, ...) | array. See [Collections](#collections). |
| `Dictionary<string, IVariable>` | dictionary (the same dictionary, not a copy) |
| `IVariable` or a specific variable type | any value of that type, passed through unchanged |
| `void`, `Task` | nothing (cannot be used as a value) |
| `Task<T>` | the awaited result, converted as `T` |

A `CancellationToken` parameter (in any position) is supplied by the interpreter and is cancelled when the script stops; it is not passed by the script. Optional parameters may be omitted.

## Collections

Scripts have a single array type; .NET collection types never appear in scripts. Any parameter or return type that implements `IEnumerable<T>` for a single supported element type `T` is an array in scripts, including nested collections such as `List<int[]>`.

An array argument is converted element by element into a new collection:

- `T[]` is created directly.
- An interface parameter (`IEnumerable<T>`, `IList<T>`, `IReadOnlyList<T>`, `ICollection<T>`, ...) receives a `List<T>`, or a `HashSet<T>` for set interfaces (`ISet<T>`, `IReadOnlySet<T>`).
- A class parameter (`HashSet<T>`, `Queue<T>`, `Stack<T>`, `Collection<T>`, ...) is created with its public constructor that takes an `IEnumerable<T>`, `IList<T>` or `ICollection<T>`. Collections without one, such as immutable collections, are not supported.

Only `List<IVariable>` and its interfaces receive the script's array itself; all other collections are copies, so changes made by the method are not visible to the script. Sets drop duplicate values, and a `Stack<T>` is filled in array order, so the last element is on top.

A returned collection becomes a new script array.

## params

A `params T[]` parameter accepts either the remaining arguments or a single array:

```
from System map String.Join

$a = Join("-", "x", "y")      // x-y
$b = Join("-", ["x", "y"])    // x-y
```

As in C#, a single array argument is passed as the array itself when it matches the parameter; otherwise the arguments are collected into an array. Signatures in error messages show a params parameter as `type... name`.

## Overloads

The overload is chosen on each call. Overloads that accept the number of arguments and whose parameter types all accept the argument values are candidates. An exact match (an int to `int`, a string to `string`) is preferred over a widening one (an int to `long`, or an enum value to an integer parameter), which is preferred over a loose one (an int to an enum parameter), which is preferred over a truncating one (an int outside the parameter's range). An array argument matches a collection parameter as well as its worst element matches the element type. If there is no candidate, or several candidates are equally good, the call is a script error that lists the available signatures. Ties are broken as in C#: an overload whose parameter types are all the same as or more specific than another's (`string[]` over `IEnumerable<string>`, `List<int>` over `IList<int>`) is chosen, then an overload that does not collect arguments into a params array. Unrelated types such as `int[]` and `List<int>` remain ambiguous.
