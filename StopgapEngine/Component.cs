using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public abstract class Component {

        public GameObject gameObject { get; private set; }
        public Transform transform => gameObject.transform;
        public Scene scene => gameObject.scene;

        internal void Init(GameObject obj) {
            gameObject = obj;
        }

        internal void destroy() {
            OnDestroy();
            gameObject = null;
        }

        internal void Enter() {
            scene.UpdateEvent += _update;
            OnEnter();
        }
        internal void Leave() {
            scene.UpdateEvent -= _update;
            OnLeave();
        }


        /// <summary>
        /// happens when the gameObject enters a scene
        /// </summary>
        protected virtual void OnEnter() { }

        /// <summary>
        /// happens when the gameObject leaves the scene
        /// </summary>
        protected virtual void OnLeave() { }

        /// <summary>
        /// after gameObject has Initialized, if the component is added to an already initialized gameObject it will run imidiatly
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// The main game loop
        /// </summary>
        protected virtual void Update() { }
        protected virtual IEnumerator UpdateEnum() { return null; }
        private IEnumerator update_enumerator;
        private void _update() {
            Update();

            if (update_enumerator == null) update_enumerator = UpdateEnum();
            else if (!update_enumerator.MoveNext()) update_enumerator = null;
            
        }

        public virtual void OnCollision(Collider other) { }

        protected virtual void OnDestroy() { }
    }
}
