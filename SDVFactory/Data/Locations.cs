using System.Collections.Generic;

namespace SDVFactory.Data
{
    internal static class Locations
    {
        internal static List<Location> LocationList => new List<Location>() {
            new Location(){ UniqueId = "bwdy.FactoryMod.Locations.Factory", DisplayName = "Factory", AssetName = "bwdy.FactoryMod.Assets.Maps.Factory", AssetPath = "Assets\\Factory.tmx", Type = "StardewValley.Locations.DecoratableLocation" }
        };
    }

    internal class Location
    {
        internal string UniqueId;
        internal string DisplayName;
        internal string AssetName;
        internal string AssetPath;
        internal string Type;
    }
}