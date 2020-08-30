using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using JsonParser;
using Nums;
using OpenTK;

namespace Stopgap {
    public class Prefab {

        public readonly Transform transform = new Transform();
        //private readonly Dictionary<Type, object[]> components = new Dictionary<Type, object[]>();
        private readonly List<Comp> components = new List<Comp>();
        public readonly List<Prefab> children = new List<Prefab>();


        class Comp {
            public readonly Type type;
            public readonly Dictionary<string, object> fields = new Dictionary<string, object>();

            public Comp(Type t, Dictionary<string, object> fs) {
                type = t;
                fields = fs;
            }

            public Component create() {
                var res = type.GetConstructor(Array.Empty<Type>()).Invoke(null) as Component;
                foreach (var item in fields) {
                    type.GetField(item.Key).SetValue(res, item.Value);
                }
                return res;
            }
        }

        public void addComp<T>(Dictionary<string, object> fields) where T : Component {
            components.Add(new Comp(typeof(T), fields));
        }

        public Prefab() { }


        public void setFromXml(XmlElement xml) {

            transform.setFromXml(xml);

            // components
            if (xml.getChild("components", out XmlElement xcomps)) {
                // foreach component
                foreach (var item in xcomps.ChildNodes) {
                    var xcomp = item as XmlElement;
                    var fs = new Dictionary<string, object>();
                    foreach (var item2 in xcomp.SelectNodes("prop")) {
                        var xfield = item2 as XmlElement;
                        fs.Add(xfield.GetAttribute("name"), Utils.getValueFromXml(xfield));
                    }
                    components.Add(new Comp(Type.GetType(xcomp.Name), fs));

                }
            }

            // children
            var c = xml.SelectNodes("child");
            foreach (var item in c) {
                var p = new Prefab();
                p.setFromXml((XmlElement)item);
                children.Add(p);
            }
        }

        public Prefab(GameObject gameobject) {
            transform.set(gameobject.transform);

            // components


            // children
            foreach (var item in gameobject.children) {
                children.Add(new Prefab(item));
            }
        }

        public XmlElement asXml() {
            throw new NotImplementedException();
        }

        public GameObject createInstance() {
            var g = new GameObject();
            g.transform.position = transform.position;
            g.transform.rotation = transform.rotation;
            g.transform.scale = transform.scale;

            foreach (var item in components) {


                //g.AddComp(item.Key.GetConstructor(item.Value?.Select(x => x.GetType())?.ToArray() ?? Array.Empty<Type>()).Invoke(item.Value) as Component);

                g.AddComp(item.create());

            }

            foreach (var item in children) {
                var c = item.createInstance();
                g.AddChild(c);
            }

            g.Start();
            return g;
        }

    }
}
