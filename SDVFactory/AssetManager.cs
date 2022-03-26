using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using System;
using System.Collections.Generic;

namespace SDVFactory
{
    internal class AssetManager : IAssetEditor, IAssetLoader
    {
        private ModEntry Mod;

        private List<string> LoadAssets = new List<string>()
        {
            "FactoryMod.MapFactory",
            "factorytiles",
            "machines.test"
        };

        private List<string> EditAssets = new List<string>()
        {
            "Data\\Furniture",
            "Data\\AdditionalLocationsData",
            "Data\\MiscGameData"
        };


        public AssetManager(ModEntry mod)
        {
            Mod = mod;
        }

        public bool CanEdit<T>(IAssetInfo asset) {
            foreach(string s in EditAssets)
            {
                if (asset.Name.IsEquivalentTo(s)) return true;
            }
            return false;
        }
        public bool CanLoad<T>(IAssetInfo asset)
        {
            foreach (string s in LoadAssets)
            {
                if (asset.Name.IsEquivalentTo(s)) return true;
            }
            return false;
        }


        public void Edit<T>(IAssetData asset)
        {
            if (asset.Name.IsEquivalentTo("Data\\AdditionalLocationsData"))
            {
                asset.AsDictionary<string, AdditionalLocationData>().Data.Add("Factory", new AdditionalLocationData() { ID = "Factory", DisplayName = "Factory", MapPath = "FactoryMod.MapFactory", Type = "StardewValley.Locations.DecoratableLocation" });
            }
            else if (asset.Name.IsEquivalentTo("Data\\MiscGameData"))
            {
                (asset.Data as MiscGameData).MineCartDestinations.Add("FactoryMod.MinecartFactory", new MinecartDestinations() { DisplayName = "Factory", Direction = "down", Location = "Factory", Tile = new Microsoft.Xna.Framework.Point(95, 8) });
            }
            else if (asset.Name.IsEquivalentTo("Data\\Furniture"))
            { //internal name|english name, furn category, image size, collision size, rotations, price, placement restrictions [0 indoors, 1 outdoors, 2 any], display name, sprite index, texture asset
                asset.AsDictionary<string, string>().Data.Add("Factory.FurnitureTest", "Factory.FurnitureTest/table/2 3/2 2/1/0/2/Factory Test/0/machines.test");
            }
        }
        public T Load<T>(IAssetInfo asset)
        {
            if (asset.Name.IsEquivalentTo("FactoryMod.MapFactory"))
            {
                return Mod.Helper.Content.Load<T>("Assets/Factory.tmx", ContentSource.ModFolder);
            }
            else if (asset.Name.IsEquivalentTo("FactoryMod.MapFactory"))
            {
                return Mod.Helper.Content.Load<T>("Assets/factorytiles.png", ContentSource.ModFolder);
            }
            else if (asset.Name.IsEquivalentTo("machines.test"))
            {
                return Mod.Helper.Content.Load<T>("Assets/testmachine.png", ContentSource.ModFolder);
            }
            throw new InvalidOperationException($"Unexpected asset '{asset.Name}'.");
        }
    }
}
