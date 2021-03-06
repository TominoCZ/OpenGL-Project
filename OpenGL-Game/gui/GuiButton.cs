﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace OpenGL_Game
{
    class GuiButton : Gui
    {
        public static GuiTexture GUI_BUTTON;
        public static GuiTexture GUI_BUTTON_HOVER;

        public int ID;

        public int posX;
        public int posY;

        public Vector2 scale = Vector2.One;

        public bool centered;

        static GuiButton()
        {
            GUI_BUTTON = new GuiTexture(GraphicsManager.loadTexture("gui/button", false));
            GUI_BUTTON_HOVER = new GuiTexture(GraphicsManager.loadTexture("gui/button_hover", false));
        }

        public GuiButton(int ID, int x, int y)
        {
            this.ID = ID;

            posX = x;
            posY = y;
        }

        public GuiButton(int ID, int x, int y, Vector2 scale) : this(ID, x, y)
        {
            this.scale = scale;
        }

        public override void render(GuiShader shader, int mouseX, int mouseY)
        {
            GuiTexture tex = GUI_BUTTON;

            if (isMouseOver(mouseX, mouseY))
                tex = GUI_BUTTON_HOVER;

            if (centered)
            {
                posX = (int)(Game.INSTANCE.ClientSize.Width / 2f - tex.textureSize.Width * scale.X / 2f);
                renderTexture(shader, tex, scale, posX, posY);
            }
            else
                renderTexture(shader, tex, scale, posX, posY);
        }

        public virtual void Dispose()
        {

        }

        internal bool isMouseOver(int x, int y)
        {
            return x >= posX &&
                   y >= posY &&
                   x <= posX + GUI_BUTTON.textureSize.Width * scale.X &&
                   y <= posY + GUI_BUTTON.textureSize.Height * scale.Y;
        }
    }
}
