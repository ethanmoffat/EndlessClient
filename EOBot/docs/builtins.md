# Built-in functions

[EOBot documentation](../README.md)

All built-in function names are read-only. Arguments must match the listed types; passing a value of the wrong type or `undefined` is a script error (exit code 3). Functions listed as returning nothing cannot be used as values.

## Core

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `print` | `value` (any) | nothing | Writes the value to the console (prefixed with `[    ] `). See [Variables and types](language.md#variables-and-types) for formatting. |
| `len` | `value` (any) | int | Number of elements of an array or dictionary, or characters of a string. `-1` for other types. |
| `error` | `message` (string) | nothing | Stops the script with an error message and exit code 4. |
| `sleep` | `ms` (int) | nothing | Waits for the given number of milliseconds. |
| `time` | none | string | Current local time in the system's long time format (for example `7:24:01 PM`). |
| `beep` | `frequency` (int), `duration` (int) | nothing | Plays a console beep (Windows only; does nothing on other platforms). |

## Collections

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `array` | `size` (int) | array | New array containing `size` `undefined` elements. |
| `dict` | none | dictionary | New empty dictionary. |
| `object` | none | object | New empty object. |
| `append` | `array`, `value` (any) | nothing | Adds `value` to the end of `array`. |
| `insert` | `array`, `index` (int), `value` (any) | nothing | Inserts `value` at `index`. |
| `remove` | `array`, `value` (any) | bool | Removes the first element equal to `value` with `==` and returns whether an element was removed. Elements that cannot be compared with `value` (for example an int and an array) are skipped. |
| `removeat` | `array`, `index` (int) | nothing | Removes the element at `index`. |
| `clear` | `array` | nothing | Removes all elements. |
| `contains` | `haystack` (any), `needle` (any) | bool | For an array: whether an element equals `needle` (matched like `remove`). For a dictionary: whether the key `needle` exists (converted to a string like an index, so `contains($d, E::A)` finds the key `"E::A"`). Otherwise: whether the string form of `haystack` contains the string form of `needle` (`contains(123, 2)` is `true`). |

## Strings and numbers

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `lower` | `s` (string) | string | Lowercase copy. |
| `upper` | `s` (string) | string | Uppercase copy. |
| `trim` | `s` (string) | string | Copy without leading/trailing whitespace. |
| `split` | `s` (string), `delimiter` (string) | array | Array of the substrings of `s` separated by `delimiter`. |
| `parse` | `s` (string) | int | Parses an integer; returns `0` if `s` is not a valid integer. |
| `abs` | `n` (int) | int | Absolute value. |
| `min` | `a` (int), `b` (int) | int | Smaller of two values. |
| `max` | `a` (int), `b` (int) | int | Larger of two values. |
| `rand` | `min` (int), `max` (int) | int | Uniformly distributed random number in the range [`min`, `max`). Returns `min` when `min` equals `max`; `max` less than `min` is an error. |

## Environment and files

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `getenv` | `name` (string) | string | Value of an environment variable, or `undefined` if it is not set. |
| `setenv` | `name` (string), `value` (string) | nothing | Sets an environment variable. |
| `readall` | `path` (string) | array | Lines of the text file at `path` as an array of strings. |

`getenv` and `setenv` use the EOBot process environment. Values set with `setenv` are visible to every bot in the same run, which makes them usable for sharing state between bots, but they are not persisted after the process exits.

## Connection and pre-game

These functions communicate with the server and return the server's reply as an int. Map the reply enums (see [Mapping .NET enums](enums.md#mapping-net-enums)) to compare by name.

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `Connect` | `host` (string), `port` (int) | nothing | Connects to the server and performs the INIT handshake using `$version`. Fails with an error (exit code 4) if the connection fails. Not needed when `autoconnect=true`. |
| `Disconnect` | none | nothing | Disconnects from the server. |
| `CreateAccount` | `user` (string), `password` (string) | int (`AccountReply`) | Checks the account name and creates the account. |
| `Login` | `user` (string), `password` (string) | int (`LoginReply`) | Logs in to an account. |
| `CreateAndLogin` | `user` (string), `password` (string) | int (`LoginReply`) | Creates the account; if it was created or already exists, logs in and returns the login reply. Otherwise returns `LoginReply::WrongUser`. |
| `ChangePassword` | `user` (string), `oldPassword` (string), `newPassword` (string) | int (`AccountReply`) | Changes the account password. |
| `CreateCharacter` | `name` (string) | int (`CharacterReply`) | Creates a character on the logged-in account. |
| `DeleteCharacter` | `name` (string), `force` (bool) | int (`CharacterReply`) | Deletes a character. When `force` is `false`, asks for confirmation on the console (`y` to confirm) and returns `CharacterReply::NotApproved` if declined. Returns `255` if the character does not exist. |
| `LoginCharacter` | `name` (string) | nothing | Logs in to the game as the named character (case-insensitive), downloading map and pub files as needed. If the account has no characters, a character with that name is created first. |

## Game loop and pathing

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `Tick` | none | nothing | Processes pending game events (packets received from the server). Call it regularly in loops so that state objects stay current. Action functions below tick automatically. |
| `GetPathTo` | `x` (int), `y` (int) | array | Path from the character's current position to (`x`, `y`) as an array of objects with `$x` and `$y` members. |
| `GetPathToAvoidingWarps` | `x` (int), `y` (int) | array | Same as `GetPathTo`, but the path does not pass over warp tiles. |
| `GetCellState` | `x` (int), `y` (int) | object | State of a map cell (see [Cell state](state.md#cell-state)). |

## Character actions

Each action sends the request to the server and then waits/ticks for the listed time.

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `Face` | `direction` (int or `Direction` enum) | nothing | Turns to face a direction (0 down, 1 left, 2 up, 3 right). Waits 120 ms. |
| `Walk` | none | bool | Walks one tile in the direction the character is facing. Returns `false` if the target tile is not walkable, `true` otherwise. Waits 480 ms. The character's position is updated locally. |
| `Attack` | none | nothing | Attacks in the facing direction. Waits 600 ms. |
| `Sit` | none | nothing | Sits down or stands up. Waits 600 ms. |
| `Cast` | `spellId` (int) | nothing | Prepares the spell, waits for its cast time (480 ms per cast time unit), casts it on the character itself, then waits 600 ms. |

## Items

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `UseItem` | `itemId` (int) | nothing | Uses an inventory item. Waits 600 ms. |
| `Drop` | `itemId` (int), `amount` (int) | nothing | Drops items at the character's position. Waits 600 ms. |
| `Pickup` | `index` (int) | nothing | Picks up a map item by its unique index (`$mapstate.$items[i].$index`, not the item ID). An index not present on the map is an error. Waits 600 ms. |
| `Junk` | `itemId` (int), `amount` (int) | nothing | Destroys items from the inventory. Waits 600 ms. |

## Party and chat

| Function | Parameters | Returns | Description |
| --- | --- | --- | --- |
| `JoinParty` | `characterId` (int) | nothing | Sends a request to join the party of the character with the given ID. Does not wait or tick. |
| `Chat` | `text` (string) | nothing | Sends a local (public) chat message. Waits 120 ms. |
