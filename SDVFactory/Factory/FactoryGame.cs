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
        internal static Harmony Harmony;
        internal static IModHelper Helper;

        public static void Initialize(Harmony harmony, IModHelper helper, IMonitor monitor)
        {
            Helper = helper;
            Logger = new Logger(monitor);
            Logger.Info("Hello from Factory! Mod initialized okay.");

            harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)}),
               prefix: new HarmonyMethod(typeof(FurniturePatch), nameof(FurniturePatch.DrawPrefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryColor)),
               postfix: new HarmonyMethod(typeof(FurniturePatch), nameof(FurniturePatch.CategoryColorPostfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryName)),
               postfix: new HarmonyMethod(typeof(FurniturePatch), nameof(FurniturePatch.CategoryNamePostfix))
            );

            Action<string,string[]> act = (a, b) =>
            {
                //give the player a free test machine
                var f = new Furniture("Factory.FurnitureTest", Microsoft.Xna.Framework.Vector2.Zero);
                NextMachineId++;
                f.modData.Add("FactoryMod", "true");
                f.modData.Add("FactoryId", NextMachineId.ToString());
                f.ParentSheetIndex = 2;
                Game1.player.addItemByMenuIfNecessary(f);
            };
            helper.ConsoleCommands.Add("test", "Adds test machines", act);
        }

        //return true to consume action check
        public static bool CheckFurnitureAction(GameLocation l, Farmer who, Furniture f, Location vect)
        {
            if (f.modData.ContainsKey("FactoryMod"))
            {
                Logger.Alert("furniture happened: " + f.Name + " :: " + f.modData["FactoryId"]);
            }
            return false;
        }

        //return true to suppress vanilla checks
        public static bool CheckAction(GameLocation l, Farmer who, Location vect)
        {
            foreach (Furniture f in l.furniture)
            {
                if (f.boundingBox.Value.Contains((int)(vect.X * 64f), (int)(vect.Y * 64f)) && f.furniture_type.Value != 12)
                {
                    if (CheckFurnitureAction(l, who, f, vect)) return true;
                }
            }
            return false;
        }
    }
}
