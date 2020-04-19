using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK;
using OpenTK.Graphics.OpenGL4;

using Glow;
using Nums;

namespace Stopgap {
    public static class Game {

        public static readonly GameWindow window;

        public static Scene scene { get; private set; }
        public static Gui.Canvas canvas;

        public static Action onLoad = null;

        public static float deltaTime { get; private set; }
        public static float time { get; private set; }

        static Game() {
            window = new GameWindow(1600, 900);

            //window.WindowState = WindowState.Fullscreen;

            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Load += Window_Load;

            Input.InitEvents();

        }

        public static void SetScene(Scene scene) {
            Game.scene = scene;
            Renderer.EnsureScene(scene);
        }

        public static void Run() {
            window.Run();
        }

        private static void Window_Load(object sender, EventArgs e) {
            Assets.Load();
            onLoad();
        }


        private static void Window_UpdateFrame(object sender, FrameEventArgs e) {
            time += (deltaTime = (float)e.Time);
            
            canvas?.Update();
            Input.Update();
            scene.Update();
        }


        private static void Window_RenderFrame(object sender, FrameEventArgs e) {

            Renderer.Render();

            GL.Flush();
            window.SwapBuffers();
        }

        private static void Window_Resize(object sender, EventArgs e) {
            GL.Viewport(0, 0, window.Width, window.Height);
            BlurFilter.ReinitializeBuffers(Game.window.Width, Game.window.Height);
            Renderer.ReinitializeImageBuffers();
        }
    }
}
