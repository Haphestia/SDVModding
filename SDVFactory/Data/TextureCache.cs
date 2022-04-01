using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SDVFactory.Data
{
    internal static class TextureCache
    {
        internal static Dictionary<string, string> AssetMap = new Dictionary<string, string>()
        {
            {"factorytiles", "Assets\\factorytiles.png"},
            {"bwdy.FactoryMod.Textures.IO", "Assets\\io.png"},
            {"bwdy.FactoryMod.Textures.PowerMeter", "Assets\\power.png"},
            {"bwdy.FactoryMod.Textures.FluidMeter", "Assets\\fluid.png"},
            {"bwdy.FactoryMod.Textures.Machines", "Assets\\machines.png"}
        };
        private static Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>(); 
        internal static Texture2D Get(string assetName)
        {
            if(!Cache.ContainsKey(assetName) || Cache[assetName].IsDisposed)
                Cache[assetName] = FGame.Helper.Content.Load<Texture2D>(assetName, StardewModdingAPI.ContentSource.GameContent);
            return Cache[assetName];
        }
    }
}
