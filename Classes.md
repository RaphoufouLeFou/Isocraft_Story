# Classes descriptions

A short descrition of all the scripts

## Chunks.cs
By L�o

Build the mesh of a chunk

## CustomRigidBody.cs
by L�o

Custom player mouvement and collition, to replace the default ones
## FastNoiseLite.cs
by ?

Generate noise

## Game.cs
by L�o

Blocks classes and informations

## Hotbat.cs
by Rapha�l

Hotbar visual control and scrolling
 - int selectedIndex -> the current selected slot
 - updateHotBar() -> Update selectedIndex with the mouse scrolling or Alpha keys

## Inventory.cs
by Rapha�l

inventory system and functions  
Inventory format : int[9, 4, 2]  
- x(0) = item, v(1) = quantity   
* hided:  
[x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v]  
[x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v]  
[x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v]  
* hotbar:  
[x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v] [x,v]  

functions :  
- AddBlock -> add a block in the inventory and update the hotbar if nessesary
- RemoveBlock -> remove a block from the inventory and update the hotbar if nessesary

## inventoryUI.cs
by Rapha�l

UI inventory functions, like hide ans display

## MapHandler.cs
by L�o

Map generator

## NetworkInfos.cs
by Rapha�l

Network static class to pass information from the menu to the game controller

## NetworkManagement.cs
by Rapha�l

Network management script that start the server or connect to a host.
It also takes care of the pause menu

## NoiseGen.cs
by L�o

nose generation for the blocks type

## Overlay.cs
by Rapha�l

Quick overlay of debug informations like fps and mspf

## ParamUI.cs
by Rapha�l

all settings functions

## Player.cs
by L�o + Rapha�l

Player main script that handle :
- the camera imputs
- player spawning
- Genral updating
- placing / breaking blocks
- partial network sync of the blocks

## PlayerCamera.cs
by L�o

Camera rotation handler

## MainMenu.cs
by Rapha�l

Main menu UI handler