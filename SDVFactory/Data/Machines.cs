using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
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
                Price = 0,
                Persistent = false,
                ItemInputs = ItemXput.ONE_ITEM,
                PowerOutputBufferSize = 99,
                Recipes = new List<MachineRecipe>()
                {
                    new MachineRecipe(){ItemInput1 = ("382", 1), PowerOutputPerMinute = 3, ProcessingTimeInMinutes = 15 }
                }
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
        public int Price;
        public bool Persistent = false;
        public ItemXput ItemInputs = ItemXput.NO_ITEMS;
        public FluidXput FluidInputs = FluidXput.NO_FLUIDS;
        public int PowerInputBufferSize = 0;
        public ItemXput ItemOutputs = ItemXput.NO_ITEMS;
        public FluidXput FluidOutputs = FluidXput.NO_FLUIDS;
        public int PowerOutputBufferSize = 0;
        public List<MachineRecipe> Recipes = new List<MachineRecipe>();

        public bool HasItemInputs => ItemInputs != ItemXput.NO_ITEMS;
        public bool HasFluidInputs => FluidInputs != FluidXput.NO_FLUIDS;
        public bool HasPowerInputs => PowerInputBufferSize > 0;
        public bool HasInputs => (HasItemInputs || HasFluidInputs || HasPowerInputs);

        public bool HasItemOutputs => ItemOutputs != ItemXput.NO_ITEMS;
        public bool HasFluidOutputs => FluidOutputs != FluidXput.NO_FLUIDS;
        public bool HasPowerOutputs => PowerOutputBufferSize > 0;
        public bool HasOutputs => (HasItemOutputs || HasFluidOutputs || HasPowerOutputs);

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
            state.IsPlaced = true;
            state.Inventory = new Item[6];
        }

        public virtual void OnRemove(MachineState state, GameLocation l, Farmer who, Furniture f, int x, int y)
        {
            if (state.MachineNumber == -1) return;
            state.IsPlaced = false;
            // machine state will be destroyed following this unless Persistent is true
        }

        public virtual void OnTick(MachineState state)
        {
            if (state.MachineNumber == -1) return;
            if (!state.IsPlaced) return;

            //todo: bring in power, items, and fluids from network here

            if(state.CurrentlyProcessingRecipe != null)
            {
                //update recipe cooking
                var r = state.CurrentlyProcessingRecipe;
                int requiredPower = r.PowerInputPerMinute;
                if(state.PowerInputBuffer < requiredPower)
                {
                    state.PowerInputBuffer = 0;
                    state.InsufficientPower = true;
                } else
                {
                    state.InsufficientPower = false;
                    state.PowerOutputBuffer += r.PowerOutputPerMinute;
                    if(state.PowerOutputBuffer > PowerOutputBufferSize) state.PowerOutputBuffer = PowerOutputBufferSize;
                    state.PowerInputBuffer -= requiredPower;
                    state.ProcessMinutesRemaining -= 1;
                    if(state.ProcessMinutesRemaining <= 0)
                    {
                        state.ProcessMinutesRemaining = 0;
                        state.CurrentlyProcessingRecipe = null;
                        FGame.Logger.Info("recipe finished");
                        //todo: recipe done cooking! do item and fluid output! (check for space if appropriate)
                    }
                }
            } else
            {
                //check for new recipe activations
                foreach(var r in Recipes)
                {
                    //check power
                    if(state.PowerInputBuffer >= r.PowerInputPerMinute)
                    {
                        //is this a power-only recipe, e.g. for a generator?
                        if(r.PowerOutputPerMinute != 0)
                        {
                            if(r.FluidOutput1.id == null && r.FluidOutput2.id == null)
                            {
                                if(r.ItemOutput1.id == null && r.ItemOutput2.id == null && r.ItemOutput3.id == null)
                                {
                                    //if so, is the power output buffer full?
                                    // if it is, let's not start a recipe that only produces power.
                                    if (state.PowerOutputBuffer >= state.Machine.PowerOutputBufferSize) continue;
                                }
                            }
                        }

                        //check fluid inputs
                        if (!r.CheckFluidIngredients(state)) continue;
                        //check item inputs
                        if (!r.CheckItemIngredients(state)) continue;

                        r.ConsumeInputs(state);
                        state.CurrentlyProcessingRecipe = r;
                        state.ProcessMinutesRemaining = r.ProcessingTimeInMinutes;
                    }
                }
            }
        }

        public virtual void OnActivate(MachineState state, GameLocation l, Farmer who, Furniture f, xTile.Dimensions.Location vect)
        {
            if (state == null || state.MachineNumber == -1) return;
            Menus.MachineMenu.Show(this, state);
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

    public class MachineRecipe
    {
        public (string id, int quantity) ItemInput1 = (null, 0);
        public (string id, int quantity) ItemInput2 = (null, 0);
        public (string id, int quantity) ItemInput3 = (null, 0);
        public (string id, int quantity) ItemOutput1 = (null, 0);
        public (string id, int quantity) ItemOutput2 = (null, 0);
        public (string id, int quantity) ItemOutput3 = (null, 0);
        public (string id, int units) FluidInput1 = (null, 0);
        public (string id, int units) FluidInput2 = (null, 0);
        public (string id, int units) FluidOutput1 = (null, 0);
        public (string id, int units) FluidOutput2 = (null, 0);
        public int PowerInputPerMinute = 0;
        public int PowerOutputPerMinute = 0;
        public int ProcessingTimeInMinutes = 0;

        public bool CheckItemIngredients(MachineState state)
        {
            if (ItemInput1.id == null && ItemInput2.id == null && ItemInput3.id == null) return true;

            var item1 = ItemInput1;
            var item2 = ItemInput2;
            var item3 = ItemInput3;
            //squash inputs
            if(item3.id != null)
            {
                if(item2.id == null || item2.id == item3.id)
                {
                    if (item2.id == null) item2.quantity = 0;
                    item2.id = item3.id;
                    item2.quantity += item3.quantity;
                    item3 = (null, 0);
                }
                if(item3.id != null)
                {
                    if (item1.id == null || item1.id == item3.id)
                    {
                        if (item1.id == null) item1.quantity = 0;
                        item1.id = item3.id;
                        item1.quantity += item1.quantity;
                        item3 = (null, 0);
                    }
                }
            }
            if(item2.id != null)
            {
                if(item1.id == null || item1.id == item2.id)
                {
                    if (item1.id == null) item1.quantity = 0;
                    item1.id = item2.id;
                    item1.quantity += item2.quantity;
                    item2 = (null, 0);
                }
            }

            bool foundItem1 = false;
            int foundItem1Quantity = 0;
            if (item1.id == null) foundItem1 = true;
            else
            {
                if (state.Inventory[0] != null && state.Inventory[0].ItemID == item1.id)
                {
                    foundItem1 = true;
                    foundItem1Quantity += state.Inventory[0].Stack;
                }
                if (state.Inventory[1] != null && state.Inventory[1].ItemID == item1.id)
                {
                    foundItem1 = true;
                    foundItem1Quantity += state.Inventory[0].Stack;
                }
                if (state.Inventory[2] != null && state.Inventory[2].ItemID == item1.id)
                {
                    foundItem1 = true;
                    foundItem1Quantity += state.Inventory[0].Stack;
                }
            }
            if (!foundItem1) return false;
            if (item1.quantity > 0 && foundItem1Quantity < item1.quantity) return false;

            bool foundItem2 = false;
            int foundItem2Quantity = 0;
            if (item2.id == null) foundItem2 = true;
            else
            {
                if (state.Inventory[0] != null && state.Inventory[0].ItemID == item2.id)
                {
                    foundItem2 = true;
                    foundItem2Quantity += state.Inventory[0].Stack;
                }
                if (state.Inventory[1] != null && state.Inventory[1].ItemID == item2.id)
                {
                    foundItem2 = true;
                    foundItem2Quantity += state.Inventory[0].Stack;
                }
                if (state.Inventory[2] != null && state.Inventory[2].ItemID == item2.id)
                {
                    foundItem2 = true;
                    foundItem2Quantity += state.Inventory[0].Stack;
                }
            }
            if (!foundItem2) return false;
            if (item2.quantity > 0 && foundItem2Quantity < item2.quantity) return false;

            bool foundItem3 = false;
            int foundItem3Quantity = 0;
            if (item3.id == null) foundItem3 = true;
            else
            {
                if (state.Inventory[0] != null && state.Inventory[0].ItemID == item3.id)
                {
                    foundItem3 = true;
                    foundItem3Quantity += state.Inventory[0].Stack;
                }
                if (state.Inventory[1] != null && state.Inventory[1].ItemID == item3.id)
                {
                    foundItem3 = true;
                    foundItem3Quantity += state.Inventory[0].Stack;
                }
                if (state.Inventory[2] != null && state.Inventory[2].ItemID == item3.id)
                {
                    foundItem3 = true;
                    foundItem3Quantity += state.Inventory[0].Stack;
                }
            }
            if (!foundItem3) return false;
            if (item3.quantity > 0 && foundItem3Quantity < item3.quantity) return false;


            return true;
        }

        public bool CheckFluidIngredients(MachineState state)
        {
            if (FluidInput1.id == null && FluidInput2.id == null) return true;
            var fluid1 = FluidInput1;
            var fluid2 = FluidInput2;

            //if someone declared two entries for the same fluid, stack them
            if(FluidInput1.id == FluidInput2.id)
            {
                fluid1.units += FluidInput2.units;
                fluid2 = (null, 0);
            }

            //find fluid 1
            bool foundFluid1 = false;
            int foundFluid1Units = 0;
            if (fluid1.id == null) foundFluid1 = true;
            else
            {
                if (state.FluidInventory[0].id == fluid1.id)
                {
                    foundFluid1 = true;
                    foundFluid1Units += state.FluidInventory[0].units;
                }
                if (state.FluidInventory[1].id == fluid1.id)
                {
                    foundFluid1 = true;
                    foundFluid1Units += state.FluidInventory[1].units;
                }
            }
            if (!foundFluid1) return false;
            if (fluid1.units > 0 && foundFluid1Units < fluid1.units) return false;

            //find fluid 2
            bool foundFluid2 = false;
            int foundFluid2Units = 0;
            if (fluid2.id == null) foundFluid2 = true;
            else
            {
                if (state.FluidInventory[0].id == fluid2.id)
                {
                    foundFluid2 = true;
                    foundFluid2Units += state.FluidInventory[0].units;
                }
                if (state.FluidInventory[1].id == fluid2.id)
                {
                    foundFluid2 = true;
                    foundFluid2Units += state.FluidInventory[1].units;
                }
            }
            if (!foundFluid2) return false;
            if (fluid2.units > 0 && foundFluid2Units < fluid2.units) return false;

            return true;
        }

        public void ConsumeInputs(MachineState state)
        {
            //we don't need to consume power, first tick will do that.
            //fluids:
            if(FluidInput1.id != null)
            {
                int reqAmount = FluidInput1.units;
                if(reqAmount > 0 && state.FluidInventory[0].id == FluidInput1.id)
                {
                    int contribution = Math.Min(state.FluidInventory[0].units, reqAmount);
                    reqAmount -= contribution;
                    state.FluidInventory[0].units -= contribution;
                    if (state.FluidInventory[0].units <= 0) state.FluidInventory[0] = (null, 0);
                }
                if (reqAmount > 0 && state.FluidInventory[1].id == FluidInput1.id)
                {
                    int contribution = Math.Min(state.FluidInventory[1].units, reqAmount);
                    reqAmount -= contribution;
                    state.FluidInventory[1].units -= contribution;
                    if (state.FluidInventory[1].units <= 0) state.FluidInventory[1] = (null, 0);
                }
            }
            if (FluidInput2.id != null)
            {
                int reqAmount = FluidInput2.units;
                if (reqAmount > 0 && state.FluidInventory[0].id == FluidInput2.id)
                {
                    int contribution = Math.Min(state.FluidInventory[0].units, reqAmount);
                    reqAmount -= contribution;
                    state.FluidInventory[0].units -= contribution;
                    if (state.FluidInventory[0].units <= 0) state.FluidInventory[1] = (null, 0);
                }
                if (reqAmount > 0 && state.FluidInventory[1].id == FluidInput2.id)
                {
                    int contribution = Math.Min(state.FluidInventory[1].units, reqAmount);
                    reqAmount -= contribution;
                    state.FluidInventory[1].units -= contribution;
                    if (state.FluidInventory[1].units <= 0) state.FluidInventory[1] = (null, 0);
                }
            }
            //items:
            if (ItemInput1.id != null)
            {
                int reqAmount = ItemInput1.quantity;
                if (reqAmount > 0 && state.Inventory[0] != null && state.Inventory[0].ItemID == ItemInput1.id)
                {
                    int contribution = Math.Min(state.Inventory[0].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[0].Stack -= contribution;
                    if (state.Inventory[0].Stack <= 0) state.Inventory[0] = null;
                }
                if (reqAmount > 0 && state.Inventory[1] != null && state.Inventory[1].ItemID == ItemInput1.id)
                {
                    int contribution = Math.Min(state.Inventory[1].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[1].Stack -= contribution;
                    if (state.Inventory[1].Stack <= 0) state.Inventory[1] = null;
                }
                if (reqAmount > 0 && state.Inventory[2] != null && state.Inventory[2].ItemID == ItemInput1.id)
                {
                    int contribution = Math.Min(state.Inventory[2].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[2].Stack -= contribution;
                    if (state.Inventory[2].Stack <= 0) state.Inventory[2] = null;
                }
            }

            if (ItemInput2.id != null)
            {
                int reqAmount = ItemInput2.quantity;
                if (reqAmount > 0 && state.Inventory[0] != null && state.Inventory[0].ItemID == ItemInput2.id)
                {
                    int contribution = Math.Min(state.Inventory[0].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[0].Stack -= contribution;
                    if (state.Inventory[0].Stack <= 0) state.Inventory[0] = null;
                }
                if (reqAmount > 0 && state.Inventory[1] != null && state.Inventory[1].ItemID == ItemInput2.id)
                {
                    int contribution = Math.Min(state.Inventory[1].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[1].Stack -= contribution;
                    if (state.Inventory[1].Stack <= 0) state.Inventory[1] = null;
                }
                if (reqAmount > 0 && state.Inventory[2] != null && state.Inventory[2].ItemID == ItemInput2.id)
                {
                    int contribution = Math.Min(state.Inventory[2].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[2].Stack -= contribution;
                    if (state.Inventory[2].Stack <= 0) state.Inventory[2] = null;
                }
            }


            if (ItemInput3.id != null)
            {
                int reqAmount = ItemInput3.quantity;
                if (reqAmount > 0 && state.Inventory[0] != null && state.Inventory[0].ItemID == ItemInput3.id)
                {
                    int contribution = Math.Min(state.Inventory[0].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[0].Stack -= contribution;
                    if (state.Inventory[0].Stack <= 0) state.Inventory[0] = null;
                }
                if (reqAmount > 0 && state.Inventory[1] != null && state.Inventory[1].ItemID == ItemInput3.id)
                {
                    int contribution = Math.Min(state.Inventory[1].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[1].Stack -= contribution;
                    if (state.Inventory[1].Stack <= 0) state.Inventory[1] = null;
                }
                if (reqAmount > 0 && state.Inventory[2] != null && state.Inventory[2].ItemID == ItemInput3.id)
                {
                    int contribution = Math.Min(state.Inventory[2].Stack, reqAmount);
                    reqAmount -= contribution;
                    state.Inventory[2].Stack -= contribution;
                    if (state.Inventory[2].Stack <= 0) state.Inventory[2] = null;
                }
            }
        }
    }

    public class MachineState
    {
        [JsonIgnore]
        [XmlIgnore]
        internal Machine Machine;

        public string MachineShortId;
        public long MachineNumber = -1;

        [JsonIgnore] //hacky workaround to use XML for inventory
        public Item[] Inventory = new Item[6];
        public string JsonInventory
        {
            get { return SerializeObjectWithXML(Inventory); }
            set { Inventory = DeserializeObjectWithXML<Item[]>(value); }
        }

        public (string id, int units)[] FluidInventory = new (string id, int units)[4]; //0-1 inputs, 2-3 outputs

        public int PowerInputBuffer = 0;
        public int PowerOutputBuffer = 0;
        public bool IsPlaced = false;

        public MachineRecipe CurrentlyProcessingRecipe = null;
        public int ProcessMinutesRemaining = 0;
        public bool InsufficientPower = false;


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

        public static string SerializeObjectWithXML<T>(T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T DeserializeObjectWithXML<T>(string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (StringReader textReader = new StringReader(toDeserialize))
            {
                return (T) xmlSerializer.Deserialize(textReader);
            }
        }
    }
}
