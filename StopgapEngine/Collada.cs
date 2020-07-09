using Nums;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
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
                geometries.Add(new Geometry(this, (XmlElement)item));
            }


            // materials
            var lib_materials = root["library_materials"];
            foreach (var item in lib_materials.GetElementsByTagName("material")) {
                materials.Add(new Material((XmlElement)item));
            }
        }

        public AdvMesh to_gameobject() {
            var meshes = geometries.Select(x => x.genMesh());


            return meshes.ElementAt(9);
        }

        public class Geometry {
            readonly Collada collada;
            readonly string id, name;

            Dictionary<string, Source> sources = new Dictionary<string, Source>();
            TriangleCollection[] triangles;

            class Source {
                vec[] cache = null;
                float[] float_array;
                int stride;
                public Source(XmlElement xml) {
                    float_array = xml["float_array"].InnerText.Split(' ').Select(x => float.Parse(x)).ToArray();
                    stride = int.Parse(xml["technique_common"]["accessor"].GetAttribute("stride"));
                }
                public T[] as_vector_array<T>() where T : vec, new() {
                    if (cache != null) return cache as T[];
                    
                    T[] res = new T[float_array.Length / stride];
                    for (int i = 0; i < res.Length; i++) {
                        res[i] = new T();
                        for (int j = 0; j < stride; j++) {
                            res[i][j] = float_array[i * stride + j];
                        }
                    }

                    cache = res as vec[];
                    return res;
                }
            }
            class TriangleCollection {

                private const string VERTEX = "VERTEX";
                private const string NORMAL = "NORMAL";
                private const string TEXCOORD = "TEXCOORD";
                private const string COLOR = "COLOR";

                Geometry geometry;

                int num_inputs;
                public readonly (Source source, int offset) pos_input;
                public readonly (Source source, int offset) normal_input;
                public readonly (Source source, int offset) texcoord_input;
                

                public string material_name;

                public vertexindices[] indices;
                public class vertexindices {
                    public int pos_index;
                    public int normal_index;
                    public int texcoord_index;
                }

                public TriangleCollection(Geometry gom, XmlElement xml) {
                    geometry = gom;
                    material_name = xml.GetAttribute("material");

                    var input_nodes = xml.GetElementsByTagName("input");
                    num_inputs = input_nodes.Count;
                    foreach (var input in input_nodes) {
                        var i = input as XmlElement;
                        var src = geometry.sources.TryGetValue(i.GetAttribute("source").TrimStart('#'), out Source s) ? s : null;
                        var ofs = int.Parse(i.GetAttribute("offset"));
                        var semantic = i.GetAttribute("semantic");
                        if (semantic.Equals(VERTEX)) {
                            pos_input.source = src;
                            pos_input.offset = ofs;
                        } else if (semantic.Equals(NORMAL)) {
                            normal_input.source = src;
                            normal_input.offset = ofs;
                        } else if (semantic.Equals(TEXCOORD)) {
                            texcoord_input.source = src;
                            texcoord_input.offset = ofs;
                        }

                    }


                    var ints = xml["p"].InnerText.Split(' ').Select(x => int.Parse(x)).ToArray();
                    var indices = new List<vertexindices>();
                    for (int i = 0; i < ints.Length / num_inputs; i++) {
                        var index = i * num_inputs;
                        int get_input_index((Source source, int offset) input) {
                            if (input.source == null) return -1;
                            else return ints[index + input.offset];
                        }

                        indices.Add(new vertexindices {
                            pos_index = get_input_index(pos_input),
                            normal_index = get_input_index(normal_input),
                            texcoord_index = get_input_index(texcoord_input)
                        });

                    }


                    this.indices = indices.ToArray();
                }
            }

            internal Geometry(Collada c, XmlElement xml) {
                collada = c;
                id = xml.GetAttribute("id");
                name = xml.GetAttribute("name");

                xml = xml["mesh"];

                // sources
                var srcnodes = xml.GetElementsByTagName("source");
                foreach (var item in srcnodes) {
                    var x = item as XmlElement;
                    sources.Add(x.GetAttribute("id"), new Source(x));
                }
                // vertices
                var vs = xml["vertices"];
                sources.Add(vs.GetAttribute("id"), sources[vs["input"].GetAttribute("source").TrimStart('#')]);

                // triangles
                triangles = xml.GetElementsByTagName("triangles").Cast<XmlElement>().Select(x => new TriangleCollection(this, x)).ToArray();
            }

            public AdvMesh genMesh() {
                var mesh = new AdvMesh();

                int add_vertex(vertex v) {
                    if (mesh.vertices.Contains(v)) return mesh.vertices.IndexOf(v);
                    mesh.vertices.Add(v);
                    return mesh.vertices.Count - 1;
                }


                foreach (var trcollection in triangles) {
                    var indices = new uint[trcollection.indices.Length];
                    vec3[] positions = trcollection.pos_input.source.as_vector_array<vec3>();
                    vec3[] normals = trcollection.normal_input.source.as_vector_array<vec3>();
                    vec2[] texcoords = trcollection.texcoord_input.source?.as_vector_array<vec2>();

                    for (int i = 0; i < trcollection.indices.Length; i++) {
                        var vi = trcollection.indices[i];
                        indices[i] = (uint)add_vertex(new vertex(
                            positions[vi.pos_index], 
                            texcoords?[vi.texcoord_index] ?? vec2.zero, 
                            normals[vi.normal_index]
                            ));
                    }

                    mesh.add_triangles(collada.get_material(trcollection.material_name).to_material(), indices);
                }
                mesh.bufferdata();

                return mesh;
            }
        }

        public Material get_material(string id) => materials.Find(x => x.id.Equals(id));

        public class Material {
            public readonly string id, name;

            internal Material(XmlElement xml) {
                id = xml.GetAttribute("id");
                name = xml.GetAttribute("name");
            }

            public Stopgap.Material to_material() {
                return new PBRMaterial {
                    albedo = (math.range(0, 1), math.range(0, 1), math.range(0, 1))
                };
            }
        }


    }
}