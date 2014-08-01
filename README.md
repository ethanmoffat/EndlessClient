EndlessClient
=============

An open source client for Endless Online written in C#

Running the game
----------------

You will need a copy of eoserv set up and running, or another Endless Online server to connect to. Go to eoserv.net and build/configure eoserv to run locally, or change the code to point to game.endless-online.com. This can be done in the misc.cs file by changing the address "127.0.0.1" to another IP or hostname.

You will also need a copy of **valid** GFX files put into bin\(Debug|Release)\GFX. There are 25 files in all, and they can be copied directly from the normal Endless Online client (gfx directory). This client is available for download at endless-online.com

In addition to GFX you will need the pub files. put into bin\(Debug|Release)\pub. There are 4 files in all, and they can be copied directly from the normal Endless Online client as well (pub directory).

Rendering Hair
--------------

This client uses a special method of rendering hair to ensure that masks are rendered one way, hoods/helmets are rendered a second way, and hats that should clip hair are rendered a third way. In order to ensure that your pub file is up-to-date and can render this as designed, run BatchPub to use a batch-processing method of assigning the updated values to the selected items.

NOTE: this is only for connecting to servers where you a) already have any of the relevant items and b) you want it to render properly.

In step two, configure as follows: Set the field to SubType, and the value to FaceMask (for masks) or HideHair (for helmets/hoods)

In step three, configure as follows: Check the checkbox, set the field to Name, set comparison to regex, and set the value to one of the following:
 - For helmets: ^[A-Za-z ]*[Hh]elm[A-Za-z ]*$
 - For hoods: ^[A-Za-z ]*[Hh]ood[A-Za-z ]*$
Pirate hat (ID 314 in standard pubs) also needs to be updated to HideHair. Change search to ==, change the field to ID, and change the value to 314.

For FaceMask updates, the following regex will update the correct items: ^((Frog Head)|([A-Za-z ]*[Mm]ask[A-Za-z ]*))$
