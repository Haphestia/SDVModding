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
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.getDescription)),
               postfix: new HarmonyMethod(typeof(Harmony), nameof(Furniture_getDescription_Post))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.placementAction)),
               postfix: new HarmonyMethod(typeof(Harmony), nameof(Furniture_placementAction_Post))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.performRemoveAction)),
               postfix: new HarmonyMethod(typeof(Harmony), nameof(Furniture_performRemoveAction_Post))
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

        public static void Furniture_placementAction_Post(Furniture __instance, StardewValley.GameLocation location, int x, int y, StardewValley.Farmer who = null)
        {
            if (!__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.IsFactoryMachine")) return;
            if (FGame.Verse == null || FGame.Verse.MachineStates == null)
            {
                FGame.Logger.Error("Tried to place machine without a Factoryverse... How did you even do this?");
            }
            if (!__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineShortId")) return;
            string machineShortId = __instance.modData["bwdy.FactoryMod.ModData.MachineShortId"];
            long mid = -1;
            bool hasMid = false;
            if (__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineNumber"))
            {
                if (long.TryParse(__instance.modData["bwdy.FactoryMod.ModData.MachineNumber"], out mid))
                {
                    hasMid = true;
                }
            }
            if (mid == -1) hasMid = false;
            if (!hasMid)
            {
                //time to create a new machine id)
                mid = FGame.Verse.CreateMachineState(machineShortId);
                __instance.modData["bwdy.FactoryMod.ModData.MachineNumber"] = mid.ToString();
                FGame.Logger.Info("New machine state: " + mid);
            }
            Data.Machine m = Data.Machines.MachineList[machineShortId];
            m.OnPlace(FGame.Verse.MachineStates[mid], location, who, __instance, x, y);
        }

        public static void Furniture_performRemoveAction_Post(Furniture __instance, Vector2 tileLocation, StardewValley.GameLocation environment)
        {
            if (!__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.IsFactoryMachine")) return;
            if (FGame.Verse == null || FGame.Verse.MachineStates == null)
            {
                FGame.Logger.Error("Tried to remove machine without a Factoryverse... How did you even do this?");
            }
            if (!__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineShortId")) return;
            string machineShortId = __instance.modData["bwdy.FactoryMod.ModData.MachineShortId"];
            long mid = -1;
            bool hasMid = false;
            if (__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineNumber"))
            {
                if (long.TryParse(__instance.modData["bwdy.FactoryMod.ModData.MachineNumber"], out mid))
                {
                    hasMid = true;
                }
            }
            if (mid == -1) hasMid = false;
            if (!hasMid) return;
            Data.Machine m = Data.Machines.MachineList[machineShortId];
            m.OnRemove(FGame.Verse.MachineStates[mid], environment, StardewValley.Game1.player, __instance, (int)tileLocation.X, (int)tileLocation.Y);
            if (!m.Persistent)
            {
                FGame.Verse.DestroyMachineState(mid);
                __instance.modData["bwdy.FactoryMod.ModData.MachineNumber"] = "-1";
            }
        }

        public static string Furniture_getDescription_Post(string __result, Furniture __instance)
        {
            if (__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.IsFactoryMachine"))
            {
                if (__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineShortId"))
                {
                    string machineShortId = __instance.modData["bwdy.FactoryMod.ModData.MachineShortId"];
                    return Data.Machines.MachineList[machineShortId].Description;
                }
            }
            return __result;
        }

        public static string Object_getCategoryName_Post(string __result, StardewValley.Object __instance)
        {
            if (__instance is Furniture)
            {
                var f = __instance as Furniture;
                if (f.modData.ContainsKey("bwdy.FactoryMod.ModData.IsFactoryMachine"))
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
                if (f.modData.ContainsKey("bwdy.FactoryMod.ModData.IsFactoryMachine"))
                {
                    return new Color(122,42,0);
                }
            }
            return __result;
        }

        public static bool Furniture_draw_Pre(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance.isTemporarilyInvisible) return false;
            if (!__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.IsFactoryMachine")) return true;
            if (FGame.Verse == null || FGame.Verse.MachineStates == null) return false;
            if (!__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineShortId")) return true;
            if (!__instance.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineNumber")) return true;
            if (!long.TryParse(__instance.modData["bwdy.FactoryMod.ModData.MachineNumber"], out long mid)) return true;
            string machineShortId = __instance.modData["bwdy.FactoryMod.ModData.MachineShortId"];
            Data.Machine m = Data.Machines.MachineList[machineShortId];
            Data.MachineState ms = null;
            if (mid >= 0 && FGame.Verse.MachineStates.ContainsKey(mid)) ms = FGame.Verse.MachineStates[mid];
            m.Draw(ms, __instance, spriteBatch, x, y, alpha);
            return false;
        }
    }
}
