using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SDVFactory.Data;
using StardewValley;
using StardewValley.Menus;

namespace SDVFactory.Menus
{
    public class MenuEngineeringTable : IClickableMenu
    {
        private string hoverTitle = "";
        private string hoverText = "";
        private Item hoverItem;
        private TableRecipe hoverRecipe;
        private int hoverAmount;
        private Item lastCookingHover;

        public InventoryMenu Inventory;
        private Item heldItem;

        [SkipForClickableAggregation]
        private Dictionary<ClickableTextureComponent, TableRecipe> ClickableRecipes = new Dictionary<ClickableTextureComponent, TableRecipe>();
        public List<ClickableComponent> ClickableComponents;
        private List<TableRecipe> Recipes;

        public static void Show()
        {
            Game1.activeClickableMenu = new MenuEngineeringTable();
            Game1.dialogueUp = true;
            Game1.player.CanMove = false;
        }
        private void doOnExit()
        {
            Game1.dialogueUp = false;
            Game1.player.canMove = true;
        }

        public MenuEngineeringTable() : base( (int)Utility.getTopLeftPositionForCenteringOnScreen(800 + borderWidth * 2, 600 + borderWidth * 2).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(800 + borderWidth * 2, 600 + borderWidth * 2).Y, 800 + borderWidth * 2, 600 + borderWidth * 2 )
        {
            Recipes = TableRecipes.Recipes;
            Inventory = new InventoryMenu(xPositionOnScreen + spaceToClearSideBorder + borderWidth, yPositionOnScreen + spaceToClearTopBorder + borderWidth + 320 - 16, false) { showGrayedOutSlots = true };
            ClickableComponents = new List<ClickableComponent>();
            foreach (ClickableComponent item in Inventory.GetBorder(InventoryMenu.BorderSide.Top)) item.upNeighborID = -99998;
            initializeUpperRightCloseButton();
            Game1.playSound("bigSelect");
            LoadRecipeLayout();
            UpdateClickables();
            if (Game1.options.SnappyMenus) snapToDefaultClickableComponent();
            exitFunction = new onExit(doOnExit);
        }

