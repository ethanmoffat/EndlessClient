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
 - [Changes from the Original Client](#Changes)
 - [Included Utility Projects](#Utility)
 - [EOBot](#EOBot)

<a name="GettingStarted">Getting started</a>
-------------

### Dependencies

Source builds require Visual Studio and the .Net 8.0 SDK. Other dependencies are installed via Nuget. MonoGame no longer needs to be installed ahead of time, and as of the .Net 8.0 upgrade, it no longer requires the .Net 3.1 runtime!

.Net 8.0 runtime is required to run the pre-built binary.

### Pre-built binary

See [releases](https://github.com/ethanmoffat/EndlessClient/releases) on GitHub for Windows/Linux binaries and macOS app (compiled for both Intel and Apple CPU architectures).

### How to play

**Windows/Linux**

Download the appropriate [release](https://github.com/ethanmoffat/EndlessClient/releases) for your platform, then copy the data directories (data, gfx, jbox, mfx, sfx) from whichever client you normally use to EndlessClient's unzip location. Run EndlessClient by double-clicking the icon.

**macOS**

Unzip EndlessClient.app to your hard drive and double-click the app icon. You will need to enable apps from any source to run in your system security settings.

> ⚠️ If the app fails to run with an error "EndlessClient.app is damaged and can't be opened", open a terminal, run the following command: `xattr -dr com.apple.quarantine /path/to/EndlessClient.app`, and relaunch.

Assets for alternate servers may be copied to the app package under `Contents/Resources`. To view package contents, right-click the `EndlessClient.app` file and select "View Package Contents".

Note that the configuration file for the app is stored under `~/.endlessclient/config/settings.ini`. This file will need to be modified to select the new server host and version number.

### Building from source

Clone (or fork+clone) this repository locally and open the solution in your IDE of choice for your platform. VSCode is supported on all OS flavors. Visual Studio 2022 is supported on Windows.

The .Net 8.0 SDK is required. Install the correct version for your platform/architecture. On macOS, XCode 16.0 or later is required (install from the App Store).

> ⚠️ macOS requires a specific version of the .Net SDK: 8.0.204, due to the .Net team forcing minos version of macOS 15 in later SDKs. If you only want to build for macOS 15 and up, you can use the latest SDK.

> ⚠️ If you have previously built EndlessClient, you may need to clear your dotnet tool cache and nuget package cache
>
> Run the following commands:
> - `dotnet nuget locals all --clear`
> - Windows (powershell): `rmdir -recurse -force $env:USERPROFILE\\.dotnet\\toolResolverCache`
> - Linux/macOS: `rm -rf ~/.dotnet/toolResolverCache`

> ⚠️ If you get build errors due to formatting
>
> Run: `dotnet format EndlessClient.sln`

<a name="Todo">Todo list</a>
---------------------

See the Github issues for planned features. Anything marked with the 'in progress' label is actively being worked on.

<a name="Changes">Changes From Original Client</a>
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

#### Music files on Linux

Activating background music on linux takes a bit of extra work, due to the fact that the music tracks are all MIDI files. The following process was tested on Ubuntu 22.04 (baremetal) and Ubuntu 23.10 (VM over RDP). ALSA driver is required - this should be included on any modern desktop Ubuntu installation.

1. Install fluidsynth server and soundfont
   ```
   sudo apt-get install -y fluidsynth fluid-soundfont-gm
   ```
2. Test that fluidsynth can play an MFX track
   ```
   fluidsynth --audio-driver=alsa /usr/share/sounds/sf2/FluidR3_GM.sf2 mfx/mfx001.mid
   ```
3. Run the fluidsynth server in a separate terminal
   ```
   fluidsynth --server --audio-driver=alsa /usr/share/sounds/sf2/FluidR3_GM.sfx
   ```
   * Note that you must type `quit` at the terminal prompt to exit the process cleanly.
4. Launch EndlessClient with `music=on` in the config/settings.ini file. Background music should start playing!

For troubleshooting purposes, follow the guide here: http://www.tedfelix.com/linux/linux-midi.html

#### Resizable Game Display

The in-game experience can be modified for larger displays by setting the following configuration options in `config/settings.ini`:

```ini
[SETTINGS]
InGameWidth=1280
InGameHeight=720
```

This enables "resizable" mode, using the default window size specified. The game window can be further resized. The minimum window size is 640*480.

While resizable mode is enabled, hud panels may be toggled on/off. Multiple hud panels may be visible on-screen at once. Hud panels may also be rearranged by dragging and dropping the panel itself to a different area of the screen. Additionally pressing the keyboard shortcut `alt`+`{num}` will toggle the appropriate panel, with inventory = 1, minimap = 2, etc. `alt`+`~` will bring the news panel back up, but this panel is subsequently hidden whenever another panel is toggled.

Removing either of these configuration options or setting them to zero will disable resizable mode and the in-game experience will remain unchanged.

<a name="Utility">Included Utility Projects</a>
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

#### EOBot

EOBot has recently been updated with an interpreter to make scripting of bots possible. The scripting language is the unholy offspring of javascript and php (really just inspired by the syntax of those languages). Run `EOBot --help` without any arguments to see more information on the command-line parameters it expects.

The default behavior of EOBot when not running in script mode is as a TrainerBot. See `TrainerBot.cs` for more details on the implementation.

EOBot is used by https://www.github.com/ethanmoffat/etheos to execute integration tests against server instances.
