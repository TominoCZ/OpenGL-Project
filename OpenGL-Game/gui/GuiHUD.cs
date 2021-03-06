﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenGL_Game.gui
{
    class GuiHUD : Gui
    {
        private GuiTexture slot;
        private GuiTexture slot_selected;

        public GuiHUD()
        {
            var slot_texture = GraphicsManager.loadTexture("gui/slot", false);
            var slot_selected_texture = GraphicsManager.loadTexture("gui/slot_selected", false);

            if (slot_texture != null)
                slot = new GuiTexture(slot_texture.textureID, slot_texture.textureSize, Vector2.Zero, Vector2.One * 2);
            if (slot_selected_texture != null)
                slot_selected = new GuiTexture(slot_selected_texture.textureID, slot_selected_texture.textureSize, Vector2.Zero, Vector2.One * 2);
        }

        public override void render(GuiShader shader, int mouseX, int mouseY)
        {
            var size = Game.INSTANCE.ClientSize;

            int space = 5;

            int scaledWidth = (int)(slot.textureSize.Width * slot.scale.X);
            int scaledHeight = (int)(slot.textureSize.Height * slot.scale.Y);

            int totalHotbarWidth = 9 * scaledWidth + 8 * space;

            int startPos = size.Width / 2 - totalHotbarWidth / 2;

            for (int i = 0; i < 9; i++)
            {
                var b = i == Game.INSTANCE.player.equippedItemHotbarIndex;

                var x = startPos + i * (scaledWidth + space);
                var y = size.Height - 20 - scaledHeight;

                renderTexture(shader, b ? slot_selected : slot, x, y);

                var stack = Game.INSTANCE.player.hotbar[i];

                if (stack != null && stack.Item is ItemBlock itemBlock)
                {
                    var block = itemBlock.getBlock();

                    x += 14;
                    y += 14;

                    renderBlock(block, 2.25f, x, y);
                }
            }
        }
    }
}
