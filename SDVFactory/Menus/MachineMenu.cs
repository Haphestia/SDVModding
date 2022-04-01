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
    public class MachineMenu: IClickableMenu
    {
        private string hoverTitle = "";
        private string hoverText = "";
        private Item hoverItem;
        private int hoverAmount;

        private Machine Machine;
        private MachineState MachineState;

        public InventoryMenu Inventory;
        private Item heldItem;

        public List<ClickableComponent> ClickableComponents;

        internal static void Show(Machine machine, MachineState state)
        {
            Game1.activeClickableMenu = new MachineMenu(machine, state);
            Game1.dialogueUp = true;
            Game1.player.CanMove = false;
        }
        private void doOnExit()
        {
            Game1.dialogueUp = false;
            Game1.player.canMove = true;
        }

        internal MachineMenu(Machine machine, MachineState state) : base((int)Utility.getTopLeftPositionForCenteringOnScreen(800 + borderWidth * 2, 600 + borderWidth * 2).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(800 + borderWidth * 2, 600 + borderWidth * 2).Y, 800 + borderWidth * 2, 600 + borderWidth * 2)
        {
            Machine = machine;
            MachineState = state;
            Inventory = new InventoryMenu(xPositionOnScreen + spaceToClearSideBorder + borderWidth, yPositionOnScreen + spaceToClearTopBorder + borderWidth + 320 - 16, false) { showGrayedOutSlots = true };
            ClickableComponents = new List<ClickableComponent>();
            foreach (ClickableComponent item in Inventory.GetBorder(InventoryMenu.BorderSide.Top)) item.upNeighborID = -99998;
            initializeUpperRightCloseButton();
            Game1.playSound("bigSelect");
            UpdateClickables();
            if (Game1.options.SnappyMenus) snapToDefaultClickableComponent();
            exitFunction = new onExit(doOnExit);
        }

        protected void UpdateClickables()
        {
            ClickableComponents.Clear();
            populateClickableComponentList();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            int width = 800 + borderWidth * 2;
            int height = 600 + borderWidth * 2;
            var tl = new Vector2(newBounds.Width / 2 - width / 2, newBounds.Height / 2 - height / 2);
            int xdiff = xPositionOnScreen;
            int ydiff = yPositionOnScreen;
            xPositionOnScreen = (int)tl.X;
            yPositionOnScreen = (int)tl.Y;
            xdiff = xPositionOnScreen - xdiff;
            ydiff = yPositionOnScreen - ydiff;
            Inventory.movePosition(xdiff, ydiff);
        }

        protected override void noSnappedComponentFound(int direction, int oldRegion, int oldID)
        {
            base.noSnappedComponentFound(direction, oldRegion, oldID);
            if (oldRegion == 8000 && direction == 2)
            {
                currentlySnappedComponent = getComponentWithID(oldID % 10);
                currentlySnappedComponent.upNeighborID = oldID;
            }
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
            //do stuff here
            if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
            {
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
                heldItem = null;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            heldItem = Inventory.rightClick(x, y, heldItem);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            hoverTitle = "";
            hoverText = "";
            hoverItem = Inventory.hover(x, y, hoverItem);
            hoverAmount = -1;
            if (hoverItem != null)
            {
                hoverTitle = Inventory.hoverTitle;
                hoverText = Inventory.hoverText;
            }
        }

        public override bool readyToClose() => heldItem == null;

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
            drawHorizontalPartition(b, yPositionOnScreen + borderWidth + spaceToClearTopBorder + 256);
            Inventory.draw(b);
            DrawMachineUI(b);
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
        }

        private void DrawMachineUI(SpriteBatch b)
        {
            int baseX = xPositionOnScreen + borderWidth;
            int baseY = yPositionOnScreen + 104;

            drawHorizontalPartition(b, baseY + 28);
            //b.Draw(Game1.menuTexture, new Rectangle(baseX + 56, baseY + 64, 64, 252), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26), Color.White);
            //b.Draw(Game1.menuTexture, new Rectangle(baseX + 680, baseY + 64, 64, 252), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26), Color.White);
            //draw machine name
            int xmod = (((int)Game1.dialogueFont.MeasureString(Machine.DisplayName).X) / 2);
            int x = baseX + 400 - xmod;
            int y = baseY;
            b.DrawString(Game1.dialogueFont, Machine.DisplayName, new Vector2(x, y) + new Vector2(2f, 2f), Game1.textShadowColor);
            b.DrawString(Game1.dialogueFont, Machine.DisplayName, new Vector2(x, y) + new Vector2(0f, 2f), Game1.textShadowColor);
            b.DrawString(Game1.dialogueFont, Machine.DisplayName, new Vector2(x, y) + new Vector2(2f, 0f), Game1.textShadowColor);
            b.DrawString(Game1.dialogueFont, Machine.DisplayName, new Vector2(x, y), Game1.textColor);

            int leftEdge = baseX;
            int rightEdge = baseX + 800;

            //draw inputs
            int tempX = leftEdge;
            if (Machine.HasInputs)
            {
                if (Machine.HasPowerInputs)
                {
                    b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.PowerMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                    int clipAmount = 27; //5 for full, 43 for empty
                    b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.PowerMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                    tempX += (17 * 4);
                }
                if (Machine.HasFluidInputs)
                {
                    if (Machine.FluidInputs == FluidXput.ONE_FLUID)
                    {
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        int clipAmount = 9; //5 for full, 43 for empty
                        Color c = Color.Coral;
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        tempX += (17 * 4);
                    } else
                    {
                        //two fluids
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        int clipAmount = 10; //5 for full, 43 for empty
                        Color c = Color.CornflowerBlue;
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        tempX += (17 * 4);

                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        clipAmount = 37; //5 for full, 43 for empty
                        c = Color.LimeGreen;
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX - 4, (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        tempX += (17 * 4);
                    }
                }
                if (Machine.HasItemInputs)
                {
                    switch (Machine.ItemInputs)
                    {
                        case ItemXput.ONE_ITEM:
                            DrawInventorySlot(b, tempX, baseY + 44 + borderWidth + 76, 0, false);
                            break;
                        case ItemXput.TWO_ITEMS:
                            DrawInventorySlot(b, tempX, baseY + 44 + borderWidth + 20, 0, false);
                            DrawInventorySlot(b, tempX, baseY + 44 + borderWidth + 76 + 52, 2, false);
                            break;
                        case ItemXput.THREE_ITEMS:
                            DrawInventorySlot(b, tempX, baseY + 44 + borderWidth, 0, false);
                            DrawInventorySlot(b, tempX, baseY + 44 + borderWidth + 76, 1, false);
                            DrawInventorySlot(b, tempX, baseY + 44 + borderWidth + (76 * 2), 2, false);
                            break;
                    }
                    b.Draw(Game1.menuTexture, new Rectangle(tempX + 44, baseY + 64, 64, 252), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26), Color.White);
                }
                b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.IO"), new Vector2(tempX + 80, (int)baseY + 28 + borderWidth), new Rectangle(0, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            }

            //draw outputs
            tempX = rightEdge;
            if (Machine.HasInputs)
            {
                if (Machine.HasPowerInputs)
                {
                    b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.PowerMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                    int clipAmount = 7; //5 for full, 43 for empty
                    b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.PowerMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                    tempX -= (17 * 4);
                }
                if (Machine.HasFluidInputs)
                {
                    if (Machine.FluidInputs == FluidXput.ONE_FLUID)
                    {
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        int clipAmount = 8; //5 for full, 43 for empty
                        Color c = Color.Coral;
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        tempX -= (17 * 4);
                    }
                    else
                    {
                        //two fluids
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        int clipAmount = 15; //5 for full, 43 for empty
                        Color c = Color.Purple;
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        tempX -= (17 * 4);

                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth), new Rectangle(1, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        clipAmount = 5; //5 for full, 43 for empty
                        c = Color.Yellow;
                        b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.FluidMeter"), new Vector2(tempX + 4 - (16 * 4), (int)baseY + 28 + borderWidth + (clipAmount * 4)), new Rectangle(19, clipAmount, 16, 61 - clipAmount), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                        tempX -= (17 * 4);
                    }
                }
                if (Machine.HasItemInputs)
                {
                    switch (Machine.ItemInputs)
                    {
                        case ItemXput.ONE_ITEM:
                            DrawInventorySlot(b, tempX - (16 * 4), baseY + 44 + borderWidth + 76, 0, false);
                            break;
                        case ItemXput.TWO_ITEMS:
                            DrawInventorySlot(b, tempX - (16 * 4), baseY + 44 + borderWidth + 20, 0, false);
                            DrawInventorySlot(b, tempX - (16 * 4), baseY + 44 + borderWidth + 76 + 52, 2, false);
                            break;
                        case ItemXput.THREE_ITEMS:
                            DrawInventorySlot(b, tempX - (16 * 4), baseY + 44 + borderWidth, 0, false);
                            DrawInventorySlot(b, tempX - (16 * 4), baseY + 44 + borderWidth + 76, 1, false);
                            DrawInventorySlot(b, tempX - (16 * 4), baseY + 44 + borderWidth + (76 * 2), 2, false);
                            break;
                    }
                    b.Draw(Game1.menuTexture, new Rectangle(tempX - 108, baseY + 64, 64, 252), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26), Color.White);
                }
                b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.IO"), new Vector2(tempX - 80 - (16 * 4), (int)baseY + 28 + borderWidth), new Rectangle(16, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            }


            //draw output test slots
            //draw input/output shields
            //b.Draw(TextureCache.Get("bwdy.FactoryMod.Textures.IO"), new Vector2(baseX + 800 - 92 - 64, (int)baseY + 28 + borderWidth), new Rectangle(16, 0, 16, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            //DrawInventorySlot(b, baseX + 728, baseY + 44 + borderWidth, 0, false);
            //DrawInventorySlot(b, baseX + 728, baseY + 44 + borderWidth + 76, 1, false);
            //DrawInventorySlot(b, baseX + 728, baseY + 44 + borderWidth + (76 * 2), 2, false);
        }

        private void DrawInventorySlot(SpriteBatch b, int x, int y, int invSlot, bool dimmed = false)
        {
            Item item = null;
            if(MachineState != null && MachineState.Inventory != null)
            {
                if (MachineState.Inventory.ContainsKey(invSlot))
                {
                    item = MachineState.Inventory[invSlot];
                }
            }
            Vector2 vector = new Vector2(x, y);
            b.Draw(Game1.menuTexture, vector, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
            if (item == null) return;
            bool drawItemShadow = true;
            item.drawInMenu(b, vector, 1f, dimmed ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, drawItemShadow);
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
            Vector2 small_text_size = Vector2.Zero;
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
            y += 16;
            if (text != null && text.Length != 0 && (text.Length != 1 || text[0] != ' '))
            {
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4), Game1.textColor * 0.9f);
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