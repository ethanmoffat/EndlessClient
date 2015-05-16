EndlessClient
=============

An open source client for Endless Online written in C#

![alt text](http://i.imgur.com/s0MpbG1.gif "Slow 9MB GIF incoming!")

I made a newer GIF but the image embed isn't working. Here is a link to [the newer GIF with more features](http://i.imgur.com/GTQbwrS.gif).

#### Jump to:
 - [Download and Play](#Download)
 - [Building the Source](#Source)
 - [What is there so far?](#SoFar)
 - [What is left to do?](#ToDo)
 - [Running the game - Additional Info](#AdditionalGameInfo)
 - [Changes from the Original Client](#Changes)
 - [Included Utility Projects](#Utility)

<a name="Download" />Download+Play
-------------

There is a [Release Binary ZIP](https://github.com/ethanmoffat/EndlessClient/blob/master/Release.zip?raw=true) available for download for those not interested in building the source. It is up to date as of 4/20/15.

You will need to install the [XNA Game Framework](http://www.microsoft.com/en-us/download/details.aspx?id=20914) and the [.NET framework 4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17718) (if you don't have .NET installed already).

Gameplay is pretty simple since you can't do much yet. Walking is done with the arrow keys. Attacking is done by holding left or right CTRL key. Mouse is used for interacting with items that drop on the map (single-click to pick up). You can click+drag items around the inventory as well. Double-click inventory items to equip them, or open the "Paperdoll" and drag+drop between the inventory and the active equipment. You can right-click other players to get a menu of interactive options for that player too.

I encourage everyone to try and break things and file bug reports (see project "Issues"). The more testing the better!

<a name="Source" />Building the Source
---------------------

There are a few prerequisites that need to be installed before the source can be built. Primary development environment for this project is VS 2013 w/ Update 2 on Win8.1 x64.

These instructions have been tested in a Virtual Machine running Windows 7 Professional x64 With SP1.

1. Install Windows and run windows update cyclically until there are no more
2. Install Visual Studio 2013 ([the **free** Community Edition also works](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx) if you don't have a licensed full version)
3. Install the [XNA 4.0 Refresh](https://xnags.codeplex.com/releases/view/117230) visual studio extension and its dependencies. These are all included in the linked .zip file (four prereqs + the extension itself)
4. Optionally: Install [JetBrains ReSharper](https://www.jetbrains.com/resharper/) (student licenses are **free**!)
5. Install your favorite git client and fork the latest changes (I highly recommend [Atlassian SourceTree](http://www.sourcetreeapp.com/))
6. Build the solution in VS 2013. Copy the required files to the bin\x86\debug\ or bin\x86\release\ folder (see below)

Note that the game client requires some additional files to be copied to the *bin* directory before the game will successfully launch:

1. Download the Endless Online client from [www.endless-online.com](http://files.endless-online.com/EOzipped028.zip)
2. Copy the data, gfx, mfx, and sfx folders from the linked ZIP archive to the output bin directory before running the game. Otherwise it will fail to launch.
3. Create an additional folder in the bin directory called Config. Copy the [sample configuration from below](#SampleConfigFile) into a file named settings.ini within this directory. Note that the original client had a setup.ini; this has been renamed to settings.ini.
4. Any other files will be downloaded or created as needed (pub, maps, and friend.ini/ignore.ini)

<a name="SoFar" />What is there so far?
---------------------

The client is largely complete. There is a pretty full feature set that allows for many of the original game's operations to be done in the same way. However, there is still a lot left out that has not been integrated into this client as of yet.

Some of the more important features that have been implemented are:
 - Complete implementation of pre-game menus and operations. This includes account creation, logging in, creating/deleting characters, and changing password
 - Character rendering and movement (via arrow keys)
 - Character attacking (rendering for this is work in progress)
 - NPC rendering, movement, talking, and attacking
 - Map rendering, including animated wall and floor tiles
 - Rendering of minimap
 - Warps between maps, and doors that open/shut
 - Chat - global, local, player commands (such as #loc #usage)
 - Item inventory management, item equipping (armor), item use, and interaction with map (dropping)
 - Stats display and leveling up or 'training'
 - Stat bars for main player in HUD (hp/tp/sp/tnl)
 - Chests and private lockers on maps
 - Right-click menus for other players (limited)
 - Friend/Ignore lists
 - "Who is online?" list
 - NPC interaction on maps - limited to shops and bank bob
 - Sound effects and background music. Not all sound effects have been hooked to actual in-game events yet, but the framework is there.
 - Party / group
 - Trading with other players

<a name="ToDo" />What's Left to do?
------------------

Since most of the major components are there that make the game playable, I'm working primarily on resolving bugs, refactoring code, and enhancing usability. 

As far as bugs are concerned, character rendering during attack is not being done properly. This is the most obvious bug that needs to be fixed, but requires a LOT of manual tinkering and is really quite tedious to get 100% right.

Concerning refactoring code, I would like to remove all dependencies on singleton instances of World and EOGame if possible. This would require huge sweeping changes so I may just leave things the way they are since there is no real benefit past the code being cleaner.

Here's the actual to-do list:
 - Character rendering during attack
 - Effects and spells (including skillmasters)
 - Guilds
 - Quests
 - Innkeepers
 - Marriage/Law
 - Sitting - floor and chairs
 - Boards
 - Jukeboxes
 - "Jump" tiles
 - Map Effect (quake, HP/TP drain)
 - Map Spikes
 
Here's a working list of things I want to add that would be additional features on top of the original client specs:
 - Use built-in patching system prior to log-in to transfer files: help, gfx, pubs, maps, data, sounds, etc.
 - More than 3 characters per account
 - Trading items between characters on the same account (with restrictions)
 - Unbounded display size, including scaling HUD with display size
 - Timed map weather systems
 - Passive skills (planned for in original but never done)
 - In-game macros (planned for in original but never done)
 
Most things on the above list would require changes to the server software which would significantly distance it from compatibility with eoserv and the original EO client, so they aren't top priority, but would still be pretty cool to have.

<a name="AdditionalGameInfo" />Running the game - additional info
----------------

You will need a copy of eoserv set up and running, or another Endless Online server to connect to. The client binary is now distributed with a config file that points at the eoserv instance I use for testing (ewmoffat.ddns.net:8078). You can also go to eoserv.net and build/configure eoserv to run locally, or change the config file to point to a different server.

GFX files are now distributed with the game client, and any map or pub files are loaded from the server during gameplay.

<a name="SampleConfigFile" />
##### Config File

Here is a sample configuration file with the available configuration settings that are currently being parsed by the client:

```ini
#Destination host/ip and port to connect to
[CONNECTION]
Host=ewmoffat.ddns.net
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
Interaction=on #I honestly am not sure what this even changes, more testing required
```

<a name="Changes" />Changes From Original Client (so far)
-------------------------------------

#### Version Numbers

To assist with debugging, I added a version number to the config file so it isn't limited to the hard-coded value that I upload here and can be changed more easily. This provides an easy debugging method for multiple servers that may have custom clients already with a hex-edited version number.

#### Map Transitions

Since the transition was pretty quick between maps, I added a cool little animation that slowly fades tiles in starting from the player and moving outward. This can be disabled in the config file if you don't like it.

#### Sound Files

Some of the audio files (sfx) from the original client are malformed. The WAV format includes in the header a length of the file in bytes - and for certain files, this length was different than the actual length of the audio data. The original client was able to read these without a problem, as were programs such as Windows Media Player and Audacity. However, the C# code for loading sound in XNA was throwing an exception for these audio files because of the improper length.

Part of the sound processing involves reading the audio data and rewriting the length to the WAV file if the length in the file is incorrect. This modification will occur for any invalid WAV files.

#### Rendering Hair

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

The core projects are EndlessClient, EndlessClientContent and EOLib. They are the only required projects in order for the game to run.

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

BatchPub is designed to do batch processing of items within the EIF file. The goal behind this was to change all items matching a certain criteria to have the same updated property (for instance, when rendering hair).

#### EOBot

EOBot launches a number of "bot" connections to a server that a) create accounts if they don't exist, b) login, c) create characters if they don't exist, and d) get the characters in game.

Once the characters are in-game further code can be added to make them do whatever. Currently, they will send a party request to 'testuser' (if testuser is logged in) which was being used to test functionality of large parties.

In the future I would like to be able to have a script processing system in place that allows an interpreter to control the bots that are being run. Anywhere from 1-25 bots can be launched.

#### PacketDecoder

PacketDecoder is built for analysing raw WireShark packet data. It provides a way to decode the raw data and convert the byte stream into values used in EO.

The idea is to be able to copy/paste an init packet (to determine the encrypt/decrypt multiples) and then copy/paste the packet data that needs to be decoded for analysis.