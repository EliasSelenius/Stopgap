using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JsonParser;
using Nums;

namespace Stopgap {
    public class Prefab {

        private readonly Transform transform;
        private readonly Dictionary<Type, object[]> components = new Dictionary<Type, object[]>();
        private readonly List<Prefab> children = new List<Prefab>();

        public Prefab(Transform t) {
            transform = t;
        }

        public static Prefab Load(JObject json) {

            // Transform:
            Transform t;
            if (json.ContainsKey("transform")) {
                t = Transform.FromJson(json["transform"] as JObject);
            } else t = new Transform();

            var res = new Prefab(t);

            // Components
            if (json.ContainsKey("components")) {
                foreach (var item in json["components"] as JArray) {
                    var o = item as JObject;
                    var type = Type.GetType(o["type"] as JString);
                    if (o.ContainsKey("args")) {
                        var jargs = o["args"] as JArray;
                        var args = new List<object>();
                        foreach (var arg in jargs) {
                            var resarg = arg.ToObject();
                            if (arg.IsString) {
                                var str = (string)(arg as JString);
                                if (str.StartsWith("@")) {
                                    // preprocessing directive:
                                    str = str.Substring(1);
                                    resarg = PreProcessingDirective(str);
                                }
                            } else if (arg.IsArray) {

                            }
                            args.Add(resarg);
                        }
                        res.AddComp(type, args.ToArray());
                    } else res.AddComp(type);
                }
            }


            // Children:
            if (json.ContainsKey("children")) {
                foreach (var childjson in json["children"] as JArray) {
                    res.AddChild(Load(childjson as JObject));
                }
            }

            return res;
        }

        private static (Type, object[]) loadComponent(JObject jobj) {

            if (jobj.ContainsKey("args")) {
                var jargs = jobj["args"] as JArray;
                var args = new List<object>();
                foreach (var jarg in jargs) {
                    if (jarg.IsArray) {
                        var a = jarg as JArray;


                        if (toVec2(jarg as JArray, out vec2 v2))
                            args.Add(v2);
                        else if (toVec3(jarg as JArray, out vec3 v3))
                            args.Add(v3);
                        else
                            args.Add(jarg.ToObject());

                    } else {
                        args.Add(jarg.ToObject());
                    }
                }
            }

            return (Type.GetType(jobj["type"] as JString), null);
        }

        private static bool toVec2(JArray a, out Nums.vec2 v) {
            if (a.Count == 2) {
                v = new Nums.vec2(a[0] as JNumber, a[1] as JNumber);
                return true;
            }
            v = vec2.zero;
            return false;
        }
        private static bool toVec3(JArray a, out vec3 v) {
            if (a.Count == 3) {
                v = new vec3(a[0] as JNumber, a[1] as JNumber, a[2] as JNumber);
                return true;
            }
            v = vec3.zero;
            return false;
        }

        private static object PreProcessingDirective(string str) {
            if (str.StartsWith("mesh(")) {
                str = str.Substring(5, str.Length - 6);
                return Assets.GetMesh(str);
            } else if (str.StartsWith("material(")) {
                str = str.Substring(9, str.Length - 10);
                return typeof(BlinnPhongMaterial).GetProperty(str).GetValue(null);
            }

            return null;
        }

        public Prefab AddChild(Prefab child) {
            children.Add(child);
            return this;
        }

        public Prefab AddChildren(params Prefab[] _children) {
            children.AddRange(_children);
            return this;
        }

        public Prefab AddComp(Type type, params object[] args) {
            components.Add(type, args);
            return this;
        }

        public Prefab AddComp<T>(params object[] args) where T : Component {
            components.Add(typeof(T), args);
            return this;
        }

        public GameObject NewInstance() {
            var g = new GameObject();
            g.transform.position = transform.position;
            g.transform.rotation = transform.rotation;
            g.transform.scale = transform.scale;

            foreach (var item in components) {
                g.AddComp(item.Key.GetConstructor(item.Value.Select(x => x.GetType()).ToArray()).Invoke(item.Value) as Component);
            }

            foreach (var item in children) {
                var c = item.NewInstance();
                g.AddChild(c);
            }

            g.Start();
            return g;
        }

    }
}
