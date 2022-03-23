# JsonMinecarts
 Need custom minecarts? Look no further!
 
 * What does it do?  
 It replaces the minecart destination menu with a nearly identical clone, which
 operates based on simple JSON data. You can use content patcher to add new minecarts,
 or edit the vanilla minecarts.
 
 * How does it work?  
 It detects minecarts by checking the tile ids used. It then compares this to its list
 of defined minecarts, and if one exists, it treats it as that one, presenting the menu
 and excluding the currently selected cart from that list.  
 
 * Adding new minecarts  
 You can add new minecarts just by placing the tiles- the tile ID checks will detect the
 new minecart. To make the mod service it, you'll need to add a content entry for it.
 Multiple minecarts on the same map should be at least 15 tiles apart. The landing point
 of a minecart must be within 5 tiles of the minecart itself to be detected properly. If
 your minecart does not provide a content entry, it will fallback to vanilla handling, 
 meaning you can still create your own private networks of minecarts using other mods or
 techniques, and this mod should not interfere with them.
 
 * Editing vanilla minecarts
 The vanilla minecarts have been reimplemented using content entries. The following keys
 pertain to the vanilla carts: "jsonminecarts.busstop", "jsonminecarts.town", 
 "jsonminecarts.mines", and "jsonminecarts.quarry". They make use of an optional parameter,
 VanillaPassthrough, which causes the minecart to pass this string as a vanilla dialogue 
 response key, triggering the game's native handling, e.g. "Minecart_Bus". If the 
 VanillaPassthrough parameter is present, the LandingPoint and LocationName are ignored. 
 You can edit those reimplementations and remove this parameter to relocate or otherwise 
 tamper with the parameters of those vanilla minecarts.
   
 The syntax to add a minecart with Content Patcher is as follows:  
   
 ```
 {
    "Format": "1.25.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "JsonMinecarts.Minecarts",
            "Entries": {
                "morecarts.islandnorth": {
                    "LocationName": "IslandNorth",
                    "DisplayName": "Island North",
                    "LandingPointX": 19,
                    "LandingPointY": 13,
                    "LandingPointDirection": 2,
                    "IsUnderground": false
                }
            }
        }
    ]
 }
 ```