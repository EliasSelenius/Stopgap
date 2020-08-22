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

        public Prefab(XmlElement xml) {
            bool child(XmlElement x, string c, out XmlElement n) => 
                (n = x[c]) != null;
            vec3 asVec3(string str) {
                var fs = str.Split(' ').Select(x => float.Parse(x));
                return new vec3(fs.ElementAt(0), fs.ElementAt(1), fs.ElementAt(2));
            }
            vec4 asVec4(string str) {
                var fs = str.Split(' ').Select(x => float.Parse(x));
                return new vec4(fs.ElementAt(0), fs.ElementAt(1), fs.ElementAt(2), fs.ElementAt(3));
            }
            Quaternion asQuat(string str) {
                var fs = str.Split(' ').Select(x => float.Parse(x));
                return new Quaternion(fs.ElementAt(0), fs.ElementAt(1), fs.ElementAt(2), fs.ElementAt(3));
            }
            Matrix4 asMat4(string str) {
                var res = new Matrix4(9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9);
                // TODO: make this work !
                var fs = str.Split(' ', '\n', '\r').Where(x => !string.IsNullOrEmpty(x)).Select(x => float.Parse(x)).ToArray();
                for (int i = 0; i < fs.Count(); i++) {
                    res[((i - 1) / 4), (i % 4)] = fs.ElementAt(i);
                }
                return res;
            }

            // Transform
            if (child(xml, "transform", out XmlElement xtrans)) {
                if (child(xtrans, "matrix", out XmlElement xmat)) {
                    transform.matrix = asMat4(xmat.InnerText);
                } else {
                    if (child(xtrans, "position", out XmlElement xpos)) {
                        transform.position = asVec3(xpos.InnerText);
                    }

                    if (child(xtrans, "scale", out XmlElement xscale)) {
                        transform.scale = asVec3(xscale.InnerText);
                    }

                    if (child(xtrans, "rotation", out XmlElement xrot)) {
                        transform.rotation = asQuat(xrot.InnerText);
                    } else if (child(xtrans, "euler", out XmlElement xeuler)) {

                    } else if (child(xtrans, "axis_angle", out XmlElement xaxisa)) {

                    }
                }
            }

            // components
            if (child(xml, "components", out XmlElement xcomps)) {
                foreach (var item in xcomps.ChildNodes) {
                    var xcomp = item as XmlElement;
                    //components.Add(Type.GetType(xcomp.Name), null);
                    var fs = new Dictionary<string, object>();
                    foreach (var item2 in xcomp.ChildNodes) {
                        var xfield = item2 as XmlElement;
                        fs.Add(xfield.Name, null); // TODO: parse value here
                    }
                    components.Add(new Comp(Type.GetType(xcomp.Name), fs));
                    
                }
            }

            // children
            var c = xml.SelectNodes("prefab");
            foreach (var item in c) {
                children.Add(new Prefab(item as XmlElement));
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
