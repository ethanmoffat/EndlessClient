# Script language basics

[EOBot documentation](../README.md)

## Syntax basics

A script is a sequence of statements, one per line. Statements are executed top to bottom. A minimal script:

```
$name = "world"
print("hello " + $name)
```

- Variables start with `$` followed by a letter or `_`, then letters, digits, or `_` (for example `$x`, `$_tmp`, `$item2`). Names are case-sensitive.
- Function, label, and enum names are identifiers without `$` and start with a letter. They are case-sensitive (`Print` is not `print`).
- Keywords: `if`, `else`, `while`, `for`, `foreach`, `in`, `continue`, `break`, `goto`, `func`, `return`, `enum`, `from`, `map`, `to`.
- Literal keywords: `true`, `false`, `undefined`.
- Type keywords (used with `is`): `bool`, `int`, `string`, `Object`, `Array`, `Dict`, `enum`.

## Comments

```
// line comment
$x = 1 /* block comments can appear inline */ + 2
/*
   block comments can span lines
*/
```

An unterminated block comment is an error.

## Statements, newlines, and blocks

- Each statement ends with a newline (or end of file).
- Semicolons are not statement terminators. A `;` anywhere other than inside a `for (...)` header is an error.
- A block is a list of statements between `{` and `}`. The `{` may be on the same line as the `if`/`while`/`for`/`foreach`/`else`/`func` or on the next line.
- `} else {` and `} else if (...) {` on one line are allowed.
- A block may be empty (`{}`), and a single statement may share a line with its braces (`{ print(1) }`).
- `if`, `else`, `while`, `for`, and `foreach` may take a single statement without braces.
- The script is parsed as it executes, so syntax errors are reported only when execution reaches them. Statements before the error will already have run. Function, enum, and map declarations are the exception: they are processed when the script is loaded.

## Variables and types

Variables are created by assignment and do not need to be declared. Reading a variable that was never assigned yields `undefined` (and creates the variable).

| Type | Example | `is` keyword | Printed as |
| --- | --- | --- | --- |
| int | `42`, `-7` | `int` | `42` |
| string | `"text"` | `string` | `text` |
| bool | `true`, `false` | `bool` | `True`, `False` |
| array | `[1, "a", true]` | `Array` | `[1, a, True]` |
| dictionary | `["a": 1, "b": 2]` | `Dict` | `[a: 1, b: 2]` (empty: `[:]`) |
| object | `{ $a = 1, $b = { $c = 2 } }` | `Object` | `Object: [(a, 1), (b, Object: [(c, 2)])]` |
| enum value | `Color::Red` | `enum` or the enum name | `Color::Red` |
| undefined | `undefined` | | `Undefined` |

- Integers are 32-bit signed. Integer literals are non-negative; negative numbers are written with unary minus (`-5`). There are no floating-point numbers.
- Strings are enclosed in `"`. There are no escape sequences: a backslash is a literal character and a string cannot contain `"`.
- Assigning a value of a different type to an existing variable is allowed but prints a warning.
- Variables share a namespace with functions: the `$` is not part of the name, so a variable cannot have the same name as a built-in or user-defined function (for example `$len`, `$dict`, and `$print` are read-only).
- Arrays, dictionaries, and objects are reference types: assigning one to another variable creates an alias, not a copy.

Values are converted to bool (truthiness) in `if`, `while`, `for`, `!`, `&&`, `||`, and `?:`:

| Type | Truthy when |
| --- | --- |
| bool | `true` |
| int | not `0` |
| string | not empty |
| array, dictionary | not empty |
| object, state objects | always |
| undefined | never |

## Literals and initializers

```
$i = 10
$s = "text"
$b = true
$u = undefined

$arr = [1, 2, 3]
$empty = []

$map1 = ["a": 1, $key: 2, 3: "three"]
$emptyDict = [:]

$obj = { $name = "bot", $pos = { $x = 1, $y = 2 } }
$emptyObj = {}

$multi = [
    "first",
    "second"
]
```

- Array elements and dictionary entries can be any expression.
- Dictionary keys are converted to strings, so `$map1[3]` and `$map1["3"]` refer to the same entry and an enum key `E::A` is stored as `"E::A"`.
- Mixing array elements and dictionary entries in one initializer is an error. Duplicate dictionary keys are an error.
- Object initializers contain only `$member = expression` entries separated by commas. Duplicate members are an error.
- Initializers may span multiple lines.
- The functions `array(n)`, `dict()`, and `object()` also create collections.

