using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SDVFactory.Data
{
    internal static class Minecarts
    {
        internal static List<Minecart> MinecartList => new List<Minecart>() {
            new Minecart(){ UniqueId = "bwdy.FactoryMod.Minecarts.FactoryMain", DisplayName = "Factory", Direction = "down", Location = "bwdy.FactoryMod.Locations.Factory", LandingTile = new Point(95, 8) }
        };
    }

    internal class Minecart
    {
        internal string UniqueId;
        internal string DisplayName;
        internal string Direction;
        internal string Location;
        internal Point LandingTile;
    }
}