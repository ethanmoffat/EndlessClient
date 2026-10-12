# Control flow and functions

[EOBot documentation](../README.md)

## Control flow

`if` / `else if` / `else`:

```
if ($hp < 10) {
    print("low")
} else if ($hp < 50) {
    print("medium")
} else {
    print("high")
}

if ($done) print("single statement form")
```

`while`:

```
$i = 0
while ($i < 10) {
    $i++
}
```

`for`: the header contains an assignment, a condition, and an assignment or increment/decrement, separated by `;`. The condition is converted to bool like an `if` condition. The loop variable remains defined after the loop.

```
for ($i = 0; $i < len($arr); $i++) {
    print($arr[$i])
}
```

`foreach`: iterates over an array or dictionary produced by any expression. The loop variable must be a plain variable. Iterating a dictionary yields objects with `$key` (string) and `$value` members.

```
foreach ($item in $character.$inventory) {
    print($item.$name + ": " + $item.$amount)
}

foreach ($entry in ["a": 1, "b": 2]) {
    print($entry.$key + "=" + $entry.$value)
}
```

If the `foreach` variable already exists, a warning is printed and its previous value is restored after the loop; otherwise the variable is removed after the loop.

`break` exits the innermost loop and `continue` skips to its next iteration. Both work inside nested `if` blocks. Outside of any loop, they are an error.

## Labels and goto

A label is an identifier followed by `:` on its own line. `goto label` jumps to it. Labels do not use `$` and have their own namespace.

```
retry:
$reply = Login($user, $pass)
if ($reply != LoginReply::Ok) {
    sleep(1000)
    goto retry
}
```

Labels must be unique. Labels inside a function are local to that function.

## Functions

```
func Add($a, $b) {
    return $a + $b
}

print(Add(1, 2))
```

- Declare functions with `func Name($param1, $param2) { ... }`, at the top level of the script or inside another function. Parameters are variables. A function may have no parameters.
- Functions are hoisted: they are registered when the script loads and may be called before their declaration. Declaring a function with the same name again prints a warning and replaces it.
- Call functions by name without `$`. Calling with the wrong number of arguments is an error. Arguments may be `undefined`; passing an argument of the wrong type to a built-in function is an error.
- Scoping: a function body can read and modify global variables. Parameters shadow globals with the same name; the global value is restored when the function returns. Variables first created inside the function are removed when it returns. A parameter cannot be named after a built-in variable (for example `$host`).
- `return expr` exits the function immediately with the value of `expr`, including from inside loops.
- `return` must have a value; a bare `return` is a parse error.
- A function that executes no `return` evaluates to `undefined`.
- Using a function that returns nothing (for example `print`) as a value is an error. Calling it as a statement is fine.
- Functions may call themselves recursively, up to a call depth of 1000; exceeding it is an error.
- A function declared inside another function can only be called while the enclosing function is running. It hides a function with the same name declared outside until the enclosing function returns.
- Enum and map declarations inside a function body are errors.
