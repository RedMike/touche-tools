# Touche Tools

Tools for inspecting/authoring TOUCHE.DAT files/game data 
for the adventure game Touché: The Adventures of the Fifth Musketeer. 
Does not include any game data or assets, you 
are expected to provide your own TOUCHE.DAT 
and similar files.

## Why?

The game itself was obscure due to issues around the game's release, 
but it was actually decent. The game is actually supported 
by ScummVM, which means it is actually playable on modern 
machines, and through browsing the engine logic in ScummVM 
I realised that the technical setup of the engine is actually 
pretty interesting with the amount of flexibility built 
into it (and at the same time, it is actually annoyingly rigid 
due to changes once deadlines started appearing on the horizon).

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

Exporting the database file is only somewhat implemented currently.
The process through which the data is exported into a small enough
file (due to strict limit in game engine loading) needs improvement
and there is no original source to reference for information on how it
was achieved.

## TODO List

[x] Debugger: Finish implementing the game engine-specific logic and 
rendering (inventory, giving items, money, action menu).

[x] Debugger: Start implementing conversation system.

[ ] Library: Start figuring out bare-bones set of data based on 
knowledge from GUI application.

[ ] Library: Implement 1-to-1 export that re-creates a usable file
from the original game TOUCHE.DAT.