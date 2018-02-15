﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenGL_Game
{
    class TextureRegistry
    {
        public static int textureAtlasID;

        private static Dictionary<EnumBlock, BlockTextureUV> UVs = new Dictionary<EnumBlock, BlockTextureUV>();

        public static void stitchTextures()
        {
            textureAtlasID = loadTextureMap(generateTextureMap());
        }

        private static Bitmap generateTextureMap()
        {
            Bitmap map = new Bitmap(64, 64);

            var blocks = Enum.GetValues(typeof(EnumBlock));
            var sides = Enum.GetValues(typeof(EnumFacing));

            var dir = "assets/textures/blocks/";
            var files = Directory.GetFiles("assets/textures/blocks", "*.png");

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]).ToLower();
            }

            int countX = 0;
            int countY = 0;

            float sizeXY = 16f / map.Size.Width;

            using (map)
            {
                using (var g = Graphics.FromImage(map))
                {
                    foreach (EnumBlock block in blocks)
                    {
                        var name = block.ToString().ToLower();

                        if (containsContaining(files, name))
                        {
                            var uvs = new BlockTextureUV();

                            if (files.Contains(name))
                            {
                                if (countX * 16 >= map.Width)
                                {
                                    countX = 0;
                                    countY++;
                                }

                                var pos = new Vector2(countX, countY) * sizeXY;
                                var end = pos + Vector2.One * sizeXY;

                                uvs.fill(pos, end);

                                using (var bmp = Image.FromFile(dir + name + ".png"))
                                {
                                    g.DrawImageUnscaled(bmp, countX * 16, countY * 16);
                                }

                                countX++;
                            }

                            foreach (EnumFacing side in sides)
                            {
                                var sideName = side.ToString().ToLower();

                                if (files.Contains(name + "_" + sideName))
                                {
                                    if (countX * 16 >= map.Size.Width)
                                    {
                                        countX = 0;
                                        countY++;
                                    }

                                    var pos = new Vector2(countX, countY) * sizeXY;
                                    var end = pos + Vector2.One * sizeXY;

                                    uvs.setUVForSide(side, pos, end);

                                    using (var bmp = Image.FromFile(dir + name + "_" + sideName + ".png"))
                                    {
                                        g.DrawImageUnscaled(bmp, countX * 16, countY * 16);
                                    }

                                    countX++;
                                }
                            }

                            UVs.Add(block, uvs);
                        }
                    }
                }

                map.Save("file.png");

                return (Bitmap)map.Clone();
            }
        }

        public static int loadTextureMap(Bitmap textureMap)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);

            BitmapData data = textureMap.LockBits(new Rectangle(0, 0, textureMap.Width, textureMap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            textureMap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.MirroredRepeat);

            return texID;
        }

        private static bool containsContaining(Array a, string s)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (((string)a.GetValue(i)).Contains(s))
                    return true;
            }

            return false;
        }

        public static BlockTextureUV getUVsFromBlock(EnumBlock block)
        {
            UVs.TryGetValue(block, out var uvs);

            return uvs;
        }
    }

    class BlockTextureUV
    {
        private Dictionary<EnumFacing, TextureUVNode> UVs;

        public BlockTextureUV()
        {
            UVs = new Dictionary<EnumFacing, TextureUVNode>();
        }

        public void setUVForSide(EnumFacing side, Vector2 from, Vector2 to)
        {
            if (UVs.ContainsKey(side))
                UVs.Remove(side);

            UVs.Add(side, new TextureUVNode(from, to));
        }

        public TextureUVNode getUVForSide(EnumFacing side)
        {
            UVs.TryGetValue(side, out var uv);

            return uv;
        }

        public void fill(Vector2 from, Vector2 to)
        {
            var values = Enum.GetValues(typeof(EnumFacing));

            foreach (EnumFacing side in values)
            {
                setUVForSide(side, from, to);
            }
        }

        public float[] ToArray()
        {
            var values = Enum.GetValues(typeof(EnumFacing));

            List<float> floats = new List<float>();

            foreach (EnumFacing value in values)
            {
                if (UVs.TryGetValue(value, out var node))
                    floats.AddRange(node.ToArray());
            }

            return floats.ToArray();
        }
    }

    class TextureUVNode
    {
        public Vector2 start;
        public Vector2 end;

        public TextureUVNode(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }

        public float[] ToArray()
        {
            return new[]
            {
                start.X, start.Y,
                start.X, end.Y,
                end.X, end.Y,
                end.X, start.Y
            };
        }
    }
}
