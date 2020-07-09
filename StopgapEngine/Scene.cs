using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Gif;
using Nums;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace Stopgap {
    public class Scene {

        public Camera main_camera { get; internal set; }

        public Skybox skybox;
        public readonly DirectionalLight directionalLight = new DirectionalLight();


        private readonly List<GameObject> _gameObjects = new List<GameObject>();
        public ReadOnlyCollection<GameObject> gameObjects => _gameObjects.AsReadOnly();

        internal readonly List<IRenderable> renderables = new List<IRenderable>();  
        internal virtual void render() {
            foreach (var renderable in renderables) {
                renderable.render();
            }

            // skybox
            skybox?.render();
        }


        internal void _remove_object(GameObject o) {
            _gameObjects.Remove(o);
        }
        internal void _add_object(GameObject o) {
            _gameObjects.Add(o);
        }

        
        internal void Update() {
            UpdateEvent?.Invoke();
        }

        internal event Action UpdateEvent;

        public GameObject spawn(params Component[] comps) {
            var g = new GameObject(comps);
            g.EnterScene(this);
            return g;
        }

        internal void processCollisions() {

            for (int i = 0; i < _gameObjects.Count; i++) {
                var g1 = _gameObjects[i];
                for (int j = i + 1; j < _gameObjects.Count; j++) {
                    var g2 = _gameObjects[j];
                    
                    if (g1 == null) break;

                    for (int c1 = 0; c1 < g1.colliders.Count; c1++) {
                        for (int c2 = 0; c2 < g2.colliders.Count; c2++) {
                            var co1 = g1.colliders[c1];
                            var co2 = g2.colliders[c2];
                            if (co1.intersects(co2)) {
                                g1.OnCollision(co2);
                                g2.OnCollision(co1);
                            }
                        }
                    }
                }
            }
        }


        public GameObject raycast(vec3 pos, vec3 dir) {
            for (int i = 0; i < _gameObjects.Count; i++) {
                var g = _gameObjects[i];
                var c = g.colliders;
                for (int j = 0; j < c.Count; j++) {
                    if (c[j].intersectsRay(pos, dir)) return g;
                }
            }
            return null;
        }

    }

    public class Editor : Scene {

        public static readonly Editor instance;

        static Editor() => instance = new Editor();
        private Editor() {
            user = spawn(new Camera(), new CamFlyController());
        }

        public Scene editing_scene;

        private GameObject user;


        public static void play() {
            Game.SetScene(instance.editing_scene);
        }
        public static void open() {
            if (Game.scene == instance) return;
            instance.editing_scene = Game.scene;
            Game.SetScene(instance);
        }

        internal override void render() {
            editing_scene?.render();
            base.render();
        }
    }
}