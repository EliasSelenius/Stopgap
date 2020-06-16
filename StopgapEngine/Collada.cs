using Nums;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stopgap {
    public class Collada {

        private readonly List<Geometry> geometries = new List<Geometry>();
        private readonly List<Material> materials = new List<Material>();

        public Collada(XmlDocument doc) {

            var root = doc["COLLADA"];

            // geometry
            var lib_geom = root["library_geometries"];
            foreach (var item in lib_geom.GetElementsByTagName("geometry")) {
                geometries.Add(new Geometry((XmlElement)item));
            }


            // materials
            var lib_materials = root["library_materials"];
            foreach (var item in lib_materials.GetElementsByTagName("material")) {
                materials.Add(new Material((XmlElement)item));
            }
        }

        public AdvMesh ToGameObject() {
            var meshes = geometries.Select(x => x.genMesh());
            return meshes.ElementAt(0);
        }

        public class Geometry {
            string id, name;

            Dictionary<string, Source> sources = new Dictionary<string, Source>();
            Triangles[] triangles;

            class Source {
                float[] float_array;
                int stride;
                public Source(float[] data, int s) {
                    float_array = data; stride = s;
                }
                public T[] as_vector_array<T>() where T : vec, new() {
                    T[] res = new T[float_array.Length / stride];
                    for (int i = 0; i < res.Length; i++) {
                        res[i] = new T();
                        for (int j = 0; j < stride; j++) {
                            res[i][j] = float_array[i * stride + j];
                        }
                    }
                    return res;
                }
            }

            class Triangles {
                public string material_name;
                // semantic, source
                public Dictionary<string, string> inputs = new Dictionary<string, string>();
                public int[] indices;

                public Triangles(XmlElement xml) {
                    material_name = xml.GetAttribute("material");

                    var input_nodes = xml.GetElementsByTagName("input");
                    foreach (var input in input_nodes) {
                        var i = input as XmlElement;
                        inputs.Add(i.GetAttribute("semantic"), i.GetAttribute("source").TrimStart('#'));
                    }


                    indices = xml["p"].InnerText.Split(' ').Select(x => int.Parse(x)).ToArray();
                }
            }

            internal Geometry(XmlElement xml) {
                id = xml.GetAttribute("id");
                name = xml.GetAttribute("name");

                xml = xml["mesh"];

                // sources
                var srcnodes = xml.GetElementsByTagName("source");
                foreach (var item in srcnodes) {
                    var x = item as XmlElement;
                    sources.Add(x.GetAttribute("id"), source(x));
                }
                // vertices
                var vs = xml["vertices"];
                sources.Add(vs.GetAttribute("id"), sources[vs["input"].GetAttribute("source").TrimStart('#')]);

                // triangles
                triangles = xml.GetElementsByTagName("triangles").Cast<XmlElement>().Select(x => new Triangles(x)).ToArray();
            }

            static Source source(XmlElement xml) {
                var float_array = xml["float_array"].InnerText.Split(' ').Select(x => float.Parse(x)).ToArray();

                var stride = int.Parse(xml["technique_common"]["accessor"].GetAttribute("stride"));

                return new Source(float_array, stride);
            }

            public AdvMesh genMesh() {

                // it seems the collada file format supports for different triangle collections in one mesh to have different vertex attributes, our engine does not support this however, so assert that all triangles inputs are equal

                var inputs = triangles.First().inputs;
                if (!triangles.All(t => t.inputs.Count == inputs.Count && !t.inputs.Except(inputs).Any())) throw new Exception("inconsistent inputs in collada file");

                vec3[] positions;
                

                return null;// new AdvMesh();
            }
        }

        public class Material {
            string id, name;

            internal Material(XmlElement xml) {
                id = xml.GetAttribute("id");
                name = xml.GetAttribute("name");
            }
        }


    }
}