﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Input;

namespace OpenGL_Game
{
    class EntityPlayerSP : Entity
    {
        public Camera camera;

        public float maxMoveSpeed = 0.25f;
        public float moveSpeed;

        public int equippedItemHotbarIndex { get; private set; }

        public ItemStack[] hotbar { get; }

        public EntityPlayerSP(Vector3 pos) : base(pos)
        {
            camera = new Camera();
            camera.pos = pos;

            boundingBox =
                new AxisAlignedBB(new Vector3(-0.3f, 0, -0.3f), new Vector3(0.3f, 1.65f, 0.3f)).offset(lastPos = pos);

            hotbar = new ItemStack[9];
        }

        public EntityPlayerSP() : this(Vector3.Zero)
        {
        }

        public override void Update()
        {
            if (Game.INSTANCE.Focused)
                UpdateCameraMovement();

            base.Update();
        }

        public override void Render(float particalTicks)
        {
            var interpolatedPos = lastPos + (pos - lastPos) * particalTicks;

            camera.pos = interpolatedPos + Vector3.UnitY * 1.625f;
        }

        private void UpdateCameraMovement()
        {
            if (Game.INSTANCE.guiScreen != null)
                return;

            var state = Keyboard.GetState();

            Vector2 dirVec = Vector2.Zero;

            if (state.IsKeyDown(Key.W))
                dirVec += camera.forward;
            if (state.IsKeyDown(Key.S))
                dirVec += -camera.forward;

            if (state.IsKeyDown(Key.A))
                dirVec += camera.left;
            if (state.IsKeyDown(Key.D))
                dirVec += -camera.left;

            float mult = 1;

            if (state.IsKeyDown(Key.LShift))
                mult = 1.5f;

            if (dirVec != Vector2.Zero)
            {
                moveSpeed = MathHelper.Clamp(moveSpeed + 0.095f, 0, maxMoveSpeed) * mult;

                motion.Xz = dirVec.Normalized() * moveSpeed;
            }
            else
                moveSpeed = 0;
        }

        public void setItemStackInHotbar(int index, ItemStack stack)
        {
            hotbar[index % 9] = stack;
        }

        public void setItemStackInSelectedSlot(ItemStack stack)
        {
            hotbar[equippedItemHotbarIndex] = stack;
        }

        public ItemStack getEquippedItemStack()
        {
            return hotbar[equippedItemHotbarIndex];
        }

        public void setSelectedSlot(int index)
        {
            equippedItemHotbarIndex = index % 9;
        }

        public void selectNextItem()
        {
            equippedItemHotbarIndex = (equippedItemHotbarIndex + 1) % 9;
        }

        public void selectPreviousItem()
        {
            if (equippedItemHotbarIndex <= 0)
                equippedItemHotbarIndex = 8;
            else
                equippedItemHotbarIndex = equippedItemHotbarIndex - 1;
        }
    }

    [Serializable]
    abstract class Item
    {
        protected object item { get; }

        string displayName { get; }

        protected Item(string displayName, object item)
        {
            this.item = item;
            this.displayName = displayName;
        }
    }

    [Serializable]
    class ItemBlock : Item
    {
        public ItemBlock(EnumBlock block) : base(block.ToString(), block)
        {
        }

        public EnumBlock getBlock()
        {
            return (EnumBlock) item;
        }
    }

    class ItemStack
    {
        public Item Item;
        
        public int Count = 1;
        public int Meta;

        public ItemStack(Item item)
        {
            Item = item;
        }

        public ItemStack(Item item, int count) : this(item)
        {
            Count = count;
        }

        public ItemStack(Item item, int count, int meta) : this(item, count)
        {
            Meta = meta;
        }
    }
}