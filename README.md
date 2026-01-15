# PvZ2-Helper-Functions
Convenient functions meant for modding Plants vs Zombies 2. These functions are made by stuff26 and are mainly made for usage with Sen 4.0.

### Redo XFL data.json
The function will take in the data.json of an xfl and a list of media that is part of an xfl. From there it will ask for an ID prefix to use for the sprite IDs. After that, it will rewrite the entire image section of the data.json and overwrite it.

### Redo OBB data.json
The function will take in a data.json from an obb and a packet folder then overwrite the data.json with all the SCGs found in the packet folder.

### Update All Coordinates in Worldmap JSON
The function will take in a world map file and ask how much to increase/decrease the x and y coordinates of every single map piece. From there it will spit out a new worldmap file with the edits.

### Organize Plant Files
The function will take in a packages folder and organize plant types, props, levels, and almanac data based on what is in property sheets

### Make Zombie and GI Templates
The function will turn a template base file and make several copies of the templates inside, perfect for people wanting to add templates for level makers. The function can also spit out a templates base file so you know how to edit it.

### Level Error Checker
The function will take in a level and the packages folder of an obb. From there it will cross check each part of a level to find any missing modules or other potential errors in a level and spit out a message of what is found.

### Check For Errors
The function checks for a few potential errors that can cause issues with XFLs that prevent them from packing or can cause them to glitch out. Some errors scanned for include:
- If any layers have more than one type of symbol
- If there are any tweens
- If there are frames with more than one sprite attached
- If there are any layers with keyframes with gaps of empty keyframes between ones with symbols attached
- If there are any bitmaps in sprite/label/main symbols or other symbols in image symbols

### Split Multi Sprite Layers
The function splits layers that use multiple different symbols, which can prevent XFLs from packing

### Rename Layers
The function renames layers either by number (ascending or descending) or by the symbol they use, useful for those who prefer to be more organized

### Offset Sprite Positions
The function takes in an XFL or individual symbol file then will change all of the sprite positions in the symbol(s) by a certain amount in the x and y direction. This is mainly for animations that don't have any sort of built in offseter in game).

### Remove Empty Layers
Removes layers in a single symbol or XFL that do not contain any keyframes with symbols attached to help clean up

### Convert XFL Type
Changes a split label type XFL into a main_sprite type or vice-versa

### Convert Newspaper Zombie XFL
Converts a Newspaper Zombie XFL into an XFL that is easier for Sen to pack by handling some of the tedious work adjusting. For a more proper guide on how to use it, please see [this video](https://youtu.be/SyAoR_PYe5s?si=rpdpxTouxsNRXQBU) from Hamulous

### Switch Image Names
Renames all the image symbols in an XFL to the media that it uses, or all the media to what image symbol uses it, helpful for fixing inconsistent naming schemes

### Rename All Media
Renames all media and image symbols to a consistent naming scheme of {PREFIX}_1, {PREFIX}_2, et, also helpful for quickly fixing inconsistent naming schemes

### Check Action Frames
Checks through all the action frames from the [Snowie Lib](https://docs.google.com/document/d/1e8vP-VS5o0Nte2eHg6eBsvLpjHiLYPQMMHR5t80dGv4/edit?usp=sharing) that an XFL may use to find any potential errors

### Speed Up Anim.
Speeds up a symbol by removing extra frames

If you have suggestions on what kind of functions to add, feel free to reach out to me either on Discord @stuff26
