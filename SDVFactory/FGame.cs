using SDVFactory.Data;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
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
            };
            Helper.ConsoleCommands.Add("test", "Adds test machines", act);

            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;
        }

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
