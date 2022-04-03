using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDVFactory.Data;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Linq;
using xTile.Dimensions;

namespace SDVFactory
{
    internal static class FGame
    {
        internal static Logger Logger;
        internal static ModEntry Mod;
        internal static IModHelper Helper { get => Mod.Helper; }
        
        public static Factoryverse Verse;
        internal static LoreTime LastMachineTickLoreTime = LoreTime.Zero;
        internal static TimeSpan LastMachineTick;
        internal static TimeSpan TickTimer = new TimeSpan(0, 0, 0, 0, 250);

        public static void Initialize(ModEntry mod)
        {
            Mod = mod;
            Logger = new Logger(mod.Monitor);

            Action<string,string[]> act = (a, b) =>
            {
                Game1.player.addItemByMenuIfNecessary(Data.Machines.MachineList["Generator1"].CreateOne());
                Game1.player.addItemByMenuIfNecessary(Data.Machines.MachineList["Crusher1"].CreateOne());
                Game1.player.addItemByMenuIfNecessary(Data.Machines.MachineList["Connector1"].CreateOne());
                Game1.player.addItemByMenuIfNecessary(Utility.CreateItemByID("bwdy.FactoryMod.Tools.WireTool", 1));
            };
            Helper.ConsoleCommands.Add("test", "Adds test machines", act);

            Action<string, string[]> act3 = (a, b) =>
            {
                if (Verse == null) return;
                if (Verse.MachineStates == null) return;
                foreach(var kvp in Verse.MachineStates)
                {
                    Logger.Info(kvp.Key + ": " + kvp.Value.Machine.DisplayName);
                }
            };
            Helper.ConsoleCommands.Add("listmachines", "Lists factory machines", act3);

            Action<string, string[]> act2 = (a, b) =>
            {
                if (Verse == null) return;
                if (Verse.MachineStates == null) return;
                if (b.Length != 3)
                {
                    Logger.Info("This command requires exactly 3 parameters.");
                    return;
                }
                if (b[0] != "add" && b[0] != "remove")
                {
                    Logger.Info("First parameter must be 'add' or 'remove'.");
                    return;
                }
                bool add = b[0] == "add";
                long mid1 = 0;
                long mid2 = 0;
                if(!long.TryParse(b[1], out mid1))
                {
                    Logger.Info("Second parameter must be an integral machine state id.");
                    return;
                }
                if (!long.TryParse(b[2], out mid2))
                {
                    Logger.Info("Third parameter must be an integral machine state id.");
                    return;
                }
                if (!Verse.MachineStates.ContainsKey(mid1))
                {
                    Logger.Info("Cannot find machine state with id: " + mid1);
                    return;
                }
                if (!Verse.MachineStates.ContainsKey(mid2))
                {
                    Logger.Info("Cannot find machine state with id: " + mid2);
                    return;
                }

                if (add)
                {
                    Verse.Wires.Add(new Data.Components.Wire(mid2, mid1));
                    Logger.Info("Added wire from " + mid1 + " (" + Verse.MachineStates[mid1].Machine.DisplayName + ") to " + mid2 + " (" + Verse.MachineStates[mid2].Machine.DisplayName + ")");
                } else
                {
                    var match = Verse.Wires.Where(ww => ww.InputMachineNumber == mid2 && ww.OutputMachineNumber == mid1);
                    if (match.Count() > 0)
                    {
                        for(int i = Verse.Wires.Count; i > 0; i--)
                        {
                            if(Verse.Wires[i].InputMachineNumber == mid2 && Verse.Wires[i].OutputMachineNumber == mid1)
                            {
                                Verse.Wires.RemoveAt(i);
                            }
                        }
                        Logger.Info("Removed wire from " + mid1 + " (" + Verse.MachineStates[mid1].Machine.DisplayName + ") to " + mid2 + " (" + Verse.MachineStates[mid2].Machine.DisplayName + ")");
                    } else
                    {
                        Logger.Info("Requested wire does not exist, and could therefore not be removed.");
                        return;
                    }
                }
            };
            Helper.ConsoleCommands.Add("wire", "Tampers with wires. Usage: wire add 2 3 - would add a wire from machinestate 2 to machinestate 3", act2);

            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
        }

