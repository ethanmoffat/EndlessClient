EndlessClient
=============

An open source client for Endless Online written in C#

<img src="http://i.imgur.com/4NNDpJR.png" />

What is there so far?
---------------------

The primary thing that's been completely is the basic framework for running and displaying the game. I have a solid custom-built UI controls library, and make a HEAVY use of the XNA Game Component methodology. Socket connections for sending/receiving data to/from the server are all set up and surprisingly stable. The framework that is there also allows for easy addition of different Packet handlers, which means more features for the game client.

**The game is not yet playable.** This is the most important thing you need to know. It is more than a sandbox, and less than a fully fledged game client. The primary thing is that all the pre-game menus work and do everything they're supposed to do, with the exception of scrolling credits. Additionally, the game allows you to log in and send public, global, and private chat messages to a server. It will also render the map you're loaded on. However, this is the very limit of what the client can do and describes some of the most recent changes that have been made to it: you can't walk, interact with anything, or actually play the game.

What's Left to do?
------------------

There is a HUGE list of things left to do. The client is in its infant stages and is a constantly evolving work in progress. What this means is that as features are implemented, I make an attempt to implement them as completely and accurately as possible before moving on to the next thing. The pre-game menus and UI are completely finished, besides a few small bugs that I'm sure to find. The framework for adding new components is there as well.

As far as game features go, there isn't much I want to leave out or change. A lot of the heavy lifting that has to be done, and that I'm currently working on, involves making the game playable as a bare-bones client. Chatting is pretty solid and map rendering is **sort of** there, but the character can't move or interact with anything yet. That being said, there isn't much in the way of packets that have been handled - but the necessary ones for getting in-game are all there (and I think these are the most challenging to get right).

Running the game
----------------

You will need a copy of eoserv set up and running, or another Endless Online server to connect to. Go to eoserv.net and build/configure eoserv to run locally, or change the code to point to game.endless-online.com. This can be done in the misc.cs file by changing the address "127.0.0.1" to another IP or hostname.

You will also need a copy of **valid** GFX files put into bin\(Debug|Release)\GFX. There are 25 files in all, and they can be copied directly from the normal Endless Online client (gfx directory). This client is available for download at endless-online.com

In addition to GFX you will need the pub files. put into bin\(Debug|Release)\pub. There are 4 files in all, and they can be copied directly from the normal Endless Online client as well (pub directory).

Rendering Hair
--------------

This might seem like a weird topic to touch on, but trust me, it's important. Unlike the other features of the game, I've taken it upon myself to update the file format for the item files to better assist with hair rendering in the client.

This client uses a special method of rendering hair to ensure that masks are rendered one way, hoods/helmets are rendered a second way, and hats that should clip hair are rendered a third way. In order to ensure that your pub file is up-to-date and can render this as designed, run BatchPub to use a batch-processing method of assigning the updated values to the selected items.

NOTE: this is only for connecting to servers where you a) already have any of the relevant items and b) you want it to render properly. The pub files that are modified with this tool should be placed in the server's pub directory.

In step two, configure as follows: Set the field to SubType, and the value to FaceMask (for masks) or HideHair (for helmets/hoods)

In step three, configure as follows: Check the checkbox, set the field to Name, set comparison to regex, and set the value to one of the following:
 - For helmets: `^[A-Za-z ]*[Hh]elm[A-Za-z ]*$`
 - For hoods: `^[A-Za-z ]*[Hh]ood[A-Za-z ]*$`
Pirate hat (ID 314 in standard pubs) also needs to be updated to HideHair. Change search to ==, change the field to ID, and change the value to 314.

For FaceMask updates, the following regex will update the correct items: `^((Frog Head)|([A-Za-z ]*[Mm]ask[A-Za-z ]*))$`
