EndlessClient
=============

An open source client for Endless Online written in C#

![](http://i.imgur.com/XO4vJPm.png)

What is there so far?
---------------------

The primary thing that's been completely is the basic framework for running and displaying the game. I have a solid custom-built UI controls library, and liberal use of the XNA Game Component model. Socket connections for sending/receiving data to/from the server are all set up and surprisingly stable (except for when the server closes a connection, but we're ignoring that for now). The framework that is there also allows for easy addition of different Packet handlers, which means more features for the game client can be added without much effort.

~~**The game is not yet playable.**~~ **The game is only slightly playable.** This is the most important thing you need to know. It is more than a foundation, and less than a fully fledged game. The primary thing is that all the pre-game menus work and do everything they're supposed to do, with the exception of scrolling credits (which were annoying anyway). Additionally, the game allows you to log in and send public, global, and private chat messages to a server. As of 10/30/14, the latest and greatest updates include the ability to walk around a map, and proper map rendering! However, this is the very limit of what the client can do and describes some of the most recent changes that have been made to it: you can't play the game like with the actual game client.

What's Left to do?
------------------

There is a HUGE list of things left to do. The client is in its infant stages and is a constantly evolving work in progress. What this means is that as features are implemented, I make an attempt to implement them as completely and accurately as possible before moving on to the next thing. The pre-game menus and UI are completely finished, besides a few small bugs that I'm sure to find. The framework for adding new components is there as well.

As far as game features go, there isn't much I want to leave out or change. A lot of the heavy lifting that has to be done, and that I'm currently working on, involves making the game playable as a bare-bones client. Chat, map rendering, character rendering, item inventory, equipping and unequipping items, and dropping items are all paritally there. The next big implementation point is to get started with NPCs. As soon as the basic elements are in, I can work on the features that involve interaction between players - thus making the game playable.

Running the game
----------------

You will need a copy of eoserv set up and running, or another Endless Online server to connect to. The client binary is now distributed with a config file that points at the eoserv instance I use for testing (ewmoffat.ddns.net:8078). You can also go to eoserv.net and build/configure eoserv to run locally, or change the config file to point to a different server. You can also change the default serverer in the misc.cs file by changing the address "127.0.0.1" to another IP or hostname.

GFX files are now distributed with the game client, and any map or pub files are loaded from the server during gameplay.

Rendering Hair
--------------

This might seem like a weird topic to touch on, but there are very subtle changes I've made to handling how hair is rendered for this client. Unlike the other features of the game, I've taken it upon myself to update the file format (GASP) for the item files to better assist with hair rendering in the client. I believe the original client had some hard-coded values for

This client uses a special method of rendering hair to ensure that masks are rendered one way, hoods/helmets are rendered a second way, and hats that should clip hair are rendered a third way. In order to ensure that your pub file is up-to-date and can render this as designed, run BatchPub to use a batch-processing method of assigning the updated values to the selected items.

NOTE: this is only for connecting to servers where you a) already have any of the relevant items and b) you want it to render properly. The pub files that are modified with this tool should be placed in the server's pub directory.

In step two, configure as follows: Set the field to SubType, and the value to FaceMask (for masks) or HideHair (for helmets/hoods)

In step three, configure as follows: Check the checkbox, set the field to Name, set comparison to regex, and set the value to one of the following:
 - For helmets: `^[A-Za-z ]*[Hh]elm[A-Za-z ]*$`
 - For hoods: `^[A-Za-z ]*[Hh]ood[A-Za-z ]*$`
Pirate hat (ID 314 in standard pubs) also needs to be updated to HideHair. Change search to ==, change the field to ID, and change the value to 314.

For FaceMask updates, the following regex will update the correct items: `^((Frog Head)|([A-Za-z ]*[Mm]ask[A-Za-z ]*))$`
