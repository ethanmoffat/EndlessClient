EndlessClient
=============

[![Build Status](https://ethanmoffat.visualstudio.com/EndlessClient/_apis/build/status/EndlessClient%20Build?branchName=master)](https://ethanmoffat.visualstudio.com/EndlessClient/_build/latest?definitionId=14&branchName=master) [![Nuget](https://badgen.net/nuget/v/EOLib)](https://badgen.net/nuget/v/EOLib)

An open source client for Endless Online written in C#

#### License Update

As of 2020-05-09, this project is relicensed under the MIT license. Older versions of the code are still licensed under the GNU GPLv2 license. The tag `gplv2` has been added to the final commit for which this project is licensed under the GPLv2.

#### Jump to:
 - [Contributing](contributing.md)
 - [Getting started](#GettingStarted)
 - [Todo list](#ToDo)
 - [New features (also todo)](#NewFeatures)
 - [Changes from the Original Client](#Changes)
 - [Included Utility Projects](#Utility)
 - [EOBot](#EOBot)

<a name="GettingStarted" />Getting started
-------------

### Dependencies

Source builds require Visual Studio, the .Net 6.0 SDK, and the .Net 3.1 runtime (for building content with the MonoGame content builder pipeline tool). Other dependencies are installed via Nuget. MonoGame no longer needs to be installed ahead of time!

.Net 6.0 runtime is required to run the pre-built binary.

On Linux, the `ttf-mscorefonts-installer` package is required. Consult your distribution's package manager documentation for instructions on how to install this.

### Pre-built binary

See [releases](https://github.com/ethanmoffat/EndlessClient/releases) on GitHub for Linux and Windows binaries. .Net 6.0 runtime must be installed.

### How to play

Download the appropriate [release](https://github.com/ethanmoffat/EndlessClient/releases) for your platform, then copy the data directories (data, gfx, jbox, mfx, sfx) from whichever client you normally use to EndlessClient's unzip location. Run EndlessClient by double-clicking the icon (any platform) or running `./EndlessClient` (Linux).

### Building from source

After installing, clone (or fork+clone) this repository locally and open the solution in your IDE of choice for your platform.

### Building on Mac

1. Download and install the [.NET 6.0 SDK (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.403-macos-x64-installer).
2. Link the binary to so it's in path `sudo ln -s /usr/local/share/dotnet/x64/dotnet /usr/local/bin/dotnet`
3. Run `dotnet build /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained EndlessClient`
4. The build will fail do to a using alias already being declared
4. Run `echo '' > EndlessClient/obj/Debug/net6.0-macos/osx-x64/EndlessClient.GlobalUsings.g.*.generated.cs`
5. Run the build again `dotnet build /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained EndlessClient`

<a name="Todo" />Todo list
---------------------

See the Github issues for planned features. Anything marked with the 'in progress' label is actively being worked on.

<a name="NewFeatures" />New features (also todo)
------------------

Here's a working list of things I want to add that would be additional features on top of the original client specs:
 - Use built-in patching system prior to log-in to transfer files
 - More than 3 characters per account
 - Trading items between characters on the same account
 - Better display scaling, resizable display
 - Timed map weather systems
 - Passive skills
 - Better inventory

<a name="Changes" />Changes From Original Client
-------------------------------------

#### Command-line arguments

 **--host <server>** Overrides the server set in the config file with a different value. Convenient for testing against different servers from Visual Studio, since the build process will overwrite the configuration file in the output directory.

 **--port <port>** Overrides the port set in the config file with a different value.

 **--version <version>** Overrides the version set in the config file or hard-coded into the client. Convenient for connecting to different servers that might require different version numbers.

 **--account_delay_ms <value>** Sets the delay when creating an account. Some servers enforce a specific limit. Defaults to 2 seconds if unset.

#### Version Numbers

For easily switching servers, there's a version number config setting so it isn't limited to the hard-coded value build into the client by default. This can be set by server operators, or you can use the `--version` command line argument.

#### Map Transitions

Since the transition was pretty quick between maps, I added a cool little animation that slowly fades tiles in starting from the player and moving outward. This can be disabled in the config file if you don't like it.

#### Sound Files

Some of the audio files (sfx) from the original client are malformed. The WAV format includes in the header a length of the file in bytes - and for certain files, this length was different than the actual length of the audio data. The original client was able to read these without a problem, as were programs such as Windows Media Player and Audacity. However, the C# code for loading sound in XNA was throwing an exception for these audio files because of the improper length.

Part of the sound processing involves reading the audio data and rewriting the length to the WAV file if the length in the file is incorrect. This modification will occur for any invalid WAV files.

#### Rendering Hair

A [configuration file](EndlessClient/ContentPipeline/HairClipTypes.ini) is included with that controls how hat items are rendered. This file is based on the EO main hat items. For example:

```ini
186 = Facemask # Bandana
187 = Standard # Mystic hat
188 = HideHair # Hood
```

Item ID 186 will render as a facemask (below hair), 187 will render over hair, and 188 will hide hair entirely.

<a name="Utility" />Included Utility Projects
-------------

There are a few other projects included with the EndlessClient solution that are designed to make the development process much easier.

#### Core

The core projects are EndlessClient and the EOLib projects under the "Lib" solution directory. They are the only required projects in order for the game to run.

#### Test

Any projects with a ".Test" suffix in the name contain unit tests. These will be expanded for as much code coverage as possible.

#### BatchMap

BatchMap is designed to do batch processing and error correction on the original EMF files. When running EOSERV with the default map files, a number of warning messages during map loading will pop up. BatchMap processes files and corrects these errors so that the output of EOSERV is much less verbose when starting up.

BatchMap corrects for a number of errors, including:
 - Tiles out of map bounds
 - Warps out of map bounds
 - NPC spawn using non-existent NPC
 - NPC spawn out of map bounds
 - NPC spawn is invalid (ie NPC may not be able to spawn in area)
 - Chest spawn using non-existent item
 - Chest spawn pointing to non-chest

BatchMap can easily be modified to take care of other batch processing tasks, such as dumping out values in map files for debugging or comparison.

#### BatchPub

BatchPub is designed to do batch processing of items within the EIF file. It can be used to bulk change properties in the pubs based on certain criteria.

#### EOBot

EOBot has recently been updated with an interpreter to make scripting of bots possible. The scripting language is the unholy offspring of javascript and php (really just inspired by the syntax of those languages). Run `EOBot --help` without any arguments to see more information on the command-line parameters it expects.

The default behavior of EOBot when not running in script mode is as a TrainerBot. See `TrainerBot.cs` for more details on the implementation.

EOBot is used by https://www.github.com/ethanmoffat/etheos to execute integration tests against server instances.

#### PacketDecoder

PacketDecoder analyzes raw WireShark packet data. It provides a way to decode the raw data and convert the byte stream into values used in EO.
