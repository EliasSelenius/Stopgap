using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using Nums;
using Glow;
using JsonParser;

using OpenTK.Graphics.OpenGL4;
using System.Xml;

namespace Stopgap {
    public static class Assets {

        private static DirectoryInfo CurrentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        private static FileInfo[] GetFiles(string p) {
            try {
                return CurrentDir.GetFiles(p, SearchOption.AllDirectories);
            } catch (DirectoryNotFoundException) {
                return new FileInfo[] { };
            }
        }

        private static readonly Dictionary<string, OBJ> OBJs = new Dictionary<string, OBJ>();
        private static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        private static readonly Dictionary<string, Prefab> prefabs = new Dictionary<string, Prefab>();
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private static readonly Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        private static readonly Dictionary<string, string> shaderSourceFiles = new Dictionary<string, string>();
        private static readonly Dictionary<string, XmlElement> scenes = new Dictionary<string, XmlElement>();
        private static readonly Dictionary<string, SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>> Images = new Dictionary<string, SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>>();



        public static void addMesh(string name, Mesh mesh) {
            meshes.Add(name, mesh);
        }
        public static Mesh getMesh(string name) {
            if (meshes.ContainsKey(name)) {
                return meshes[name];
            }
            var m = OBJs[name + ".obj"].GenMesh();
            m.Init();
            meshes.Add(name, m);
            return m;
        }

        public static Prefab getPrefab(string name) => prefabs[name];
        public static ShaderProgram getShader(string name) => shaders[name];
        public static void setShader(string name, ShaderProgram s) => shaders[name] = s;
        public static Material getMaterial(string name) => materials[name];
        public static Scene loadScene(string name) {
            var r = new Scene();
            r.loadFromXML(scenes[name]);
            return r;
        } 

        internal static void Load() {

            // loading OBJs:
            {
                foreach (var item in GetFiles("data/models/*.obj")) {
                Log("loading OBJ file: " + item.Name);
                OBJs.Add(item.Name, OBJ.LoadFile(item.FullName));
            }
            }

            // load shader source files:
            {
                foreach (var item in GetFiles("data/shaders/*.glsl")) {
                    shaderSourceFiles.Add(item.Name, File.ReadAllText(item.FullName));
                }
                // add some default shader files:
                shaderSourceFiles["defaultVertex"] = Stopgap.Shaders.ShaderResources.vertex;
                shaderSourceFiles["defaultFragment"] = Stopgap.Shaders.ShaderResources.fragement;
                shaderSourceFiles["skyboxVertex"] = Stopgap.Shaders.ShaderResources.skyboxVertex;

                Log("Finnished loading shader source code");
            }

            // process shader source files:
            {
                var rgx = new Regex("#include +\"(?<filename>[a-zA-Z._]+)\"");
                for (int i = 0; i < shaderSourceFiles.Count; i++) {
                    var item = shaderSourceFiles.ElementAt(i);
                    var m = rgx.Matches(item.Value);
                    for (int j = 0; j < m.Count; j++) {
                        var match = m[j];
                        shaderSourceFiles[item.Key] = item.Value.Replace(match.Value, shaderSourceFiles[match.Groups["filename"].Value]);
                    }
                }
                Log("Finnished processing shader source code");
            }

            // load images:
            {
                foreach (var item in GetFiles("data/textures/*.png")) {
                    Images.Add(item.Name, SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(item.FullName));
                }
            }

            // load collada files:
            {
                foreach (var item in GetFiles("data/*.dae")) {
                    Log("Loading " + item.Name);
                    var d = new XmlDocument();
                    d.Load(item.FullName);
                    var c = new Collada(d);
                    var prfs = c.toPrefabs();
                    foreach (var pr in prfs) {
                        prefabs[pr.Key] = pr.Value;
                    }
                }
            }

            // add some default meshes
            {
                var m = Mesh.GenIcosphere();
                m.Subdivide(2);
                m.Mutate(v => {
                    v.pos = v.pos.normalized();
                    return v;
                });
                m.GenNormals();
                addMesh("sphere", m);
                addMesh("cube", Mesh.GenCube());
            }

            // load assets from xml files
            {
                var prefabXml = new List<XmlElement>();

                // foreach xml file...
                foreach (var item in GetFiles("data/*.xml")) {
                    var xmldoc = new XmlDocument(); xmldoc.Load(item.FullName);
                    // foreach asset...
                    foreach (var elm in xmldoc.DocumentElement.ChildNodes) {
                        var xml = elm as XmlElement;

                        // shaders:
                        if (xml.Name.Equals("ShaderProgram")) {
                            var list = new List<Shader>();
                            bool addshader(string tagName, ShaderType type) {
                                if (xml.getChild(tagName, out XmlElement x)) {
                                    list.Add(new Shader(type, shaderSourceFiles[x.InnerText]));
                                    return true;
                                }
                                return false;
                            }

                            if (!addshader("compute", ShaderType.ComputeShader)) {
                                addshader("vert", ShaderType.VertexShader);
                                addshader("frag", ShaderType.FragmentShader);
                                addshader("geom", ShaderType.GeometryShader);
                            }

                            shaders[xml.GetAttribute("name")] = new ShaderProgram(list.ToArray());
                            foreach (var shader in list) shader.Dispose();
                            
                        }

                        // materials:
                        else if (xml.Name.Equals("material")) {
                            var mat = new Material();
                            mat.setFromXml(xml);
                            materials.Add(xml.GetAttribute("name"), mat);
                        }

                        // prefabs:
                        else if (xml.Name.Equals("prefab")) {
                            prefabXml.Add(xml);
                            prefabs.Add(xml.GetAttribute("name"), new Prefab());
                        }

                        // scenes...
                        else if (xml.Name.Equals("scene")) {
                            scenes.Add(xml.GetAttribute("name"), xml);
                        }

                    }
                }

                // prefabs may cross refrence eachother, so they get loaded only after all prefabs have been created
                foreach (var item in prefabXml) {
                    getPrefab(item.GetAttribute("name")).setFromXml(item);
                }
            }

        }

        private static void Log(string msg) {
            Console.WriteLine($"[Assets] {msg}");
        }

    }



    public class Assetpack {
        public interface IAsset {

        }

        private readonly Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();

        public IAsset get_asset(string name) => assets[name];

    }
}
