# Errors and known limitations

[EOBot documentation](../README.md)

## Errors

- Errors print the line and column where they occurred followed by a message, for example `Error during execution at line 4 column 2` and `Calling function 'F' with wrong number of parameters`.
- `error("message")` prints the call location(s) and `Error at line L column C: message` and exits with code 4.
- Errors during load (invalid characters such as `@`, an unterminated string or block comment, or invalid function/enum/map declarations) fail bot initialization; initialization is retried `initattempts` times before the bot gives up.
- See [Exit codes](cli.md#exit-codes) for the mapping of errors to exit codes.

## Known quirks

- `$args` values are always strings; use `parse()` for numbers.
- `$account`, `$character`, `$mapstate`, and `$map` are predefined and read-only; they cannot be assigned or used as function parameter names.
- Variable names that match a built-in or user-defined function name (for example `$dict`, `$len`) are read-only.
