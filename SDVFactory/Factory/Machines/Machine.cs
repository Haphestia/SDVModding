using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SDVFactory.Factory.UI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace SDVFactory.Factory.Machines
{
    public class Machine
    {
        public long Id { get; set; } = -1;

        private static Texture2D Texture;

        private static List<HandCraftingRecipe> CraftingRecipes = new List<HandCraftingRecipe>()
        {
            new HandCraftingRecipe(){
                DisplayName = "Factory Test",
                TileStartX = 0, TileStartY = 0,
                WidthInTiles = 2, HeightInTiles = 3,
                Texture = GetTexture(),
                Ingredients = new Dictionary<string, int>(){{ "388", 1 }, { "390", 5 } },
                CreateFunc = (_) => CreateOne(),
                CreateDummyFunc = (_) => CreateDummy()
            },
        };


        public static void Register()
        {

        }

        public Machine()
        {
        }

        public virtual void OnTick()
        {
            FGame.Logger.Info("machine ticking: " + Id);
        }

        public virtual void OnActivate(GameLocation l, Farmer who, Furniture f, Location vect)
        {
            FGame.Logger.Info("machine activate: " + Id);
        }

        public virtual void Draw(Furniture f, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            var sourceIndexOffset = FGame.Helper.Reflection.GetField<NetInt>(f, "sourceIndexOffset").GetValue().Value;
            var drawPosition = FGame.Helper.Reflection.GetField<NetVector2>(f, "drawPosition").GetValue().Value;
            Microsoft.Xna.Framework.Rectangle drawn_source_rect = f.sourceRect.Value;
            drawn_source_rect.X += drawn_source_rect.Width * sourceIndexOffset;
            if (Furniture.isDrawingLocationFurniture)
            {
                spriteBatch.Draw(GetTexture(), Game1.GlobalToLocal(Game1.viewport, drawPosition + ((f.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, f.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((float)(f.boundingBox.Value.Bottom - ((f.furniture_type.Value == 6 || f.furniture_type.Value == 17 || f.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
            }
            else
            {
                spriteBatch.Draw(GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((f.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (f.sourceRect.Height * 4 - f.boundingBox.Height) + ((f.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), f.sourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, f.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((float)(f.boundingBox.Value.Bottom - (((int)f.furniture_type.Value == 6 || (int)f.furniture_type.Value == 17 || (int)f.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
            }
        }

        public static Texture2D GetTexture()
        {
            if(Texture == null) Texture = Data.TextureCache.Get("bwdy.FactoryMod.Textures.Machines");
            return Texture;
        }

        public static Furniture CreateDummy()
        {
            var f = new Furniture("Factory.FurnitureTest", Microsoft.Xna.Framework.Vector2.Zero);
            f.modData.Add("FactoryMod", "true");
            return f;
        }

        public static Furniture CreateOne()
        {
            long id = ++FGame.World.NextMachineId;
            FGame.Logger.Info("machine create: " + id);
            var m = new Machine();
            m.Id = id;
            FGame.World.Machines.Add(id, m);
            var f = new Furniture("Factory.FurnitureTest", Microsoft.Xna.Framework.Vector2.Zero);
            f.modData.Add("FactoryMod", "true");
            f.modData.Add("FactoryId", id.ToString());
            return f;
        }
    }
}
