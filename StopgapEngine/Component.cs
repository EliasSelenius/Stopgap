using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public abstract class Component {

        public GameObject gameObject { get; private set; }
        public Transform transform => gameObject.transform;

        internal void Init(GameObject obj) {
            gameObject = obj;
        }

        /// <summary>
        /// after gameObject has Initialized, if the component is added to an already initialized gameObject it will run imidiatly
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// happens when the gameObject leaves the scene
        /// </summary>
        public virtual void OnLeave() { }

        /// <summary>
        /// happens when the gameObject enters a scene
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// The main game loop
        /// </summary>
        public virtual void Update() { }

        public virtual void OnCollision(Collider other) { }
    }
}
