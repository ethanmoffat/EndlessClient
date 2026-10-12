# Built-in variables and state objects

[EOBot documentation](../README.md)

## Variables

| Variable | Type | Writable | Description |
| --- | --- | --- | --- |
| `$host` | string | No | `host` argument (null/empty string if not given). |
| `$port` | int | No | `port` argument (`0` if not given). |
| `$user` | string | No | `account` argument. |
| `$pass` | string | No | `password` argument. |
| `$botindex` | int | No | Zero-based index of this bot (useful with `bots` > 1). |
| `$args` | array of strings | No | Arguments after `--` on the command line. |
| `$version` | int | Yes | Client version sent during the INIT handshake. Default `28`. Set it before calling `Connect` (with `autoconnect=true` the connection is made before the script starts). |
| `$account` | state object | No | Logged-in account. |
| `$character` | state object | No | Logged-in character. |
| `$mapstate` | state object | No | Entities on the current map. |
| `$map` | state object | No | Current map file information. |

State objects are read live each time a member is accessed, and all of their members are read-only. They are only valid once the corresponding game state exists (logged in to an account / to a character); accessing them before that causes an error. Lists inside state objects are snapshots: re-read them (for example after `Tick()`) to observe changes.

## `$account`

| Member | Type | Description |
| --- | --- | --- |
| `$name` | string | Account name. |
| `$characters` | array of objects | Characters on the account. Each has `$name` (string). |

## `$character`

| Member | Type | Description |
| --- | --- | --- |
| `$id` | int | Character ID. |
| `$name` | string | Character name. |
| `$map` | int | Current map ID. |
| `$x`, `$y` | int | Current position. |
| `$destx`, `$desty` | int | Destination position while walking. |
| `$direction` | int | Facing direction (0 down, 1 left, 2 up, 3 right). |
| `$admin` | int | Admin level (see `AdminLevel`). |
| `$inventory` | array of objects | Inventory items. Each has `$name` (string), `$id` (int), and `$amount` (int). |
| `$spells` | array of objects | Known spells. Each has `$name` (string), `$id` (int), and `$amount` (int; this is the spell level). |
| `$stats` | object | Character stats: `$hp`, `$maxhp`, `$tp`, `$maxtp`, `$weight`, `$maxweight` (all int). |

## `$mapstate`

| Member | Type | Description |
| --- | --- | --- |
| `$characters` | array of objects | Other characters on the map. Each has `$id`, `$name`, `$map`, `$x`, `$y`, `$direction`. |
| `$npcs` | array of objects | NPCs on the map. Each has `$name` (string), `$x`, `$y`, `$id` (NPC ID), `$direction`, `$index` (map index), `$ismonster` (bool; passive or aggressive NPC), `$incombat` (bool), and `$opponent` (ID of the character it is fighting, or `undefined`). |
| `$items` | array of objects | Items on the map. Each has `$name` (string), `$x`, `$y`, `$id` (item ID), `$amount`, and `$index` (unique index, used with `Pickup`). |

## `$map`

| Member | Type | Description |
| --- | --- | --- |
| `$id` | int | Map ID. |
| `$width` | int | Map width. |
| `$height` | int | Map height. |
| `$warps` | array of objects | Warp tiles. Each has `$x`, `$y`, and `$map` (destination map ID). |

## Cell state

`GetCellState(x, y)` returns an object with these members:

| Member | Type | Description |
| --- | --- | --- |
| `$x`, `$y` | int | Cell coordinates. |
| `$spec` | int | Tile spec of the cell. |
| `$character` | object or `undefined` | Character on the cell (same members as `$mapstate.$characters` entries). |
| `$npc` | object or `undefined` | NPC on the cell (same members as `$mapstate.$npcs` entries). |
| `$item` | object or `undefined` | First item on the cell (same members as `$mapstate.$items` entries). |

## Example

```
from Moffat.EndlessOnline.SDK.Protocol.Net.Server map *

if (CreateAndLogin($user, $pass) != LoginReply::Ok) {
    error("login failed")
}
LoginCharacter($args[0])

while (true) {
    Tick()
    foreach ($npc in $mapstate.$npcs) {
        if ($npc.$ismonster) {
            print("monster " + $npc.$name + " at " + $npc.$x + "," + $npc.$y)
        }
    }
    sleep(1000)
}
```
