# Touche Tools

Tools for inspecting/authoring TOUCHE.DAT files/game data 
for the adventure game Touché: The Adventures of the Fifth Musketeer. 
Does not include any game data or assets, you 
are expected to provide your own TOUCHE.DAT 
and similar files.

## Why?

The game itself was obscure due to issues around the game's release, 
but it was actually decent. The game is supported by ScummVM, 
which means it is playable on modern machines, and through browsing 
the engine logic in ScummVM I realised that the technical setup of the engine is 
pretty interesting with the amount of flexibility built 
into it (and at the same time, it is in fact annoyingly rigid 
due to changes once deadlines started appearing on the horizon presumably).

Making a small hacky (and probably not very stable) fan-game 
on the engine appealed to me, and I realised that it's not an 
unreasonably huge project, with how the engine is set up; in fact
the biggest part (figuring out the data format/logic) was already 
done by the maintainers of the engine in ScummVM.

## Current State

The library is usable to load/inspect the database file correctly. 
A GUI-based (ImGUI/Veldrid) application has been created to display
this data, as well as provide a debugger to allow playback
of the logic. This application is not feature-complete by any means.

Exporting a loaded database file has been implemented and the 
resulting new DAT file is playable in the original game engine
as well as in the debugger. There are likely minor bugs in the 
exported version, but generally it should match pretty closely.

## TODO List

[x] Debugger: Finish implementing the game engine-specific logic and 
rendering (inventory, giving items, money, action menu).

[x] Debugger: Start implementing conversation system.

[x] Library: Implement 1-to-1 export that re-creates a usable file
from the original game TOUCHE.DAT.

[x] Library: Start figuring out bare-bones set of data based on
knowledge from GUI application.

[x] Library: Set up sample "game" with a set of data that forms a very
basic game setup (multiple rooms, characters, items, conversation)

[x] Editor: Begin implementing higher-level tools to create/import
assets to generate databases (image processing into palettes, 
basic scripting language, walk point definition, etc)

[x] Editor: Finish implementing higher-level tools for game creation
to a point at which a sample game can be fully delivered mostly within
the application itself

[ ] Editor: Use tools to deliver a sample game that demonstrates
all the basic functionality available in the game and is directly
usable with ScummVM