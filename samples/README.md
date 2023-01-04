# Samples

These samples are meant to be used as reference and documentation
for various parts of either the game engine features or for features of
the editor itself. The generated DAT files should be runnable in ScummVM
(with the `Touche` built-in plugin). The folders should be directly openable
in the editor and everything should already be fully wired up.

(Ideally they should also be runnable in the original Touche game running in DOS,
but that isn't as easy to check.)

All the art, text and game scripts are original, and do not use
the original game or any other licensed game/asset pack/etc.

## Basic Mechanics

This sample demonstrates the following:

* Basic game setup (rooms, programs, key characters)
* Movement by player and key characters
* Hotspots with actions and default actions
* Custom scripts on actions
* Moving from room to room
* Moving from program to program

## Graphical Areas

**STILL TODO.**

This sample demonstrates the following:

* Backgrounds vs Areas vs Room Areas vs Walk Areas
* Using backgrounds for foreground objects
* Using areas for clipping on walk paths for partial rectangular 3D
* Using room areas to change background image
* Using walk areas to achieve partial non-rectangular 3D

## Movement/View Types

**STILL TODO.**

This sample demonstrates the following:

* Typical perspective view (Y/Z is Y/depth; both Y and Z change)
* Top-down mock view (map/overworld view; Y changes, Z is static)
* Sideways mock depth animation (going in/out of screen on the spot; Y is static, Z changes)
* Complex 3D-looking paths (combinations of types of views)
* Key Character following logic 

## Conversations

**STILL TODO.**

This sample demonstrates the following:

* Conversation setup (multiple choices, altering choices, etc)
* Complex conversation choice logic (movement, animation, new key characters)
* Requirement for inventory to be visible on-screen when conversations are enabled
* Special conversation screen (similar to Gabriel Knight 1 dialogue screens)

## Item Inventory

**STILL TODO.**

This sample demonstrates the following:

* Hiding/showing inventory
* Inventory setup (items/icons, actions that award or change items, inventory actions, etc)
* Item use on hotspots
* Checking item ownership

## Multiple Item Inventories

**STILL TODO.**

This sample demonstrates the following:

* Multiple inventories per key character (with restrictions)
* Graphically display inventory from another on-demand
* Giving items (and associated issues and complexity)

## Money Inventory

**STILL TODO.**

This sample demonstrates the following:

* Money (or countable inventory) setup (initialising, changing, etc)
* Money display functionality
* Money give/count functionality

## Disable Money Inventory

**STILL TODO.**

This sample demonstrates an approach to hiding the built-in money display usefully,
which is a workaround and requires changes/restrictions to art for inventory area.




