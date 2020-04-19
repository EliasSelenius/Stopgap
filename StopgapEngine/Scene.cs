using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
