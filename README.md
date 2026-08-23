# PvZ2-Helper-Functions
Convenient functions meant for modding Plants vs Zombies 2. These functions are made by stuff26 and are mainly made for usage with Sen 4.0. Coded entirely in C#.

## ERROR FINDERS

### Check For Errors
The function checks for a few potential errors that can cause issues with XFLs that prevent them from packing or can cause them to misrepresent in game look.

### Check Action Frames
Checks through all the action frames from the [Snowie Lib](https://docs.google.com/document/d/1e8vP-VS5o0Nte2eHg6eBsvLpjHiLYPQMMHR5t80dGv4/edit?usp=sharing) that an XFL may use to find any potential errors

## MODIFIERS

### Offset Sprite Positions
The function takes in an XFL or individual symbol file then will change all of the sprite positions in the symbol(s) by a certain amount in the x and y direction. This is mainly for animations that don't have any sort of built in offseter in game).

### Speed Up Anim.
Speeds up a symbol by removing extra frames

## ORGANIZATION

### Split Multi Sprite Layers
The function splits layers that use multiple different symbols, which can prevent XFLs from packing

### Rename Layers
The function renames layers either by number (ascending or descending) or by the symbol they use, useful for those who prefer to be more organized

### Remake XFL data.json
The function will take in the data.json of an xfl and a list of media that is part of an xfl. From there it will ask for an ID prefix to use for the sprite IDs. After that, it will rewrite the entire image section of the data.json and overwrite it.

### Remove Empty Layers
Removes layers in a single symbol or XFL that do not contain any keyframes with symbols attached to help clean up

### Rename All Media
Renames all media and image symbols to a consistent naming scheme of user choice, also helpful for quickly fixing inconsistent naming schemes

### Remove Unused Items
Recursively removes items not used in the library for organization purposes

### Add Missing Labels
Finds label symbols not found in the DOMDocument and adds them to the end safely, most useful for things such as dummy PAMs

### Reorganize Label Order
Reorganizes the order of labels in the DOMDocument, either alphabetical, by length of each label, or by user input

## REMAKERS

### Convert XFL Type
Changes a split label type XFL into a main_sprite type or vice-versa

### Convert Newspaper Zombie XFL
Converts a Newspaper Zombie XFL into an XFL that is easier for Sen to pack by handling a lot of the tedious work. For a more proper guide on how to use it, please see [this video](https://youtu.be/SyAoR_PYe5s?si=rpdpxTouxsNRXQBU) from Hamulous

## PACKAGES

### Update All Coordinates in Worldmap JSON
The function will take in a world map file and ask how much to increase/decrease the x and y coordinates of every single map piece. From there it will spit out a new worldmap file with the edits.

### Organize Plant Files
The function will take in a packages folder and organize plant types, props, levels, and almanac data based on what is in property sheets

### Redo OBB data.json
The function will take in a data.json from an obb and a packet folder then overwrite the data.json with all the SCGs found in the packet folder.

### Level Error Checker
The function will take in a level and the packages folder of an obb. From there it will cross check each part of a level to find any missing modules or other potential errors in a level and spit out a message of what is found.

### Scan Packages Errors
Scans through multiple different files in packages to check for wrong references, supports [Snowie Lib](https://docs.google.com/document/d/1e8vP-VS5o0Nte2eHg6eBsvLpjHiLYPQMMHR5t80dGv4/edit?usp=sharing) added content as well


## AI DISCLAIMER
AI has been used for the following purposes:
- Writing the code for Animate Elements/DOMDocument Elements/ActionScripts.cs. This is the only file significantly composed of AI written code, but has been adjusted over time by humans
- Reviewing and getting suggestions to improve code. Suggestions were closely reviewed before being implemented and no direct copy and pasting from given AI code was done in large amounts
- Instructing how to implement the XML serialization, with actual code logic being done by human
- Instructing and helping on other small things that haven't been kept track of

The following are examples are things not done by AI:
- Almost all of the code
- Main logic of all the functions
- Bug fixing (aside from ones initially found by AI when asking for ways to improve code)
- Ideas for functions

Overall, this project is not vibe coded at all and I'd say at least 98% of the overall work has been done by humans.
AI has overall been used to help improve pre-existing code and show how to implement certain things.

## CONCLUSION
If you have suggestions on what kind of functions to add, feel free to reach out to me either on Discord @stuff26
