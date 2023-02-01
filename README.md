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

ToucheTools.ModApp is a separate simplified console application which
can be used to modify a pre-built TOUCHE.DAT file for specific
purposes: localisation, changing some graphics, etc. This application
should only really be used when the source package is not available.

## Usage

To use the editor, download the latest release [from the releases page](https://github.com/RedMike/touche-tools/releases). 
Extract the ZIP to a folder on your machine, and run ToucheTools.App.exe. 

**To enable running the game in ScummVM from editor:** If you
have ScummVM installed, go to File > Editor Settings and add the path to `scummvm.exe`
to the first box, and `-p {0} touche:touche` to the second box. This
will make the `Publish & Run` option correctly launch ScummVM.

You can load a package or sample by using the options in the File menu. The packages are
stored in the folder with the application, in a `packages` subfolder. The samples are
stored in the folder with the application, in a `samples` subfolder. You can copy one of the
samples to the `packages` folder and modify it as desired.

You can also load a published folder/DAT file (an already published one) for debugging/inspecting by 
using the Load Published options in the File menu. You will not be able to
modify the file as it has already been published, you need to open
the source package for this.

After publishing a TOUCHE.DAT file, you can run it in ScummVM outside the editor 
by doing the following:

1. Create a folder on your machine, e.g. `D:\MyGame\ `
2. Inside that folder create a new folder called DATABASE, e.g. `D:\MyGame\DATABASE\ `
3. Place the TOUCHE.DAT you published in the DATABASE folder, e.g. `D:\MyGame\DATABASE\TOUCHE.DAT `
4. Open Scumm VM
5. Select "Add Game" and select the original folder, e.g. `D:\MyGame\ `
6. It should detect the game as "Touche: The Adventures of the Fifth Musketeer - unknown variant"

## Usage of Library

To use the library directly from a .NET project, download the source code and build the 
ToucheTools DLL. Use `MainLoader` to import an existing DAT file, or otherwise create your own instance
of `DatabaseModel` to represent your game package. Then use `MainExporter` to save
that `DatabaseModel` instance to a new DAT file.

## Usage of Simplified Mod App

This application is not recommended to be used unless you're trying to modify
a package for which you do not have a source package (e.g. the original retail game).
This application is provided for those limited purposes (localisation, bugfixes, etc).

To use the simplified mod app, download it 
[from the releases page](https://github.com/RedMike/touche-tools/releases).
Extract the ZIP to a folder on your machine, and run ToucheTools.ModApp.exe.
It will ask you for the path to the file you want to modify, e.g.
`D:\MyGame\DATABASE\TOUCHE.DAT`. Paste it in and hit Enter, then 
follow the instructions.

In the first step, the app will generate a lot of CSV files 
that contain the original text in the DAT file you provided. 
Modify the CSV files with your choice of text editor. Note that
if the new text strings are consistently too long, it may not
be possible to run the export because of size limits in the game engine.

In the second step, the app will read in the CSV files generated previously
and apply any changes you have made in them to the loaded DAT file
then create a **new** DAT file containing your modifications. The name
of the new DAT file is based on the CSV file names, so you can re-run
on the same set of CSV files to replace the DAT file in-place.