﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace OpenGL_Game
{
    class GameMain
    {
        [STAThread]
        static void Main()
        {
            var window = new MainWindow();
            window.Run(0);
        }
    }

    public sealed class MainWindow : GameWindow
    {
        private Renderer renderer;
        private Camera camera;

        private Loader loader;

        private StaticShader shader;

        private WindowState lastWindowState;

        Stopwatch sw;

        int frames;

        public MainWindow()
        {
            CursorVisible = false;
            VSync = VSyncMode.Off;

            sw = new Stopwatch();
            sw.Start();

            Title = "OpenGL Game";

            MakeCurrent();

            initRenderer();

            new Thread(() =>
            {
                int i = 0;

                while (true)
                {
                    // mouse, every 5ms - 200Hz
                    if (Focused)
                    {
                        // every 15ms
                        if (++i >= 2)
                        {
                            camera.move();
                            i = 0;
                        }

                        var center = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));

                        var state = OpenTK.Input.Mouse.GetCursorState();
                        var point = new Point(state.X, state.Y);
                        var delta = new Point(center.X - point.X, center.Y - point.Y);

                        camera.yaw -= delta.X / 1000f;
                        camera.pitch -= delta.Y / 1000f;

                        OpenTK.Input.Mouse.SetPosition(center.X, center.Y);
                    }

                    Thread.Sleep(8);
                }
            })
            { IsBackground = true }.Start();
        }

        void initRenderer()
        {
            shader = new StaticShader("texture");
            camera = new Camera();
            renderer = new Renderer(this, shader, camera);
            loader = new Loader();

            var texture = new ModelTexture(loader.loadTexture("image"));
            var model = new Model(texture, shader);

            model.addVertices(
                -0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,

                0.5f, 0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, 0.5f, -0.5f,

                -0.5f, 0.5f, -0.5f,
                -0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, -0.5f,

                -0.5f, -0.5f, 0.5f,
                -0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, 0.5f,

                -0.5f, 0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, 0.5f,
                -0.5f, 0.5f, 0.5f,

                0.5f, 0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, -0.5f,
                0.5f, 0.5f, -0.5f);

            for (int i = 0; i < 24; i += 4)
            {
                model.addIndices(
                    i, i + 1, i + 3,
                    i + 3, i + 1, i + 2);

                model.addUVs(
                    0, 0,
                    0, 1,
                    1, 1,
                    1, 0);
            }

            model.bake(loader);

            List<BlockNode> blocks = new List<BlockNode>();

            for (int x = 0; x < 64; x++)
            {
                for (int z = 0; z < 64; z++)
                {
                    var entity = new BlockNode(model, new Vector3(-x, -1, -z), 1f);

                    blocks.Add(entity);
                }
            }

            renderer.blockRenderer.addBlockNodes(blocks.ToArray());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            frames++;

            if (sw.ElapsedMilliseconds >= 1000)
            {
                Console.WriteLine(frames + " FPS");

                frames = 0;
                sw.Restart();
            }

            renderer.render();

            SwapBuffers();
            ProcessEvents(true);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (Keyboard.GetState().IsKeyDown(Key.LAlt | Key.F4))
                Exit();

            if (e.Key == Key.F11)
            {
                if (WindowState != WindowState.Fullscreen)
                {
                    lastWindowState = WindowState;
                    WindowState = WindowState.Fullscreen;
                }
                else
                    WindowState = lastWindowState;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ClientSize.Width, ClientSize.Height, 0, -1, 0);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            shader.DetachShader();
            loader.cleanUp();
        }
    }
}
