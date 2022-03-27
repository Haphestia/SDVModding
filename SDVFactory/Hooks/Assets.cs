using StardewModdingAPI;
using StardewValley.GameData;
using System;
using System.Collections.Generic;

namespace SDVFactory.Hooks
{
    internal class Assets : IAssetEditor, IAssetLoader
    {
        private ModEntry Mod;
        private Dictionary<string, string> LoadableAssetMap;
        private List<Data.Minecart> Minecarts;
        private List<Data.Location> Locations;

        private List<string> EditAssets = new List<string>()
        {
            "Data\\Furniture",
            "Data\\AdditionalLocationsData",
            "Data\\MiscGameData"
        };

        public static void Patch(ModEntry mod) => new Assets(mod);

        public Assets(ModEntry mod)
        {
            Mod = mod;

            LoadableAssetMap = Data.LoadableAssets.LoadableAssetMap;
            Locations = Data.Locations.LocationList;
            foreach (var l in Locations) LoadableAssetMap.Add(l.AssetName, l.AssetPath);
            Minecarts = Data.Minecarts.MinecartList;

            Mod.Helper.Content.AssetLoaders.Add(this);
            Mod.Helper.Content.AssetEditors.Add(this);
        }

        public bool CanEdit<T>(IAssetInfo asset) {
            foreach(string s in EditAssets)
            {
                if (asset.Name.IsEquivalentTo(s)) return true;
            }
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.Name.IsEquivalentTo("Data\\AdditionalLocationsData"))
            {
                var locs = asset.AsDictionary<string, AdditionalLocationData>().Data;
                foreach(var l in Locations)
                {
                    FGame.Logger.Alert("Adding location: " + l.UniqueId);
                    locs.Add(l.UniqueId, new AdditionalLocationData() { ID = l.UniqueId, DisplayName = l.DisplayName, MapPath = l.AssetName, Type = l.Type });
                }
            }
            else if (asset.Name.IsEquivalentTo("Data\\MiscGameData"))
            {
                var carts = (asset.Data as MiscGameData).MineCartDestinations;
                foreach(var c in Minecarts)
                {
                    FGame.Logger.Alert("Adding minecart: " + c.UniqueId);
                    carts.Add(c.UniqueId, new MinecartDestinations() { DisplayName = c.DisplayName, Direction = c.Direction, Location = c.Location, Tile = c.LandingTile });
                }
            }
            else if (asset.Name.IsEquivalentTo("Data\\Furniture"))
            { //internal name|english name, furn category, image size, collision size, rotations, price, placement restrictions [0 indoors, 1 outdoors, 2 any], display name, sprite index, texture asset
                asset.AsDictionary<string, string>().Data.Add("Factory.FurnitureTest", "Factory.FurnitureTest/table/2 3/2 2/1/0/2/Factory Test/0/machines.test");
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            foreach (string s in LoadableAssetMap.Keys) if (asset.Name.IsEquivalentTo(s)) return true;
            return false;
        }
        public T Load<T>(IAssetInfo asset)
        {
            foreach (var s in LoadableAssetMap.Keys)
            {
                if (asset.Name.IsEquivalentTo(s))
                {
                    return Mod.Helper.Content.Load<T>(LoadableAssetMap[s], ContentSource.ModFolder);
                }
            }
            throw new InvalidOperationException($"Unloadable asset requested: '{asset.Name}'.");
        }
    }
}
