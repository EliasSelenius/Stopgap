using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK;
using OpenTK.Graphics.OpenGL4;

using Glow;
using Nums;
using System.Threading;
using OpenTK.Graphics;
using Stopgap.Gui;

namespace Stopgap {

    /*
    public class Window : NativeWindow {
        public Window() {
            
        }
    }*/


    public static class Game {

        public static readonly GameWindow window;
        public static Renderer renderer { get; private set; }

        public static Camera current_camera => scene?.main_camera;
        public static Scene scene { get; private set; }
        public static Gui.Canvas canvas;

        public static Action onLoad = null;

        public static float deltaTime { get; private set; }
        public static float time { get; private set; }

        internal static Thread collisons_thread;

        static Game() {
            window = new GameWindow(1600, 900, GraphicsMode.Default, "Stopgap");
            window.VSync = VSyncMode.Off;
            //window.WindowState = WindowState.Fullscreen;
            

            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Load += Window_Load;

            Input.InitEvents();

            collisons_thread = new Thread(() => { while (true) scene?.processCollisions(); }) { IsBackground = true };

        }

        public static void SetScene(Scene scene) {
            Game.scene = scene;
        }

        public static void Run() {
            window.Run();
        }

        private static void Window_Load(object sender, EventArgs e) {
            
            renderer = new DefaultRenderer();
            //renderer = new DeferredRenderer();
            Assets.Load();
            onLoad();
            CreateDebugGUI();

            //collisons_thread.Start();

        }

        private static void CreateDebugGUI() {

            var c = Game.canvas ??= new Gui.Canvas();

            
            // FPS display:
            var fpsd = c.Create<Gui.Textbox>();
            fpsd.font_size = 0.14f;
            fpsd.size = Gui.unit2.parse("0.2vw 0.03vh");
            fpsd.pos = Gui.unit2.parse("-0.5vw 0.5vh");
            fpsd.anchor = Gui.Anchor.top_left;
            fpsd.draw_background = false;
            fpsd.OnUpdate += e => {
                fpsd.SetText("FPS: " + Math.Round(Game.window.RenderFrequency));
            };
        }

        private static void Window_UpdateFrame(object sender, FrameEventArgs e) {

            if (Input.IsKeyPressed(OpenTK.Input.Key.F1)) {
                if (Editor.is_open) Editor.play();
                else Editor.open();
            }
            
            if (!Editor.is_open) {
                time += (deltaTime = (float)e.Time);
            }
            
            Input.Update();
            canvas?.Update();
            scene.Update();
            Input.reset();
            //scene.processCollisions();
        }


        private static void Window_RenderFrame(object sender, FrameEventArgs e) {

            renderer.Render();

            GL.Flush();
            window.SwapBuffers();
        }

        private static void Window_Resize(object sender, EventArgs e) {
            GL.Viewport(0, 0, window.Width, window.Height);
            //BlurFilter.ReinitializeBuffers(Game.window.Width, Game.window.Height);
            BlurFilter.resize();
            renderer.OnWindowResize(window.Width, window.Height);
            //canvas.resize(window.Width, window.Height);
        }
    }
}