## Indexing and member access

```
$arr[0]                 // array element (zero-based)
$grid[1][2]             // indexes can be chained
$entries["key"]         // dictionary entry
$obj.$member            // object member
$obj.$list[1]           // element of an array member
$arr[0].$x              // member of an array element
$obj.$pos.$x = 5        // assignment through member access
$grid[1][2] = 5         // assignment through chained indexes
$obj.$newMember = 1     // adds a member to an object
$entries["new"] = 1     // adds a dictionary entry
split($s, ",")[1]       // function results, literals, and parenthesized expressions can be indexed
```

- Array indexes must be ints. Indexing an array with a negative index or past the end is an error, for both reading and assigning; use `append` or `insert` to add elements.
- Indexing a value that is not an array or dictionary is an error, including `undefined` (for example `$d["missing"][0]`).
- Reading a missing object member yields `undefined`. Reading a missing dictionary key yields `undefined` without adding it; assigning to a missing key adds it.
- Indexing binds tighter than unary operators: `-f()[0]` negates the element.
- Only variables can be assigned through an index; `f()[0] = 1` is an error.

## Operators

| Precedence | Operators | Description |
| --- | --- | --- |
| 1 (highest) | `-x`, `!x` | Unary minus (ints only), logical not |
| 2 | `*`, `/`, `%` | Multiplication, integer division, remainder |
| 3 | `+`, `-` | Addition / string concatenation, subtraction |
| 4 | `<`, `<=`, `>`, `>=` | Comparison |
| 5 | `==`, `!=`, `===`, `!==`, `is` | Equality and type test |
| 6 | `&&` | Logical and (short-circuit) |
| 7 | `||` | Logical or (short-circuit) |
| 8 (lowest) | `cond ? a : b` | Ternary conditional (may be nested) |

Binary operators are left-associative (`10 - 3 - 2` is `5`). Parentheses group sub-expressions.

Arithmetic:

- `-`, `*`, `/`, `%` require ints. Division truncates toward zero (`-7 / 2` is `-3`, `-7 % 3` is `-1`).
- Division or remainder by zero is a script error.
- `+` adds two ints. If either operand is a string, the other is converted to a string and they are concatenated (`"a" + 1` is `"a1"`, `true + "x"` is `"Truex"`).
- Arithmetic on enum values produces plain ints (`E::C + 1`).
- `--3` is parsed as the decrement operator and is an error; write `-(-3)`.

Comparison and equality: `==` and `!=` compare values without type conversion, and `===` and `!==` also require both values to have the same type. See [Comparisons](comparisons.md) for the result of each combination of types.

Logical:

- `&&` and `||` short-circuit: the right side is not evaluated if the result is determined by the left side. The result is a bool.
- `!` converts its operand to bool and negates it.

Type test (`is`):

```
$x is int          // int, string, bool, Object, Array, Dict
$x is enum         // any enum value
$x is Color        // a value of the enum Color
```

`E::A is int` is `false`. The built-in state objects (`$account`, `$character`, `$mapstate`, `$map`) are not `Object` for the purposes of `is`.

## Assignment, increment, and decrement

| Operator | Description |
| --- | --- |
| `=` | Assign |
| `+=` | Add, or concatenate if either side is a string |
| `-=`, `*=`, `/=`, `%=` | Arithmetic compound assignment |
| `++`, `--` | Increment / decrement, prefix or postfix |

```
$x = 5
$x += 2
$s = "ab"
$s += 1          // "ab1"
$a[0] *= 3
$o.$count -= 1

$i++
--$i
$y = $x++        // $y is the old value of $x
$y = ++$x        // $y is the new value of $x
$a[0]++
$o.$n--
```

- Assignment targets may be variables, array elements, dictionary entries, or object members (including nested member access).
- Compound operators other than string `+=` convert operands to int: a numeric string is parsed (a non-numeric string becomes `0`), `true`/`false` become `1`/`0`, and a missing dictionary entry becomes `0`. For example, `$t = "5"` followed by `$t -= 1` leaves the int `4`.
- `++`/`--` may be used as statements or inside expressions with C semantics: postfix yields the old value, prefix yields the new value.
- Compound assignment and `++`/`--` on a variable that has never been assigned is an error.
- Built-in variables are read-only (except `$version`); assigning to them is an error.
