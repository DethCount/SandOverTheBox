# SandOverTheBox
My first Unity 3D project, writen in C#.

Done :
- Basic environnement
- First person view
- Show/hide logs
- UI toolbar to change selected block type

Work in progress:
- Create, move, delete different types of blocks

What may come next:
- Highlight selected toolbar item
- Multiple toolbars
- Change toolbar orientation from settings menu
- Highlight moving block
- Cancel/redo/copy/cut/paste/linear edition
- Third person view
- Skybox
- Terrain generation
- Tile generation
- Merge several blocks
- Use blocks of different base shapes and scales
- Inventory / containers / Items
- Resources
- Mining
- Crafting
- Weapons
- Research
- Industry
- Vehicles
- Blue prints
- NPC trading system
- Planets and orbits
- Gravity area
- Spaceships
- Multiplayer
- Chat
- Player trading system
...

Game Controls:
- Move mouse : move view
- Use arrows or WASD : move player (sorry for non-qwerty keyboards)
- Space : jump
- Left click : create a block at pointed position
- Right click : delete a block at pointed position
- Maintain left click : precisely positionate a new block
- Control (left or right) + maintain left click : move an existing block at pointed position
- 1-0 : select block type at given position in current toolbar

Upcoming controls:
- Shift (left or right) + maintain left click : create a block line from pointed position
- Ctrl + C : select same block type as pointed one
- Ctrl + V : create new block at pointed position
- Ctrl + X : delete pointed block and select its type
- Ctrl + Z : cancel last action (block creation/move/deletion)
- Ctrl + Y : redo last canceled action (block creation/move/deletion)
- Ctrl + Wheel up : switch to previous toolbar
- Ctrl + Wheel down : switch to next toolbar
- Wheel up : In third person view, zoom in OR in first person view, switch to previous weapon/tool/block
- Wheel down : In third person view, zoom out OR in first person view, switch to next weapon/tool/block