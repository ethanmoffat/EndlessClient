# Running EOBot

[EOBot documentation](../README.md)

```
EOBot.exe host=<host> port=<port> bots=<numBots>[,<simultaneousBots>] [initdelay=<ms>] account=<account> password=<password> character=<character> [script=<file>] [autoconnect=<true|false>] [initattempts=<n>] [-- arg1 [arg2 ... argN]]
```

Run `EOBot.exe help` to print usage. Running with invalid or missing arguments prints the error followed by usage.

## Command-line arguments

Arguments are `key=value` pairs, may be given in any order, and keys are case-insensitive. Values cannot contain `=`.

| Argument | Required | Default | Description |
| --- | --- | --- | --- |
| `host` | Without a script | none | Server hostname or IP address. Available to scripts as `$host`. |
| `port` | Without a script | none | Server port (usually 8078). Available to scripts as `$port`. |
| `bots` | Yes | none | `N` or `N,S`. `N` is the total number of bots (1-25); `S` is how many run simultaneously (defaults to `N`, must be ≤ `N`). Each bot runs its own copy of the script with its own `$botindex`. |
| `initdelay` | No | `1100` | Delay in milliseconds between bot INIT handshakes. Must be an integer ≥ 1100. |
| `account` | Without a script | none | Account name. Available to scripts as `$user`. |
| `password` | Without a script | none | Account password. Available to scripts as `$pass`. |
| `character` | Without a script | none | Character name. Not exposed to scripts as a variable; pass it via script arguments if needed. |
| `script` | No | none | Path to the script file to execute. If omitted, the legacy trainer bot is run. |
| `autoconnect` | No | `true` | When `true`, each bot connects to `host`:`port` before the script starts (staggered by `initdelay`) and disconnects 2 seconds after the script finishes. When `false`, the script must call `Connect` itself. |
| `initattempts` | No | `5` | Number of attempts to initialize each bot. Must be ≥ 1. Retries wait `initdelay + 1000 * attempt²` ms. |
| `help` | No | | Prints usage and a link to this documentation, then exits with code 1. |
| `--` | No | | Every argument after `--` is passed to the script in `$args`. |

## Validation rules

- An unknown key, or a value containing `=`, is a format error.
- Without `script`, `host`, `port`, `bots`, `account`, `password`, and `character` are all required.
- `--` script arguments and `autoconnect=false` require `script`.
- With `script` and more than one bot, `autoconnect=false` is not allowed (eoserv connection throttling).
- `script` must point to an existing file.
- `bots` must be between 1 and 25. With `script`, `bots` is still required in practice: omitting it fails at startup with exit code 4.
- `initdelay` must be an integer ≥ 1100 (the error message says "> 1100ms", but 1100 is accepted).
- `initattempts` must be ≥ 1.
- `autoconnect` must be `true` or `false` (case-insensitive).

Validation errors print a message and usage and exit with code 1.

## Script arguments

Arguments after `--` are available in the read-only `$args` array. They are always strings; use `parse()` to convert to `int`.

```
EOBot.exe script=farm.eob bots=1 host=localhost port=8078 account=bot password=secret -- MyChar 10
```

```
$name = $args[0]
$count = parse($args[1])
```

## Trainer bot (no script)

When no `script` is given, each bot runs the legacy trainer bot: each bot connects, logs in to `account` (creating it if it does not exist), logs in to `character`, and then repeatedly attacks adjacent NPCs, picks up items, and heals with items or spells. All bots use the same account and character names. Script arguments and `autoconnect=false` are not available in this mode.

## Exit codes

| Code | Meaning |
| --- | --- |
| `0` | Success. The script ran to completion. |
| `1` | Invalid arguments, or `help` was requested. |
| `2` | Parse error: an unexpected token was encountered (for example `$x = )` or `return` without a value). |
| `3` | Evaluation error: a script runtime error such as division by zero, an index out of range, comparing incompatible values, `break` or `continue` outside of a loop, a semicolon outside a `for` header, a wrong number of function arguments, assigning to a read-only variable, exceeding the maximum call depth, or passing an argument of the wrong type to a built-in function. |
| `4` | Exception: `error()` was called, a connection/login failure occurred, bot initialization failed, or an unhandled .NET exception occurred. |
| `130` | Cancelled with Ctrl+C. |

When several bots run, the process exit code is the highest exit code of all bots.
