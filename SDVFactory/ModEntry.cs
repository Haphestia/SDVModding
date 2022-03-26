using HarmonyLib;
using StardewModdingAPI;
using System;

namespace SDVFactory
{
    public class ModEntry : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            var am = new AssetManager(this);
            helper.Content.AssetLoaders.Add(am);
            helper.Content.AssetEditors.Add(am);
            new FactoryModHooks(this);
            var harmony = new Harmony(ModManifest.UniqueID);
            Factory.FactoryGame.Initialize(harmony, helper, Monitor);
        }
    }
}
