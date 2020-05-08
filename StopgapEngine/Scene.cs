using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Gif;

namespace Stopgap {
    public class Scene {

        public Skybox skybox;

        public readonly DirectionalLight directionalLight = new DirectionalLight();


        private readonly List<GameObject> _gameObjects = new List<GameObject>();
        public ReadOnlyCollection<GameObject> gameObjects => _gameObjects.AsReadOnly();

        internal void _RemoveObject(GameObject o) {
            _gameObjects.Remove(o);
        }
        internal void _AddObject(GameObject o) {
            _gameObjects.Add(o);
        }


        internal void Update() {
            for (int i = 0; i < _gameObjects.Count; i++)
                _gameObjects[i].Update();
        }

        public GameObject Spawn(params Component[] comps) {
            var g = new GameObject(comps);
            g.EnterScene(this);
            return g;
        }

        internal void processCollisions() {
            for (int i = 0; i < gameObjects.Count; i++) {
                var g1 = gameObjects[i];
                for (int j = i + 1; j < gameObjects.Count; j++) {
                    var g2 = gameObjects[j];

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

    }
}
