# Touche Tools

Tools for inspecting/authoring games for the engine built for the 
adventure game Touché: The Adventures of the Fifth Musketeer. 

Does not include any game assets or functionality from the original game.

## Why?

The game itself was obscure due to issues around the game's release, 
but it was actually decent. It is supported by ScummVM, which means it is 
playable on many modern machines, and through browsing the engine logic 
in ScummVM I realised that the technical setup of the engine is 
pretty interesting with the amount of flexibility built into it 
(and at the same time, it is in fact annoyingly rigid due to realistic
compromises to get the game released).

Making a small hacky (and probably not very stable) fan-game 
on the engine appealed to me, and I realised that it's not an 
unreasonably huge project, with how the engine is set up; in fact
the biggest part (figuring out the data format/logic) was already 
done by the maintainers of the engine in ScummVM.

## Current State

The library is usable to load/inspect the database file correctly. 
A GUI-based (ImGUI/Veldrid) application has been created to display
this data, as well as provide a debugger to allow playback
of the logic. This application is not feature-complete but improving.

Exporting a loaded database file has been implemented and the 
resulting new DAT file is playable in the original game engine
as well as in the debugger. There are likely minor bugs in the 
exported version, but generally it should match pretty closely.

Authoring a new game is possible at this point, although some engine
features may not be implemented yet to the point of using them.
All the basic gameplay mechanics are in, although some need complex
instructions to use (e.g. conversations, giving items, etc).

Current focus is on building the mechanics out via implementing them 
in the samples, and on improving the actual editor functionality.