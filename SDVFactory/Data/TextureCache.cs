using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SDVFactory.Data
{
    internal static class TextureCache
    {
        private static Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>(); 
        internal static Texture2D Get(string assetName)
        {
            if(!Cache.ContainsKey(assetName) || Cache[assetName].IsDisposed)
                Cache[assetName] = FGame.Helper.Content.Load<Texture2D>(assetName, StardewModdingAPI.ContentSource.GameContent);
            return Cache[assetName];
        }
    }
}
