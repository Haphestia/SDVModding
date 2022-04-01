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
            "Data\\MiscGameData",
            "Data\\AudioCueModificationData"
        };

        public static void Patch(ModEntry mod) => new Assets(mod);

        public Assets(ModEntry mod)
        {
            Mod = mod;

            LoadableAssetMap = Data.TextureCache.AssetMap;
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
                    locs.Add(l.UniqueId, new AdditionalLocationData() { ID = l.UniqueId, DisplayName = l.DisplayName, MapPath = l.AssetName, Type = l.Type });
                }
            }
            else if (asset.Name.IsEquivalentTo("Data\\MiscGameData"))
            {
                var carts = (asset.Data as MiscGameData).MineCartDestinations;
                foreach(var c in Minecarts)
                {
                    carts.Add(c.UniqueId, new MinecartDestinations() { DisplayName = c.DisplayName, Direction = c.Direction, Location = c.Location, Tile = c.LandingTile });
                }
            }
            else if (asset.Name.IsEquivalentTo("Data\\AudioCueModificationData")) {
                asset.AsDictionary<string, AudioCueData>().Data.Add("bwdy.FactoryMod.Music.Potential", new AudioCueData() { ID = "bwdy.FactoryMod.Music.Potential", Looped = true, StreamedVorbis = true, UseReverb = false, Category = "Music", FilePaths = new List<string>() { FGame.Mod.Helper.DirectoryPath + "/Assets/Potential.ogg" } });
            }
            else if (asset.Name.IsEquivalentTo("Data\\Furniture"))
            {   //internal name|english name, furn category, image size, collision size, rotations, price, placement restrictions [0 indoors, 1 outdoors, 2 any], display name, sprite index, texture asset
                var data = asset.AsDictionary<string, string>().Data;
                foreach(var m in Data.Machines.MachineList.Values)
                {
                    string md_id = "bwdy.FactoryMod.Furniture." + m.ShortId;
                    string md_category = "table";
                    string md_drawSize = m.DrawSize.X + " " + m.DrawSize.Y; //e.g. "3 2" starting top-left
                    string md_collisionSize = m.CollisionSize.X + " " + m.CollisionSize.Y; //e.g. "2 2" starting bottom left
                    string md_rotations = "1";
                    string md_price = m.Price.ToString();
                    string md_placementRestrictions = "2"; //[0 indoors, 1 outdoors, 2 any]
                    string md_displayName = m.DisplayName;
                    string md_spriteIndex = "0";
                    string md_textureAssetName = m.TextureName;
                    data.Add(md_id, md_id + "/" + md_category + "/" + md_drawSize + "/" + md_collisionSize + "/" + md_rotations + "/" + md_price + "/" + md_placementRestrictions + "/" + md_displayName + "/" + md_spriteIndex + "/" + md_textureAssetName);
                }
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