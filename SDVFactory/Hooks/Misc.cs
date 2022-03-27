using Microsoft.Xna.Framework;
using StardewValley;

namespace SDVFactory.Hooks
{
    internal static class Misc
    {
        internal static void Patch()
        {
            foreach(var a in Data.Actions.ActionList)
                if (a.Value.touch) GameLocation.RegisterTouchAction(a.Key, TouchCallback);
                else GameLocation.RegisterTileAction(a.Key, TileCallback);
        }


        private static void TouchCallback(GameLocation location, string[] args, Farmer player, Vector2 tile)
        {
            if (args.Length > 0) Data.Actions.ActionList[args[0]].callback();
        }

        private static bool TileCallback(GameLocation location, string[] args, Farmer player, Point tile)
        {
            if (args.Length > 0) Data.Actions.ActionList[args[0]].callback();
            return true;
        }
    }
}
