

namespace Stopgap {
    public class Editor : Scene {

        public static readonly Editor instance;
        public static bool is_open => Game.scene == instance;

        static Editor() => instance = new Editor();
        private Editor() {
            user = spawn(new Camera(), new CamFlyController());
        }

        public Scene editing_scene;

        public readonly GameObject user;


        public static void play() {
            Game.SetScene(instance.editing_scene);
        }
        public static void open() {
            if (is_open) return;
            instance.editing_scene = Game.scene;
            Game.SetScene(instance);
        }

        internal override void render() {
            editing_scene?.render();
            base.render();
        }

        internal override void Update() {
            base.Update();


        }
    }
}