        private static void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            //render our wires
            if (Verse == null || Verse.MachineStates == null) return;
            var spriteBatch = e.SpriteBatch;
            foreach(var state in Verse.MachineStates.Values)
            {
                if (state.Machine == null) continue;
                foreach(var src_mloc in state.Locations)
                {
                    foreach (var w in Verse.Wires.Where(ww => ww.IsOutputMachine(state)))
                    {
                        var other_m = w.GetOtherMachine(state);
                        if (other_m == null || other_m.Machine == null) continue;
                        Vector2 here = Game1.GlobalToLocal(new Vector2(src_mloc.X, src_mloc.Y) + (state.Machine.PowerConnectionPointOut * 4f));
                        foreach (var mloc in other_m.Locations)
                        {
                            if (mloc.LocationId == Game1.player.currentLocation.uniqueName.Value)
                            {
                                Vector2 there = Game1.GlobalToLocal(new Vector2(mloc.X, mloc.Y) + (other_m.Machine.PowerConnectionPointIn * 4f));
                                DrawWire(spriteBatch, here, there);
                            }
                        }
                    }
                }
            }
        }

        #region WireDrawing
        private static Texture2D WireTexture;
        private static Color WireColor = new Color(32, 32, 32);
        private static Texture2D GetWireTexture(SpriteBatch spriteBatch)
        {
            if (WireTexture == null)
            {
                WireTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                WireTexture.SetData(new[] { Color.White });
            }

            return WireTexture;
        }
        private static void DrawWire(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, WireColor, 4f);
        }
        private static void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetWireTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 100f);
        }
        #endregion

        private static void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            if (Verse == null) return;
            Helper.Data.WriteSaveData("bwdy.FactoryMod.SaveData.Factoryverse", System.Text.Json.JsonSerializer.Serialize(Verse, typeof(Factoryverse), new System.Text.Json.JsonSerializerOptions() { IncludeFields = true }));
        }

        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LastMachineTick = Game1.currentGameTime.TotalGameTime;
            LastMachineTickLoreTime = LoreTime.Now;
            var w = Helper.Data.ReadSaveData<string>("bwdy.FactoryMod.SaveData.Factoryverse");
            if (w == null)
            {
                Verse = new Factoryverse();
                Logger.Info("Created new Factoryverse.");
            }
            else
            {
                Verse = System.Text.Json.JsonSerializer.Deserialize<Factoryverse>(w, new System.Text.Json.JsonSerializerOptions() { IncludeFields = true });
                Logger.Info("Loaded saved Factoryverse.");
            }
        }

        private static void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (Verse == null) return;
            if (Game1.currentGameTime == null) return;
            if (!Game1.IsMasterGame) return; //this needs to be handled differently
            if(Game1.currentGameTime.TotalGameTime - LastMachineTick > TickTimer)
            {
                LastMachineTick = Game1.currentGameTime.TotalGameTime;
                uint deltaMinutes = LoreTime.Now - LastMachineTickLoreTime;
                if (deltaMinutes < 1) return;
                if (!Context.IsWorldReady) return;
                LastMachineTickLoreTime = LoreTime.Now;
                Verse.Tick(deltaMinutes);
            }
        }

        //return true to suppress vanilla checks
        public static bool CheckAction(GameLocation l, Farmer who, xTile.Dimensions.Location vect)
        {
            if (Verse == null) return false;
            foreach (Furniture f in l.furniture)
            {
                if (f.boundingBox.Value.Contains((int)(vect.X * 64f), (int)(vect.Y * 64f)))
                {
                    if (f.modData.ContainsKey("bwdy.FactoryMod.ModData.IsFactoryMachine") && f.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineShortId"))
                    {
                        string machineShortId = f.modData["bwdy.FactoryMod.ModData.MachineShortId"];
                        Data.Machine machine = Data.Machines.MachineList[machineShortId];
                        long mid = -1;
                        bool hasMid = false;
                        if (f.modData.ContainsKey("bwdy.FactoryMod.ModData.MachineNumber"))
                        {
                            if (long.TryParse(f.modData["bwdy.FactoryMod.ModData.MachineNumber"], out mid))
                            {
                                hasMid = true;
                            }
                        }
                        if (mid == -1) hasMid = false;
                        if (hasMid) machine.OnActivate(Verse.MachineStates[mid], l, who, f, vect);
                        else machine.OnActivate(null, l, who, f, vect);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
