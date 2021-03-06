﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Stopgap {
    public class GameObject {

        public Scene scene { get; private set; }
        public GameObject parent { get; private set; }

        private readonly List<GameObject> _children = new List<GameObject>();
        public ReadOnlyCollection<GameObject> children => _children.AsReadOnly();

        public readonly Transform transform = new Transform();

        public bool is_root => parent == null;
        public bool is_parent => _children.Count > 0;
        public bool is_child => parent != null;

        public Matrix4 model_matrix {
            get {
                var m = transform.matrix;
                return is_child ? m * parent.model_matrix : m;
            }
        }


        private readonly List<Component> _components = new List<Component>();
        public ReadOnlyCollection<Component> components => _components.AsReadOnly();
        
        public Rigidbody rigidbody { get; private set; }
        private readonly List<Collider> _colliders = new List<Collider>();
        public ReadOnlyCollection<Collider> colliders => _colliders.AsReadOnly();


        public GameObject(params Component[] comps) {
            addComponents(comps);
        }

        public T getComponent<T>() where T : Component {
            return (from c in _components
                    where c is T
                    select c).FirstOrDefault() as T;
        }

        public Component getComponent(Type type) {
            return (from c in _components
                    where type.IsInstanceOfType(c)
                    select c).FirstOrDefault();
        }

        public void addChild(GameObject obj) {
            // make sure the child object is in the same scene as the parent
            if (obj.scene != scene) obj.enterScene(scene);
            
            _children.Add(obj);
            obj.parent = this;
        }

        public void removeChild(GameObject obj) {
            _children.Remove(obj);
            obj.parent = null;
        }

        public void addComponent(Component c) {
            if (c.gameObject != null) throw new Exception("Component is already attached to a object");

            // keep track of 'offten used' components
            if (c is Rigidbody r) rigidbody = r;
            else if (c is Collider col) _colliders.Add(col);

            _components.Add(c);
            c.Init(this);
            if (has_started) { 
                c.Start();
                if (scene != null) c.Enter();
            }
        }

        public void addComponents(params Component[] comps) {
            foreach (var item in comps) {
                addComponent(item);
            }
        }

        public bool has_started { get; private set; } = false;
        internal void Start() {
            for (int i = 0; i < components.Count; i++) components[i].Start();
            has_started = true;
        }


        public void enterScene(Scene s) {
            // if we already are inside the scene
            if (scene == s) return;

            // make sure gameobject has 'started'
            if (!has_started) this.Start();
            // allow chilldren to enter first
            for (int i = 0; i < _children.Count; i++) _children[i].enterScene(s);
            // leave current scene before entering the new one
            if (scene != null) leaveScene();
            // enter scene
            scene = s;
            scene._add_object(this);
            // notify components that we have entered a scene
            for (int i = 0; i < _components.Count; i++) _components[i].Enter();
            
        }

        public void leaveScene() {
            if (scene == null) return;
            for (int i = 0; i < children.Count; i++) _children[i].leaveScene();
            for (int i = 0; i < _components.Count; i++) _components[i].Leave();
            scene._remove_object(this);
            scene = null;
        }

        internal void OnCollision(Collider other) {
            for (int i = 0; i < _components.Count; i++) _components[i].OnCollision(other);
        }

        public void destroy() {
            parent?.removeChild(this);
            for (int i = 0; i < _children.Count; i++) _children[i].destroy();
            for (int i = 0; i < _components.Count; i++) _components[i].destroy();
            if (scene != null) leaveScene();
            
            _children.Clear();
            _components.Clear();
            _colliders.Clear();
            rigidbody = null;
        }
    }
}
