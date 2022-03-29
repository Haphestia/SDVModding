using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDVFactory.Data
{
    internal static class TableRecipes
    {
        internal static List<TableRecipe> Recipes = new List<TableRecipe>() {
            
            //Primitive Generator
            new TableRecipe(Machines.MachineList["Generator1"]){
                TableLayout = new Point(0,0),
                Ingredients = new Dictionary<string, int>() {
                    { "388", 5 }, //wood
                    { "390", 15 } //stone
                }
            }

        };
    }

    internal class TableRecipe
    {
        public string UniqueId { get => "bwdy.FactoryMod.Recipes.Table." + ShortId; }
        public Texture2D Texture { get => TextureCache.Get(TextureName); }
        public string ShortId;
        public string DisplayName;
        public string Description;
        public Point Size;
        public string TextureName;
        public Point SheetIndex;
        public Point TableLayout;
        public Dictionary<string, int> Ingredients;

        public TableRecipe() { }
        public TableRecipe(Machine machine)
        {
            ShortId = machine.ShortId;
            DisplayName = machine.DisplayName;
            Description = machine.Description;
            Size = machine.DrawSize;
            TextureName = machine.TextureName;
            SheetIndex = machine.TextureTopLeftTile;
        }

        public Machine GetMachine()
        {
            return Machines.MachineList[ShortId];
        }

        public bool CanAfford()
        {
            foreach (var i in Ingredients) if (i.Value - Game1.player.getItemCount(i.Key) > 0) return false;
            return true;
        }

        public void ConsumeIngredients()
        {
            for (int i = Ingredients.Count - 1; i >= 0; i--)
            {
                string key = Ingredients.Keys.ElementAt(i);
                int required_count = Ingredients[key];
                for (int ii = Game1.player.Items.Count - 1; ii >= 0; ii--)
                {
                    if (CraftingRecipe.ItemMatchesForCrafting(Game1.player.Items[ii], key))
                    {
                        int toRemove = required_count;
                        required_count -= Game1.player.Items[ii].Stack;
                        Game1.player.Items[ii] = Game1.player.Items[ii].ConsumeStack(toRemove);
                        if (required_count <= 0) break;
                    }
                }
            }
        }

        public Item Craft()
        {
            return GetMachine().CreateOne();
        }

        public int GetDescriptionHeight(int width)
        {
            return (int)(Game1.smallFont.MeasureString(Game1.parseText(Description, Game1.smallFont, width)).Y + (float)(Ingredients.Count * 36) + (float)(int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567")).Y + 21f);
        }

        public string getNameFromIndex(string item_id)
        {
            if (item_id != null && item_id.StartsWith("-"))
            {
                switch (item_id)
                {
                    case "-1": return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568");
                    case "-2": return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
                    case "-3": return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
                    case "-4": return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
                    case "-5": return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
                    case "-6": return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
                    default: return (item_id == (-777).ToString()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.574")  : "???";
                }
            }
            ParsedItemData item_data = Utility.GetItemDataForItemID(item_id);
            return (item_data != null) ? item_data.displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575");
        }

        public virtual void drawRecipeDescription(SpriteBatch b, Vector2 position, int width)
        {
            int lineExpansion = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 8 : 0);
            b.Draw(Game1.staminaRect, new Rectangle((int)(position.X + 8f), (int)(position.Y + 32f + Game1.smallFont.MeasureString("Ing!").Y) - 4 - 2 - (int)((float)lineExpansion * 1.5f), width - 32, 2), Game1.textColor * 0.35f);
            Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont, position + new Vector2(8f, 28f), Game1.textColor * 0.75f);
            for (int i = 0; i < Ingredients.Count; i++)
            {
                int required_count = Ingredients.Values.ElementAt(i);
                string required_item = Ingredients.Keys.ElementAt(i);
                int bag_count = Game1.player.getItemCount(required_item, 8);
                int containers_count = 0;
                required_count -= bag_count;
                string ingredient_name_text = this.getNameFromIndex(Ingredients.Keys.ElementAt(i));
                Color drawColor = ((required_count <= 0) ? Game1.textColor : Color.Red);
                ParsedItemData itemDataForItemID = Utility.GetItemDataForItemID(Ingredients.Keys.ElementAt(i));
                Texture2D texture = itemDataForItemID.texture;
                Rectangle source_rect = itemDataForItemID.GetSourceRect(0);
                float scale = 2f;
                if (source_rect.Width > 0 || source_rect.Height > 0)
                {
                    scale *= 16f / (float)Math.Max(source_rect.Width, source_rect.Height);
                }
                b.Draw(texture, new Vector2(position.X + 16f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 16f), source_rect, Color.White, 0f, new Vector2(source_rect.Width / 2, source_rect.Height / 2), scale, SpriteEffects.None, 0.86f);
                Utility.drawTinyDigits(Ingredients.Values.ElementAt(i), b, new Vector2(position.X + 32f - Game1.tinyFont.MeasureString(Ingredients.Values.ElementAt(i).ToString() ?? "").X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 21f), 2f, 0.87f, Color.AntiqueWhite);
                Vector2 text_draw_position = new Vector2(position.X + 32f + 8f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 4f);
                Utility.drawTextWithShadow(b, ingredient_name_text, Game1.smallFont, text_draw_position, drawColor);
                if (Game1.options.showAdvancedCraftingInformation)
                {
                    text_draw_position.X = position.X + (float)width - 40f;
                    b.Draw(Game1.mouseCursors, new Rectangle((int)text_draw_position.X, (int)text_draw_position.Y + 2, 22, 26), new Rectangle(268, 1436, 11, 13), Color.White);
                    Utility.drawTextWithShadow(b, (bag_count + containers_count).ToString() ?? "", Game1.smallFont, text_draw_position - new Vector2(Game1.smallFont.MeasureString(bag_count + containers_count + " ").X, 0f), drawColor);
                }
            }
            b.Draw(Game1.staminaRect, new Rectangle((int)position.X + 8, (int)position.Y + lineExpansion + 64 + 4 + Ingredients.Count * 36, width - 32, 2), Game1.textColor * 0.35f);
            Utility.drawTextWithShadow(b, Game1.parseText(Description, Game1.smallFont, width - 8), Game1.smallFont, position + new Vector2(0f, 76 + Ingredients.Count * 36 + lineExpansion), Game1.textColor * 0.75f);
        }
    }
}
