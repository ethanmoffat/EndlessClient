EndlessClient
=============

An open source client for Endless Online written in C#

![alt text](http://i.imgur.com/s0MpbG1.gif "Slow 9MB GIF incoming!")

Note: any glitches you can find in the above GIF prove how much of a work in progress this is. But it looks pretty cool so far, amirite??

#### Jump to:
 - [Download and Play](#Download)
 - [What is there so far?](#SoFar)
 - [What is left to do?](#ToDo)
 - [Running the game - Additional Info](#AdditionalGameInfo)
 - [Changes from the Original Client](#Changes)

<a name="Download" />Download+Play
-------------

There is a [Release Binary ZIP](https://github.com/ethanmoffat/EndlessClient/blob/master/Release.zip?raw=true) available for download for those not interested in building the source. It is up to date as of 2/7/15.

You will need to install the [XNA Game Framework](http://www.microsoft.com/en-us/download/details.aspx?id=20914) and the [.NET framework 4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17718) (if you don't have .NET installed already).

Gameplay is pretty simple since you can't do much yet. Walking is done with the arrow keys. Attacking is done by holding left or right CTRL key. Mouse is used for interacting with items that drop on the map (single-click to pick up). You can click+drag items around the inventory as well. Double-click inventory items to equip them, or open the "Paperdoll" and drag+drop between the inventory and the active equipment. You can right-click other players to get a menu of interactive options for that player too.

I encourage everyone to try and break things and file bug reports (see project "Issues"). The more testing the better!

<a name="SoFar" />What is there so far?
---------------------

The primary thing that's been completely is the basic framework for running and displaying the game. I have a solid custom-built UI controls library, and liberal use of the XNA Game Component model. Socket connections for sending/receiving data to/from the server are all set up and surprisingly stable (except for when the server closes a connection, but we're ignoring that for now). The framework that is there also allows for easy addition of different Packet handlers, which means more features for the game client can be added without much effort.

**The game is somewhat playable.** This is the most important thing you need to know. It is more than a foundation, and less than a fully fledged game. The important thing is that it is a work in progress and constantly being improved.

Here's a list of some things that can be done:
 - Character rendering/walking
 - NPC rendering/walking
 - Map rendering/scrolling during walk
 - Doors/warping to different maps
 - Talking - global, local, player commands (#usage #find #loc)
 - **Nearly completed** attacking - the damage is done, NPCs are killed, and items may be dropped. Damage dealt and health remaining are shown above the NPC/character being hit. Character clothes and weapons don't render properly during this yet.
 - Animated map tiles
 - Item inventory and paperdoll display, equipping/unequipping items
 - Stats display and leveling up stats (str/int/wis/agi/con/cha)
 - Stat bars for main player in HUD (hp/tp/sp/tnl)
 - Map chest interaction
 - Interaction with other players via right-clicking them (limited)
 - All pre-game menus, logging in, creating/deleting characters, creating account, etc.

<a name="ToDo" />What's Left to do?
------------------

There is a HUGE list of things left to do. The client is in its infant stages and is a constantly evolving work in progress. What this means is that as features are implemented, I make an attempt to implement them as completely and accurately as possible before moving on to the next thing. The pre-game menus and UI are completely finished, besides a few small bugs that I'm sure to find. The framework for adding new components is there as well.

Here's a working but incomplete list of things I want to get to in this client (strikethrough means completed):
 - Finish attacking - requires ~~damage counter~~, ~~health bars~~, and fixing animations to show up properly
 - Map signs
 - Map boards
 - ~~Map chests~~
 - Map chairs
 - Map bank vaults
 - ~~Right-click players on map~~
 - Cast spells and spell animations
 - Finishing HUD panels -  ~~'view minimap' toggle~~, spells, online players, parties, spells, settings, help
 - ~~HUD meters - HP/SP/TP/TNL~~
 - HUD Quests/Exp info
 - HUD Friend/Ignore lists
 - Player emotes
 - NPC interaction on maps (shops, inns, quests)
 
Here's a working list of things I want to add that would be additional features on top of the original client specs:
 - Use built-in patching system prior to log-in to transfer files: help, gfx, pubs, maps, data, sounds, etc.
 - More than 3 characters per account
 - Unbounded display size
 - Scale HUD with display size without blowing up graphics, so expand viewing range
 - Trade items between characters before logging in or during game play, or shared bank vaults
 - Timed map weather systems (planned for in original but never done)
 - Passive skills (planned for in original but never done)
 - In-game macros (planned for in original but never done)
 
Most things on the above list would require changes to the server software which would significantly distance it from compatibility with eoserv and the original EO client, so they aren't top priority, but would still be pretty cool to have.

<a name="AdditionalGameInfo" />Running the game - additional info
----------------

You will need a copy of eoserv set up and running, or another Endless Online server to connect to. The client binary is now distributed with a config file that points at the eoserv instance I use for testing (ewmoffat.ddns.net:8078). You can also go to eoserv.net and build/configure eoserv to run locally, or change the config file to point to a different server.

GFX files are now distributed with the game client, and any map or pub files are loaded from the server during gameplay.

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
ShowShadows=true #show/hide shadows on map
ShowTransition=true #enable/disable fancy transition on map (custom)
EnableLogging=true #enable/disable logging (Warning: this causes a performance hit and should only be used for debugging purposes)
[CUSTOM]
#seconds after a drop that drop protection will stop (custom)
NPCDropProtectTime=30
PlayerDropProtectTime=5
```

<a name="Changes" />Changes From Original Client (so far)
-------------------------------------

#### Version Numbers

To assist with debugging, I added a version number to the config file so it isn't limited to the hard-coded value that I upload here and can be changed more easily. This provides an easy debugging method for multiple servers that may have custom clients already with a hex-edited version number.

#### Map Transitions

Since the transition was pretty quick between maps, I added a cool little animation that slowly fades tiles in starting from the player and moving outward. This can be disabled in the config file if you don't like it.

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