        private void LoadRecipeLayout()
        {
            int craftingPageX = xPositionOnScreen + spaceToClearSideBorder + borderWidth - 16;
            int spaceBetweenCraftingIcons = 8;
            int x = 0;
            int y = 0;
            int i = 0;
            foreach (var recipe in Recipes)
            {
                i++;
                int id = 200 + i;
                var source_rect = new Rectangle(recipe.SheetIndex.X * 16, recipe.SheetIndex.Y * 16, recipe.Size.X * 16, recipe.Size.Y * 16);
                var component = new ClickableTextureComponent("", new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), (yPositionOnScreen + spaceToClearTopBorder + borderWidth - 16) + y * 72, recipe.Size.X * 64, recipe.Size.Y * 64), null, "", recipe.Texture, source_rect, 4f) { myID = id, rightNeighborID = -99998, leftNeighborID = -99998, upNeighborID = -99998, downNeighborID = -99998, fullyImmutable = true, region = 8000 };
                ClickableRecipes.Add(component, recipe);
            }
        }

        protected void UpdateClickables()
        {
            ClickableComponents.Clear();
            foreach (ClickableTextureComponent component in ClickableRecipes.Keys) ClickableComponents.Add(component);
            populateClickableComponentList();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            int width = 800 + borderWidth * 2;
            int height = 600 + borderWidth * 2;
            var tl = new Vector2(newBounds.Width / 2 - width / 2, newBounds.Height / 2 - height / 2);
            xPositionOnScreen = (int)tl.X;
            yPositionOnScreen = (int)tl.Y;
        }

        protected override void noSnappedComponentFound(int direction, int oldRegion, int oldID)
        {
            base.noSnappedComponentFound(direction, oldRegion, oldID);
            if (oldRegion == 8000 && direction == 2) {
                currentlySnappedComponent = getComponentWithID(oldID % 10);
                currentlySnappedComponent.upNeighborID = oldID;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = ClickableRecipes.First().Key;
            base.snapCursorToCurrentSnappedComponent();
        }

        protected override void actionOnRegionChange(int oldRegion, int newRegion)
        {
            base.actionOnRegionChange(oldRegion, newRegion);
            if (newRegion != 9000 || oldRegion == 0) return;
            for (int i = 0; i < 10; i++) if (Inventory.inventory.Count > i) Inventory.inventory[i].upNeighborID = currentlySnappedComponent.upNeighborID;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y);
            heldItem = Inventory.leftClick(x, y, heldItem);
            foreach (ClickableTextureComponent c in ClickableRecipes.Keys)
            {
                if (c.containsPoint(x, y) && ClickableRecipes[c].CanAfford()) CraftTheRecipe(ClickableRecipes[c], true);
                if (heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift) && heldItem.maximumStackSize() == 1 && Game1.player.couldInventoryAcceptThisItem(heldItem))
                {
                    Game1.player.addItemToInventoryBool(heldItem);
                    heldItem = null;
                }
            }
            if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
            {
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
                heldItem = null;
            }
        }

        private void CraftTheRecipe(TableRecipe recipe, bool playSound = true)
        {
            if (heldItem == null)
            {
                recipe.ConsumeIngredients();
                heldItem = recipe.Craft();
                if (playSound) Game1.playSound("coin");
            }
            else return;
            if (Game1.options.gamepadControls && heldItem != null && Game1.player.couldInventoryAcceptThisItem(heldItem))
            {
                Game1.player.addItemToInventoryBool(heldItem);
                heldItem = null;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.heldItem = this.Inventory.rightClick(x, y, this.heldItem);
            foreach (ClickableTextureComponent c in ClickableRecipes.Keys)
            {
                if (c.containsPoint(x, y) && !c.hoverText.Equals("ghosted") && ClickableRecipes[c].CanAfford()) CraftTheRecipe(ClickableRecipes[c]);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            hoverTitle = "";
            hoverText = "";
            hoverRecipe = null;
            hoverItem = Inventory.hover(x, y, hoverItem);
            hoverAmount = -1;
            if (hoverItem != null)
            {
                hoverTitle = Inventory.hoverTitle;
                hoverText = Inventory.hoverText;
            }
            foreach (ClickableTextureComponent c in ClickableRecipes.Keys)
            {
                if (c.containsPoint(x, y))
                {
                    hoverRecipe = ClickableRecipes[c];
                    if (lastCookingHover == null || !lastCookingHover.DisplayName.Equals(hoverRecipe.DisplayName)) lastCookingHover = hoverRecipe.Craft();
                    c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
                }
                else c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
            }
        }

        public override bool readyToClose() => heldItem == null;

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
            drawHorizontalPartition(b, yPositionOnScreen + borderWidth + spaceToClearTopBorder + 256);
            Inventory.draw(b);
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (ClickableTextureComponent c in ClickableRecipes.Keys)
            {
                if (!ClickableRecipes[c].CanAfford()) c.draw(b, Color.DimGray * 0.4f, 0.89f);
                else c.draw(b);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (hoverItem != null) drawToolTip(b, hoverText, hoverTitle, hoverItem, heldItem != null);
            else if (!string.IsNullOrEmpty(hoverText))
            {
                if (hoverAmount > 0) drawToolTip(b, hoverText, hoverTitle, null, true, -1, 0, null, -1, null, hoverAmount);
                else drawHoverText(b, hoverText);
            }
            if (heldItem != null) heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
            if (hoverRecipe != null) drawHoverText(b, " ");
        }

        internal void drawHoverText(SpriteBatch b, string ts)
        {
            var text = HoverTextStringBuilder;
            text.Clear();
            text.Append(ts);
            if (text == null || text.Length == 0) return;
            int xOffset = heldItem != null ? 48 : 0;
            int yOffset = heldItem != null ? 48 : 0;
            var font = Game1.smallFont;
            string bold_title_subtext = null;
            if (hoverRecipe.DisplayName != null && hoverRecipe.DisplayName.Length == 0) hoverRecipe.DisplayName = null;
            int width = Math.Max((int)font.MeasureString(text).X, (hoverRecipe.DisplayName != null) ? ((int)Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName).X) : 0) + 32;
            int height = Math.Max(20 * 3, (int)font.MeasureString(text).Y + 32 + (int)(8f + (int)((hoverRecipe.DisplayName != null) ? (Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName).Y + 16f) : 0f)));
            string categoryName = null;
            if (lastCookingHover != null)
            {
                categoryName = lastCookingHover.getCategoryName();
                if (categoryName.Length > 0)
                {
                    width = Math.Max(width, (int)font.MeasureString(categoryName).X + 32);
                    height += (int)font.MeasureString("T").Y;
                }
                int buffer = 92;
                Point p = lastCookingHover.getExtraSpaceNeededForTooltipSpecialIcons(font, width, buffer, height, text, hoverRecipe.DisplayName, -1);
                width = ((p.X != 0) ? p.X : width);
                height = ((p.Y != 0) ? p.Y : height);
            }
            Vector2 small_text_size = Vector2.Zero;
            if (hoverRecipe != null)
            {
                width = (int)Math.Max(Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName).X + small_text_size.X + 12f, 384f);
                height += hoverRecipe.GetDescriptionHeight(width - 8) - 32;
            }
            else if (bold_title_subtext != null && hoverRecipe.DisplayName != null)
            {
                small_text_size = Game1.smallFont.MeasureString(bold_title_subtext);
                width = (int)Math.Max(width, Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName).X + small_text_size.X + 12f);
            }
            int x = Game1.getOldMouseX() + 32 + xOffset;
            int y = Game1.getOldMouseY() + 32 + yOffset;
            if (x + width > Utility.getSafeArea().Right)
            {
                x = Utility.getSafeArea().Right - width;
                y += 16;
            }
            if (y + height > Utility.getSafeArea().Bottom)
            {
                x += 16;
                if (x + width > Utility.getSafeArea().Right)
                {
                    x = Utility.getSafeArea().Right - width;
                }
                y = Utility.getSafeArea().Bottom - height;
            }
            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width + ((hoverRecipe != null) ? 21 : 0), height, Color.White);
            if (hoverRecipe.DisplayName != null)
            {
                Vector2 bold_text_size = Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName);
                drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width + ((hoverRecipe != null) ? 21 : 0), (int)Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName).Y + 32 + (int)((lastCookingHover != null && categoryName.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, Color.White, 1f, drawShadow: false);
                b.Draw(Game1.menuTexture, new Rectangle(x + 12, y + (int)Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName).Y + 32 + (int)((lastCookingHover != null && categoryName.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, width - 4 * ((hoverRecipe != null) ? 1 : 6), 4), new Rectangle(44, 300, 4, 4), Color.White);
                b.DrawString(Game1.dialogueFont, hoverRecipe.DisplayName, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
                b.DrawString(Game1.dialogueFont, hoverRecipe.DisplayName, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
                b.DrawString(Game1.dialogueFont, hoverRecipe.DisplayName, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
                if (bold_title_subtext != null)
                {
                    Utility.drawTextWithShadow(b, bold_title_subtext, Game1.smallFont, new Vector2((float)(x + 16) + bold_text_size.X, (int)((float)(y + 16 + 4) + bold_text_size.Y / 2f - small_text_size.Y / 2f)), Game1.textColor);
                }
                y += (int)Game1.dialogueFont.MeasureString(hoverRecipe.DisplayName).Y;
            }
            if (lastCookingHover != null && categoryName.Length > 0)
            {
                y -= 4;
                Utility.drawTextWithShadow(b, categoryName, font, new Vector2(x + 16, y + 16 + 4), lastCookingHover.getCategoryColor(), 1f, -1f, 2, 2);
                y += (int)font.MeasureString("T").Y + ((hoverRecipe.DisplayName != null) ? 16 : 0) + 4;
            }
            y += 16;
            if (text != null && text.Length != 0 && (text.Length != 1 || text[0] != ' '))
            {
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4), Game1.textColor * 0.9f);
                y += (int)font.MeasureString(text).Y + 4;
            }
            if (hoverRecipe != null)
            {
                hoverRecipe.drawRecipeDescription(b, new Vector2(x + 16, y - 8), width);
                y += hoverRecipe.GetDescriptionHeight(width - 8);
            }
        }

        protected override bool _ShouldAutoSnapPrioritizeAlignedElements() => false;

        public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
        {
            if (a.region == 8000 && (direction == 3 || direction == 1) && b.region == 9000)
            {
                return false;
            }
            return base.IsAutomaticSnapValid(direction, a, b);
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            if (heldItem != null)
            {
                Item item = heldItem;
                heldItem = null;
                Utility.CollectOrDrop(item);
            }
        }
    }
}