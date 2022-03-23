# JsonMinecarts
 Need custom minecarts? Look no further!
 
 * What does it do? *  
 It replaces the minecart destination menu with a nearly identical clone, which
 operates based on simple JSON data. You can use content patcher to add new minecarts,
 or edit the vanilla minecarts.
 
 * How does it work? *  
 It detects minecarts by checking the tile ids used. It then compares this to its list
 of defined minecarts, and if one exists, it treats it as that one, presenting the menu
 and excluding the currently selected cart from that list.  
   
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