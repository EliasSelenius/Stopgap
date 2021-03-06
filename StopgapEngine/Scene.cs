﻿using System;
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
using Glow;
using System.Xml;
using OpenTK.Graphics.ES10;

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

        internal void cleanup() {

        }

        internal void loadFromXML(XmlElement xml) {
            GameObject loadGO(XmlElement x) {
                GameObject go;
                if (x.HasAttribute("prefab")) {
                    go = Assets.getPrefab(x.GetAttribute("prefab")).createInstance();
                } else go = new GameObject();

                go.transform.setFromXml(x);

                // components

                // children
                foreach (var item in x.SelectNodes("child")) {
                    go.addChild(loadGO(item as XmlElement));
                }

                return go;
            }

            foreach (var item in xml.SelectNodes("gameobject")) {
                loadGO(item as XmlElement).enterScene(this);
            }

        }

        internal void _remove_object(GameObject o) {
            _gameObjects.Remove(o);
        }
        internal void _add_object(GameObject o) {
            _gameObjects.Add(o);
        }

        
        internal virtual void Update() {
            UpdateEvent?.Invoke();
        }

        internal event Action UpdateEvent;

        public GameObject spawn(params Component[] comps) {
            var g = new GameObject(comps);
            g.enterScene(this);
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
}