using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SDVFactory.Data
{
    internal static class Machines
    {
        internal static Dictionary<string, Machine> MachineList = new Dictionary<string, Machine>()
        {
            { "Generator1", new Machine() {
                ShortId = "Generator1",
                DisplayName = "Primitive Generator",
                Description = "Generates power from solid fuels.",
                DrawSize = new Point(2, 3),
                CollisionSize = new Point(2, 2),
                TextureName = "bwdy.FactoryMod.Textures.Machines",
                TextureTopLeftTile = new Point(0,0),
                MachineType = MachineType.GENERATOR,
                Price = 0,
                Persistent = false,
                ItemInputs = ItemXput.ONE_ITEM,
                PowerOutputs = PowerXput.POWER
            }}
        };
    }

    internal enum ItemXput
    {
        NO_ITEMS,
        ONE_ITEM,
        TWO_ITEMS,
        THREE_ITEMS
    }

    public enum FluidXput
    {
        NO_FLUIDS,
        ONE_FLUID,
        TWO_FLUIDS
    }

    public enum PowerXput
    {
        NO_POWER,
        POWER
    }

    internal class Machine
    {
        public string UniqueId { get => "bwdy.FactoryMod.Machines." + ShortId; }
        public string ShortId;
        public string DisplayName;
        public string Description;
        public Point DrawSize;
        public Point CollisionSize;
        public string TextureName;
        public Point TextureTopLeftTile;
        public MachineType MachineType;
        public int Price;
        public bool Persistent = false;
        public ItemXput ItemInputs = ItemXput.NO_ITEMS;
        public FluidXput FluidInputs = FluidXput.NO_FLUIDS;
        public PowerXput PowerInputs = PowerXput.NO_POWER;
        public ItemXput ItemOutputs = ItemXput.NO_ITEMS;
        public FluidXput FluidOutputs = FluidXput.NO_FLUIDS;
        public PowerXput PowerOutputs = PowerXput.NO_POWER;

        public Furniture CreateOne()
        {
            var f = new Furniture("bwdy.FactoryMod.Furniture." + ShortId, Vector2.Zero);
            f.modData.Add("bwdy.FactoryMod.ModData.IsFactoryMachine", "true");
            f.modData.Add("bwdy.FactoryMod.ModData.MachineShortId", ShortId);
            f.modData.Add("bwdy.FactoryMod.ModData.MachineNumber", "-1");
            return f;
        }

        public virtual void OnPlace(MachineState state, GameLocation l, Farmer who, Furniture f, int x, int y)
        {
            if (state.MachineNumber == -1) return;
            // FGame.Logger.Info("machine placed: " + state.MachineNumber);
            switch (MachineType)
            {
                case MachineType.GENERATOR:
                    if (state.Inventory == null) state.Inventory = new Dictionary<int, Item>();
                    break;
            }
        }

        public virtual void OnRemove(MachineState state, GameLocation l, Farmer who, Furniture f, int x, int y)
        {
            if (state.MachineNumber == -1) return;
            // FGame.Logger.Info("machine removed: " + state.MachineNumber);
            // machine state will be destroyed following this unless Persistent is true
        }

        public virtual void OnTick(MachineState state)
        {
            if (state.MachineNumber == -1) return;
            // FGame.Logger.Info("machine ticking: " + state.MachineNumber);
        }

        public virtual void OnActivate(MachineState state, GameLocation l, Farmer who, Furniture f, xTile.Dimensions.Location vect)
        {
            if (state == null || state.MachineNumber == -1) return;
            // FGame.Logger.Info("machine activate: " + state.MachineNumber);
            switch (MachineType)
            {
                case MachineType.GENERATOR:
                    Menus.MachineMenu.Show(this, state);
                    break;
            }
        }

        public virtual void Draw(MachineState state, Furniture f, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            var sourceIndexOffset = FGame.Helper.Reflection.GetField<NetInt>(f, "sourceIndexOffset").GetValue().Value;
            var drawPosition = FGame.Helper.Reflection.GetField<NetVector2>(f, "drawPosition").GetValue().Value;
            Rectangle drawn_source_rect = f.sourceRect.Value;
            drawn_source_rect.X += drawn_source_rect.Width * sourceIndexOffset;
            Texture2D tex = TextureCache.Get(TextureName);
            if (Furniture.isDrawingLocationFurniture)
            {
                spriteBatch.Draw(tex, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((f.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, f.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((float)(f.boundingBox.Value.Bottom - ((f.furniture_type.Value == 6 || f.furniture_type.Value == 17 || f.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
            }
            else
            {
                spriteBatch.Draw(tex, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((f.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (f.sourceRect.Height * 4 - f.boundingBox.Height) + ((f.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), f.sourceRect.Value, Color.White * alpha, 0f, Vector2.Zero, 4f, f.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((float)(f.boundingBox.Value.Bottom - (((int)f.furniture_type.Value == 6 || (int)f.furniture_type.Value == 17 || (int)f.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
            }
        }
    }

    public enum MachineType
    {
        NONE = 0,
        GENERATOR = 1
    }

    public class MachineState
    {
        [JsonIgnore]
        [XmlIgnore]
        internal Machine Machine;

        public string MachineShortId;
        public long MachineNumber = -1;
        public Dictionary<int, Item> Inventory;


        public MachineState() { }
        public MachineState(string shortId, long number)
        {
            MachineShortId = shortId;
            MachineNumber = number;
        }

        internal Machine GetMachine()
        {
            if (Machine != null) return Machine;
            Machine = Machines.MachineList.GetValueOrDefault(MachineShortId);
            return Machine;
        }

        public virtual void OnTick()
        {
            GetMachine().OnTick(this);
        }

        public virtual void OnActivate(GameLocation l, Farmer who, Furniture f, xTile.Dimensions.Location vect)
        {
            GetMachine().OnActivate(this, l, who, f, vect);
        }
    }
}
