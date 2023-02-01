# Touche Tools

Tools for inspecting/authoring games for the engine built for the 
adventure game Touché: The Adventures of the Fifth Musketeer. 

Does not include any game assets or functionality from the original game.

The resulting game files (TOUCHE.DAT) are runnable in both ScummVM and the 
original game engine (in DOS).

## Components

A .NET library called ToucheTools is included which is used for the basic
data parsing/exporting. This library can be used to create custom scripts
or applications that need to modify/inspect TOUCHE.DAT files.

ToucheTools.App is the editor application which you can use to create,
modify, or inspect game packages or TOUCHE.DAT files.

ToucheTools.ModApp is a separate simplified editor application which
can be used to modify in-place a built TOUCHE.DAT file for specific
purposes: localisation, changing some graphics, etc. This application
should only really be used when the source package is not available.

## Usage

To use the library, download the source code and build the DLL. (NuGet package TODO)

To use the editor, download the latest release [from the releases page](https://github.com/RedMike/touche-tools/releases). 
Extract the ZIP to a folder on your machine, and run ToucheTools.App.exe. If you
have ScummVM installed, go to File > Editor Settings and add the path to `scummvm.exe` to 
enable running it from the editor.

You can load a package or sample by using the options in the File menu.

You can also load a published folder/DAT file (an already published one) for debugging/inspecting by 
using the Load Published option in the File menu.

After publishing a TOUCHE.DAT file, you can run it in ScummVM by doing the following:

1. Create a folder on your machine, e.g. `D:\MyGame\ `
2. Inside that folder create a new folder called DATABASE, e.g. `D:\MyGame\DATABASE\ `
3. Place the TOUCHE.DAT file in the DATABASE folder, e.g. `D:\MyGame\DATABASE\TOUCHE.DAT `
4. Open Scumm VM
5. Add Game and select the original folder, e.g. `D:\MyGame\ `
6. It should detect the game as "Touche: The Adventures of the Fifth Musketeer - unknown variant"

