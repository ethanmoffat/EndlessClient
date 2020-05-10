EndlessClient
=============

[![Build status](https://ethanmoffat.visualstudio.com/EndlessClient/_apis/build/status/EndlessClient%20Gated%20Build)](https://ethanmoffat.visualstudio.com/EndlessClient/_build/latest?definitionId=7)

An open source client for Endless Online written in C#

#### Want more features?
This project has been on GitHub since the end of July, 2014. It is currently suffering through a rewrite of the code. If you're looking for the more feature-complete version of the code base, see the old_code branch (now protected)

#### License Update

As of 2020-05-09, this project is relicensed under the MIT license. Older versions of the code are still licensed under the GNU GPLv2 license. The tag `gplv2` has been added to the final commit for which this project is licensed under the GPLv2.

#### Jump to:
 - [Getting started](#GettingStarted)
 - [Contributing](contributing.md)
 - [Current feature list](#SoFar)
 - [Todo list](#ToDo)
 - [New features (also todo)](#NewFeatures)
 - [Sample configuration file](#SampleConfigFile)
 - [Changes from the Original Client](#Changes)
 - [Included Utility Projects](#Utility)

<a name="GettingStarted" />Getting started
-------------

### Pre-built binary

Binary releases are not currently available, but may be in the future (due to the recent addition of a CI pipeline).

### Building from source

The only required external dependency is [MonoGame 3.7.1](http://community.monogame.net/t/monogame-3-7-1-release/11173). On Windows, Visual Studio (2015u3+) is used for development. Project Rider is supported on Linux, and Visual Studio 2017 for Mac will work on macOS.

After installing, clone (or fork+clone) this repository locally and open the solution in your IDE of choice for your platform. All additional dependencies should be restored from Nuget and allow the game to launch after building.

<a name="SoFar" />Current feature list
---------------------

- Pre-game menus and dialogs
    - Debug mode for character offset calculation (press F5 from the initial game screen in a debug build)
- Map rendering and warping
- Characters
    - Walk animations
    - Attack animations
    - Equipment display (work in progress)
- NPCs
    - Walk animations
    - Attack animations
    - Speech
- HUD status bars (HP, TP, SP, TNL)
- Chat
    - All chat types properly handled
    - Speech bubbles rendered
- Character stats and training
- Mouse cursor rendering
    - Display of the cursor on the current grid space

<a name="Todo" />Todo list
---------------------

GitHub issues should be used to track progress. See issues list for more detail.
- Finish map rendering
    - Animated walls/tiles
- Finish mouse cursor rendering
    - Character/NPC name rendering
- Mouse interactions
    - Context menu (right-click)
    - Map sign interaction
    - Board interaction
    - NPC interaction (quests, shops)
- Finish character equipment rendering
    - Fix offsets for all equipment types
- Finish NPC alignment to grid
- Map item interaction
    - Item pickup/drop
- Paperdoll
- HUD panels
    - Inventory
    - Online list
    - Party
    - Settings
- Friend/ignore list
- Quest progress/daily exp
- Sit/stand
- Map refresh (F12)
- Sounds/music
    - Need to track down a cross-platform midi player for background music

<a name="NewFeatures" />New features (also todo)
------------------

Here's a working list of things I want to add that would be additional features on top of the original client specs:
 - Use built-in patching system prior to log-in to transfer files: help, gfx, pubs, maps, data, sounds, etc.
 - More than 3 characters per account
 - Trading items between characters on the same account (with restrictions)
 - Unbounded display size, including scaling HUD with display size (basically, re-vamp the HUD to a more modern design)
 - Timed map weather systems
 - Passive skills (planned for in original but never done)
 - In-game macros (planned for in original but never done)

Most things on the above list would require changes to the server software which would significantly distance it from compatibility with eoserv and the original EO client, so they aren't top priority, but would still be pretty cool to have. I will most likely fork the project for these additional changes.


<a name="SampleConfigFile" />Sample configuration file
------------------

Here is a sample configuration file with the available configuration settings that are currently being parsed by the client.

A config file is automatically downloaded as part of the EndlessClient.Binaries nuget package and should have all these settings configured. If you would like to change any of these settings while debugging, edit the file in the packages/EndlessClient.Binaries directory as it will be copied over the file in bin/debug or bin/release.

```ini
#Destination host/ip and port to connect to
[CONNECTION]
Host=ewmoffat.ddns.net #or use game.eoserv.net, enhanced clone of original main server
Port=8078
#override the version sent from the client to the server. For testing with multiple server versions.
[VERSION]
Major=0
Minor=0
Client=28
#individual settings
[SETTINGS]
Music=off #enable/disable background music
Sound=off #enable/disable sound effects
ShowBaloons=on #show/hide chat bubbles on map
ShowShadows=true #show/hide shadows on map
ShowTransition=true #enable/disable fancy transition on map (custom)
EnableLogging=true #enable/disable logging (Warning: this causes a performance hit and should only be used for debugging purposes)
[CUSTOM]
#seconds after a drop that drop protection will stop (custom)
NPCDropProtectTime=30
PlayerDropProtectTime=5
[LANGUAGE]
#0=english 1=dutch 2=swedish 3=portuguese (defaults to english)
Language=0
#note - different keyboard layouts are not going to be supported
[CHAT]
Filter=off  #normal curse filter
FilterAll=on #strict curse filter
LogChat=off  #chat logging is currently not supported
LogFile=CHATLOG.TXT
HearWhisper=on
Interaction=on
```

<a name="Changes" />Changes From Original Client
-------------------------------------

#### Version Numbers

To assist with debugging, I added a version number to the config file so it isn't limited to the hard-coded value that I upload here and can be changed more easily. This provides an easy debugging method for multiple servers that may have custom clients already with a hex-edited version number.

#### Map Transitions

Since the transition was pretty quick between maps, I added a cool little animation that slowly fades tiles in starting from the player and moving outward. This can be disabled in the config file if you don't like it.

#### Sound Files

Some of the audio files (sfx) from the original client are malformed. The WAV format includes in the header a length of the file in bytes - and for certain files, this length was different than the actual length of the audio data. The original client was able to read these without a problem, as were programs such as Windows Media Player and Audacity. However, the C# code for loading sound in XNA was throwing an exception for these audio files because of the improper length.

Part of the sound processing involves reading the audio data and rewriting the length to the WAV file if the length in the file is incorrect. This modification will occur for any invalid WAV files.

#### Rendering Hair

*"This is horrible" - Falco, Star Fox 64*

*I'm not even sure how accurate this section is anymore. Hair rendering will (hopefully) be fixed up to work well with the new code base.*

There are very subtle changes I've made to handling how hair is rendered for this client. Unlike the other features of the game, I've taken it upon myself to update the file format (GASP) for the item files to better assist with hair rendering in the client. I believe the original client had some hard-coded values for certain items that should render a certain way.

EndlessClient uses a special method of rendering hair to ensure that face masks are rendered one way, hoods/helmets are rendered a second way, and hats that should clip hair are rendered a third way. In order to ensure that your pub file is up-to-date and can render this as designed, run BatchPub to use a batch-processing method of assigning the updated values to the selected items. Otherwise, the default pubs will have some weird graphics showing up when hats are equipped.

NOTE: this is only for connecting to servers where you a) already have any of the relevant items and b) you want it to render properly. The pub files that are modified with this tool should be placed in the server's pub directory. If a difference in PUB files is detected client-side, it will request new files from the server and overwrite your local changes.

Open up BatchPub.

In step two, configure as follows: Set the field to SubType, and the value to FaceMask (for masks) or HideHair (for helmets/hoods)

In step three, configure as follows: Check the checkbox, set the field to Name, set comparison to regex, and set the value to one of the following:
 - For helmets: `^[A-Za-z ]*[Hh]elm[A-Za-z ]*$`
 - For hoods: `^[A-Za-z ]*[Hh]ood[A-Za-z ]*$`
Pirate hat (ID 314 in standard pubs) also needs to be updated to HideHair. Change search to ==, change the field to ID, and change the value to 314.

For FaceMask updates, the following regex will update the correct items: `^((Frog Head)|([A-Za-z ]*[Mm]ask[A-Za-z ]*))$`

<a name="Utility" />Included Utility Projects
-------------

There are a few other projects included with the EndlessClient solution that are designed to make the development process much easier.

#### Core

The core projects are EndlessClient, EOLib, and EOLib.Graphics. They are the only required projects in order for the game to run.

#### Test

Any projects with a ".Test" suffix in the name contain unit tests. These will be expanded for as much code coverage as possible.

#### BatchMap

BatchMap is designed to do batch processing and error correction on the original EMF files. When running EOSERV, a number of warning messages during map loading popped up. I created BatchMap to process map files and correct these errors so that the output of EOSERV was much less verbose when starting up.

BatchMap corrects for a number of errors, including:
 - Tiles out of map bounds
 - Warps out of map bounds
 - NPC spawn using non-existent NPC
 - NPC spawn out of map bounds
 - NPC spawn is invalid (ie NPC may not be able to spawn in area)
 - Chest spawn using non-existent item
 - Chest spawn pointing to non-chest

#### BatchPub

BatchPub is designed to do batch processing of items within the EIF file. The goal behind this was to change all items matching a certain criteria to have the same updated property (for instance, when rendering hair, see above).

#### EOBot

EOBot launches a number of "bot" connections to a server that a) create accounts if they don't exist, b) login, c) create characters if they don't exist, and d) get the characters in game.

Once the characters are in-game further code can be added to make them do whatever. Currently, they will send a party request to 'testuser' (if testuser is logged in) which was being used to test functionality of large parties.

In the future I would like to be able to have a script processing system in place that allows an interpreter to control the bots that are being run. Anywhere from 1-25 bots can be launched.

#### PacketDecoder

PacketDecoder is built for analysing raw WireShark packet data. It provides a way to decode the raw data and convert the byte stream into values used in EO.

The idea is to be able to copy/paste an init packet (to determine the encrypt/decrypt multiples) and then copy/paste the packet data that needs to be decoded for analysis.
