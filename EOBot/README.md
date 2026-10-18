# EOBot

EOBot is a command-line tool that runs one or more bots against an Endless Online server. Each bot either runs a user-provided script written in the EOBot script language or, when no script is given, the built-in legacy trainer bot.

## Quick start

Write a script, for example `hello.eob`:

```
from Moffat.EndlessOnline.SDK.Protocol.Net.Server map *

if (Login($user, $pass) != LoginReply::Ok) {
    error("login failed")
}
print("logged in as " + $account.$name)
```

Run it with one bot:

```
EOBot.exe host=localhost port=8078 bots=1 account=mybot password=secret script=hello.eob
```

With the default `autoconnect=true`, the bot connects to the server before the script starts. The process exit code is `0` on success and non-zero on failure; see [Exit codes](docs/cli.md#exit-codes).

## Documentation

| Page | Contents |
| --- | --- |
| [Running EOBot](docs/cli.md) | Command-line arguments, validation rules, script arguments, the trainer bot, and exit codes. |
| [Script language basics](docs/language.md) | Syntax, comments, statements and blocks, types, literals and initializers, indexing, operators, and assignment. |
| [Comparisons](docs/comparisons.md) | Results of `==`, `===`, and ordering operators for each combination of types. |
| [Control flow and functions](docs/control-flow.md) | `if`/`else`, loops, `break`/`continue`, labels and `goto`, and user-defined functions. |
| [Enums](docs/enums.md) | Declaring enums and mapping .NET enums such as server reply codes. |
| [Mapping .NET methods](docs/mapping.md) | Calling static .NET methods from scripts, supported types, and overload resolution. |
| [Built-in functions](docs/builtins.md) | Core, collection, string, environment, and game functions. |
| [Built-in variables and state objects](docs/state.md) | Command-line variables and the live `$account`, `$character`, `$mapstate`, and `$map` objects. |
| [Errors and known limitations](docs/errors.md) | Error reporting and current interpreter limitations. |
