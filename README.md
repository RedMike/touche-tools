# Touche Tools

Tools for working with TOUCHE.DAT files/game data for adventure game Touché: The Adventures of the Fifth Musketeer. Does not include any game data or assets, you are expected to provide your own TOUCHE.DAT and similar files.

## Why?

The game itself was obscure due to issues around the game's release, but it was actually decent. The game is actually supported by ScummVM, which means it is actually playable on modern machines, and through browsing the engine logic in ScummVM I realised that the technical setup of the engine is actually pretty interesting with the amount of flexibility built into it (and at the same time, the amount of rigidity once deadlines clearly started eating into development time).

Making a small hacky (and probably not very stable) fan-game on the engine appealed to me, and I realised that it's not an unreasonably huge project, with how the engine is set up.

## Current State

The library and application are usable to load and export the database file with some amount of compatibility already, but without any useful authoring tools in the application. The application also cannot display some useful assets (sound/music) due to the legacy formats.

Importing and subsequently exporting the original game database currently reaches the starting scene all successfully, however there are issues around the conversation system and other small things.