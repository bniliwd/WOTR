# The One Pearl mod for Pathfinder: Wrath of the Righteous

### Requirements: [Mod Menu](https://github.com/WittleWolfie/ModMenu/releases/latest)

Adds The One Pearl, which combines powers of all pearls of power player currently has in their inventory. 

### Quickstart
1. Load into your game
2. Open Settings -> Mod Menu -> The One Pearl -> Press "Receive"
3. Unequip All pearls of power you have on anyone. Otherwise they won't be tracked.
3. Equip newly appeared One Pearl
4. Restore spells as needed.


### Explanation on numbers you see
Mod calculates how many pearls player has in the inventory and combines them into pool.    
Example: Player has 2 pearls of 1st level, 1 pearl of 3th level and 1 pearl of 5th level. All are unused yet today.
That means that could restore potentially:

4 spells of 1st level (all 4 pearls can be used on lvl 1)    
2 spells of 2nd level (pearl of 3th and 5th level can be used here)    
2 spells of 3rd level (pearl of 3th and 5th level can be used here)    
1 spells of 4th level (only pearl of 5th level can be used here)    
1 spells of 5th level (only pearl of 5th level can be used here)    
0 spells of 6th level and above    

And that's how number of pearl usages will look like in conversion menu.

Let's say player wants  to restore 2nd level slot.  
Mod will use pearl of lowest available level that has charges.   
In this example that would be 3rd level pearl.     
After usage numbers will go down to    

3 of 1st level (2 pearls of 1st lvl and 1 pearl of 5th level)    
1 of 2nd level (pearl of 5th level)     
1 of 3rd level (pearl of 5th level)    
1 of 4th level (pearl of 5th level)    
1 of 5th level (pearl of 5th level)   
0 of 6th level and above    


If you enable in settings option to have pearls apply to specific level, the picture (before usage of 3rd) would look like this:

2 of 1st level      
0 of 2nd level    
1 of 3rd level     
0 of 4th level    
1 of 5th level    

### Details

One Pearl automatically synchronizes resources to pearls you have on one pearl equip and on rest. So if you acquired new pearl, either re-equip One Pearl or use ability on the One Pearl item.  
(By the way Toybox Rest All is not an real rest, so after using that you'll need to do the above actions to match pearls).

When using One Pearl it will silently spend resources on the pearls on inventory. This will unstack your pearls one by one if they were stacked.   

If you have multiple One Pearls given to different characters, they will synchronize too.

This mod introduces save dependency. Because new item.