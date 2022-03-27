using System;
using System.Collections.Generic;

namespace SDVFactory.Data
{
    internal static class Actions
    {
        internal static Dictionary<string, (bool touch, Action callback)> ActionList = new Dictionary<string, (bool touch, Action callback)>()
        {
            { "bwdy.FactoryMod.TileActions.EngineeringTable", (false, () => { Menus.MenuEngineeringTable.Show(); }) }
        };
    }
}
