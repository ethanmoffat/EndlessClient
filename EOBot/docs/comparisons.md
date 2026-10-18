# Comparisons

The result of comparing two values depends on the types of both operands. The order of the operands does not matter: `$a == $b` and `$b == $a` behave the same. `!=` and `!==` are the negation of `==` and `===` and fail in the same cases.

| Operands | `==` | `===` | `<`, `<=`, `>`, `>=` |
| --- | --- | --- | --- |
| int and int | By value | By value | Numerically |
| int and enum value | By value (`E::A == 0` is `true`) | `false` | By value |
| enum values of the same enum | By value | By value | By value |
| enum values of different enums | By value | `false` | By value |
| bool and bool | By value | By value | `false < true` |
| string and string | By value (case-sensitive) | By value | Ordinally by character code (`"B" < "a"`) |
| `undefined` and `undefined` | `true` | `true` | Equal (`<=` and `>=` are `true`) |
| `undefined` and any other value | `false` | `false` | Error |
| array and array | By contents | Same instance only | Error |
| dictionary and dictionary | By contents | Same instance only | Error |
| object and object | By contents | Same instance only | Error |
| state object and the same state object | `true` | `true` | Error |
| state object and an object or a different state object | `false` | `false` | Error |
| array, dictionary, object, or state object and a different kind of value | Error | `false` | Error |
| Any other pair (for example int and string, bool and int, enum value and string) | `false` | `false` | Error |

State objects are `$account`, `$character`, `$mapstate`, and `$map`. Comparison errors are script errors (exit code 3).

Comparing by contents:

- Arrays are equal when they have the same length and their elements are equal in order.
- Dictionaries are equal when they have the same keys with equal values, regardless of insertion order. Keys are strings, so `[3: 1] == ["3": 1]` is `true`.
- Objects are equal when they have the same members with equal values.
- Elements, values, and members are compared with `==`, so nested collections are compared by contents and an incompatible nested pair (for example `[[1]] == [1]`) is an error.
- Comparing a collection that contains itself is an error.

The `remove` and `contains` built-in functions search arrays with `==`, but skip elements that cannot be compared instead of raising an error; see [Built-in functions](builtins.md#collections).
