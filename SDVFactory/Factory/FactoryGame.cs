using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace SDVFactory.Factory
{
    internal static class FactoryGame
    {
        internal static Logger Logger;
        internal static int NextMachineId = 0;
        internal static IModHelper Helper;

        public static FactoryWorld World;

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            Helper = helper;
            Logger = new Logger(monitor);

            Action<string,string[]> act = (a, b) =>
            {
                Game1.player.addItemByMenuIfNecessary(Machines.Machine.CreateOne());
            };
            helper.ConsoleCommands.Add("test", "Adds test machines", act);

            //todo: load me
            World = new FactoryWorld();
        }

        //return true to suppress vanilla checks
        public static bool CheckAction(GameLocation l, Farmer who, Location vect)
        {
            foreach (Furniture f in l.furniture)
            {
                if (f.boundingBox.Value.Contains((int)(vect.X * 64f), (int)(vect.Y * 64f)) && f.furniture_type.Value != 12)
                {
                    if (f.modData.ContainsKey("FactoryMod") && f.modData.ContainsKey("FactoryId"))
                    {
                        string s = f.modData["FactoryId"];
                        if (!string.IsNullOrEmpty(s))
                        {
                            if(long.TryParse(s, out long mid))
                            {
                                World.ActivateMachine(l, who, f, vect, mid);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
