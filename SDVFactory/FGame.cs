using SDVFactory.Factory;
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
        
        public static Factoryverse World;
        internal static LoreTime LastMachineTickLoreTime = LoreTime.Zero;
        internal static TimeSpan LastMachineTick;
        internal static TimeSpan TickTimer = new TimeSpan(0, 0, 0, 0, 250);

        public static void Initialize(ModEntry mod)
        {
            Mod = mod;
            Logger = new Logger(mod.Monitor);

            Action<string,string[]> act = (a, b) =>
            {
                Game1.player.addItemByMenuIfNecessary(Factory.Machines.Machine.CreateOne());
            };
            Helper.ConsoleCommands.Add("test", "Adds test machines", act);

            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;

            RegisterMachines();
        }

        private static void RegisterMachines()
        {
            Factory.Machines.Machine.Register();
        }



        private static void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            if (World == null) return;
            Helper.Data.WriteSaveData("bwdy.FactoryMod.World", World);
        }

        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LastMachineTick = Game1.currentGameTime.TotalGameTime;
            LastMachineTickLoreTime = LoreTime.Now;
            var w = Helper.Data.ReadSaveData<Factoryverse>("bwdy.FactoryMod.World");
            if (w == null)
            {
                World = new Factoryverse();
                Logger.Info("Created new Factory data.");
            }
            else
            {
                World = w;
                Logger.Info("Loaded Factory data.");
            }
        }

        private static void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (World == null) return;
            if (Game1.currentGameTime == null) return;
            if (!Game1.IsMasterGame) return; //this needs to be handled differently
            if(Game1.currentGameTime.TotalGameTime - LastMachineTick > TickTimer)
            {
                LastMachineTick = Game1.currentGameTime.TotalGameTime;
                uint deltaMinutes = LoreTime.Now - LastMachineTickLoreTime;
                if (deltaMinutes < 1) return;
                if (!Context.IsWorldReady) return;
                LastMachineTickLoreTime = LoreTime.Now;
                World.Tick(deltaMinutes);
            }
        }

        //return true to suppress vanilla checks
        public static bool CheckAction(GameLocation l, Farmer who, Location vect)
        {
            if (World == null) return false;
            foreach (Furniture f in l.furniture)
            {
                if (f.boundingBox.Value.Contains((int)(vect.X * 64f), (int)(vect.Y * 64f)))
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
