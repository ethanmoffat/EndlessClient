EndlessClient
=============

An open source client for Endless Online written in C#

NOTE: This project has been on GitHub since the end of July, 2014. If you're looking for the more feature-complete version of the code base, see the old_code branch (now protected)

## Build Status

[![Build status](https://ethanmoffat.visualstudio.com/EndlessClient/_apis/build/status/EndlessClient%20Gated%20Build)](https://ethanmoffat.visualstudio.com/EndlessClient/_build/latest?definitionId=7)

#### Jump to:
 - [Download and Play](#Download)
 - [Building the Source](#Source)
 - [Obtaining Copyrighted Files](#CopyrightedFiles)
 - [What is there so far?](#SoFar)
 - [What is left to do?](#ToDo)
 - [Running the game - Additional Info](#AdditionalGameInfo)
 - [Changes from the Original Client](#Changes)
 - [Included Utility Projects](#Utility)

<a name="Download" />Download+Play
-------------

ZIP file of the [Release Binary](https://github.com/ethanmoffat/EndlessClient/blob/master/Release.zip?raw=true) is available for download for those not interested in building the source. It is up to date as of 11 May 2016.

The game is able to run independently of a MonoGame installation. The only requirement is .Net 4.5, which is available with installations of Windows 8 and later. Windows Vista and Windows 7 users will [need to install .Net 4.5](https://www.microsoft.com/en-us/download/details.aspx?id=30653) if it (or a later version) is not already installed.

**Special Note**: Copyrighted sound, graphics, and other data files are not included in the release zip. See [how to obtain the original copyrighted files](#CopyrightedFiles) from the original client for more information.

<a name="Source" />Building the Source
---------------------

**Note:** The solution was recently updated to support .Net 4.6.2. This means that Visual Studio 2013 will no longer be able to compile the code. Visual Studio 2015 is now the minimum supported version.

There are a few prerequisites that need to be installed before the source can be built. The primary development environment is Visual Studio 2015 (with Update 3) on Windows 10 Professional x64. The solution should be compatible with other IDEs that support MonoGame.

1. Install Windows

  a. Run Windows Update and reboot cyclically until Windows is up to date
  
2. Install Visual Studio ([2015 Community](https://go.microsoft.com/fwlink/?LinkId=691978&clcid=0x409))

  a. Optional, but recommended: For Visual Studio 2015, install [Visual Studio 2015 Update 3](https://go.microsoft.com/fwlink/?LinkId=691129)
  
  b. Optional, but *highly* recommended: Install [JetBrains ReSharper](https://www.jetbrains.com/resharper/download/) (student licenses are free with a .edu address!)
  
  c. Optional, but recommended: Install Productivity Power Tools ([2015](https://visualstudiogallery.msdn.microsoft.com/34ebc6a2-2777-421d-8914-e29c1dfa7f5d))

3. Install [MonoGame 3.5](http://www.monogame.net/2016/03/17/monogame-3-5/)

4. Install a git client (I use and recommend [Atlassian SourceTree](https://www.sourcetreeapp.com/))

5. Clone the repo to your computer

6. Build the source in Visual Studio

7. Copy the required files from the original game client to the bin/Debug or bin/Release directory

<a name="CopyrightedFiles" />Obtaining additional copyrighted files
---------------------

Note that the game client requires some additional files to be copied to the *bin* directory before the game will successfully launch:

1. Download the [Endless Online client](http://cache.tehsausage.com/EOzipped028.zip). This link points at a ZIP file hosted by Sausage (author of EOSERV). EIRC.exe may be a false-positive flag by some antivirus scanners.
2. Copy the data, gfx, help, jbox, mfx, and sfx folders from the linked ZIP archive to the output bin directory before running the game.
3. Create an additional folder in the bin directory called Config. Copy the [sample configuration from below](#SampleConfigFile) into a file named settings.ini within this directory. Note that the original client had a setup.ini; this has been renamed to settings.ini.
4. Any other files will be downloaded or created as needed (pub, maps, and friend.ini/ignore.ini)

<a name="SoFar" />What is there so far?
---------------------

*For the list of features in the legacy code base, see the README.md file in the old_code branch*

- Connecting to the server (INIT Packet family)
- Support for sequence numbers in the EO protocol (CONNECTION Packet family)
- Logging in with a username/password (LOGIN Packet family)
- Creating an account and changing a password (ACCOUNT Packet family)
- Creating and deleting a character (CHARACTER Packet family)
- Logging in as a character (WELCOME Packet family)

<a name="ToDo" />What's Left to do?
------------------

*For the TODO list based on the original code base, see the README.md file in the old_code branch*

Any features related to packet families not mentioned above still need to be re-implemented. The next step in the rewrite process is to re-write all of the in-game logic, which is a majority of the game. More updates on this as work is completed.
 
Here's a working list of things I want to add that would be additional features on top of the original client specs:
 - Use built-in patching system prior to log-in to transfer files: help, gfx, pubs, maps, data, sounds, etc.
 - More than 3 characters per account
 - Trading items between characters on the same account (with restrictions)
 - Unbounded display size, including scaling HUD with display size (basically, re-vamp the HUD to a more modern design)
 - Timed map weather systems
 - Passive skills (planned for in original but never done)
 - In-game macros (planned for in original but never done)
 
Most things on the above list would require changes to the server software which would significantly distance it from compatibility with eoserv and the original EO client, so they aren't top priority, but would still be pretty cool to have.

<a name="AdditionalGameInfo" />Running the game - additional info
----------------

You will need a copy of eoserv set up and running, or another Endless Online server to connect to. I am self-hosting a test instance of EOSERV at ewmoffat.ddns.net:8078. You can also go to eoserv.net and build/configure eoserv to run locally, or change the config file to point to a different server such as [game.eoserv.net](https://game.eoserv.net/).

*Note: the auto-login button mentioned below is not in the new code base yet. Since the in-game state isn't built up yet, it hasn't been added back in. I will most likely re-enable it once the in-game logic comes together a little more.*

The auto-login button in debug builds has been disabled unless you specify a username/password in a separate config file. Create a file name "local.config" and add the following to it:

```xml
<?xml version="1.0" encoding="utf-8"?>
<appSettings>
	<add key="auto_login_user" value="USERNAME_HERE" />
	<add key="auto_login_pass" value="PASSWORD_HERE" />
</appSettings>
```

<a name="SampleConfigFile" />
##### Config File

Here is a sample configuration file with the available configuration settings that are currently being parsed by the client:

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
