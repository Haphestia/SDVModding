using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using System;

namespace SDVFactory.Hooks
{
    internal static class Harmony
    {
        internal static void Patch()
        {
            var harmony = new HarmonyLib.Harmony("bwdy.FactoryMod");
            harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(Harmony), nameof(Furniture_draw_Pre))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryColor)),
               postfix: new HarmonyMethod(typeof(Harmony), nameof(Object_getCategoryColor_Post))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryName)),
               postfix: new HarmonyMethod(typeof(Harmony), nameof(Object_getCategoryName_Post))
            );

        }

        public static string Object_getCategoryName_Post(string __result, StardewValley.Object __instance)
        {
            if (__instance is Furniture)
            {
                var f = __instance as Furniture;
                if (f.modData.ContainsKey("FactoryMod"))
                {
                    return "Machine";
                }
            }
            return __result;
        }

        public static Color Object_getCategoryColor_Post(Color __result, StardewValley.Object __instance)
        {
            if (__instance is Furniture)
            {
                var f = __instance as Furniture;
                if (f.modData.ContainsKey("FactoryMod"))
                {
                    return new Color(122,42,0);
                }
            }
            return __result;
        }

        public static bool Furniture_draw_Pre(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            //todo, readd custom texture here
            //todo, make a machine class to simplify this stuff
            Furniture f = __instance as Furniture;
            if (!f.modData.ContainsKey("FactoryMod")) return true;
            if (FGame.World == null || FGame.World.Machines == null) return false;
            if (!f.modData.ContainsKey("FactoryId")) return true;
            if (!long.TryParse(f.modData["FactoryId"], out long mid)) return true;
            if (!FGame.World.Machines.ContainsKey(mid)) return true;
            var m = FGame.World.Machines[mid];

            if (f.isTemporarilyInvisible)
            {
                return false;
            }

            m.Draw(__instance, spriteBatch, x, y, alpha);

            return false;
        }
    }
}
