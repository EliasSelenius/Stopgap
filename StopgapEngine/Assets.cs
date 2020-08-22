﻿using System;
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

        public static readonly Dictionary<string, OBJ> OBJs = new Dictionary<string, OBJ>();


        public static readonly Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
        public static readonly Dictionary<string, string> ShaderSourceFiles = new Dictionary<string, string>();
        
        public static readonly Dictionary<string, Prefab> Prefabs = new Dictionary<string, Prefab>();
        
        public static readonly Dictionary<string, SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>> Images = new Dictionary<string, SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>>();


        private static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        public static void AddMesh(string name, Mesh mesh) {
            meshes.Add(name, mesh);
        }
        public static Mesh GetMesh(string name) {
            if (meshes.ContainsKey(name)) {
                return meshes[name];
            }
            var m = OBJs[name + ".obj"].GenMesh();
            m.Init();
            meshes.Add(name, m);
            return m;
        }

        public static Prefab getPrefab(string name) {
            return Prefabs[name];
        }

        internal static void Load() {
            LoadObjs();


            LoadShaderSources();
            // add some default shader assets:
            ShaderSourceFiles["defaultVertex"] = Stopgap.Shaders.ShaderResources.vertex;
            ShaderSourceFiles["defaultFragment"] = Stopgap.Shaders.ShaderResources.fragement;
            ShaderSourceFiles["skyboxVertex"] = Stopgap.Shaders.ShaderResources.skyboxVertex;

            ProcessShaders();
            CompileShadersFromConfig();


            LoadImages();

            loadColladaFiles();


            // add some default meshes
            {
                var m = Mesh.GenIcosphere();
                m.Subdivide(2);
                m.Mutate(v => {
                    v.pos = v.pos.normalized();
                    return v;
                });
                m.GenNormals();
                AddMesh("sphere", m);
                AddMesh("cube", Mesh.GenCube());
            }


            // load assets from xml files
            loadXmlFiles();   

        }


        private static void loadColladaFiles() {
            foreach (var item in GetFiles("data/*.dae")) {
                Log("Loading " + item.Name);
                var d = new XmlDocument();
                d.Load(item.FullName);
                var c = new Collada(d);
                var prfs = c.toPrefabs();
                foreach (var pr in prfs) {
                    Prefabs[pr.Key] = pr.Value;
                }
            }
        }


        private static void loadXmlFiles() {
            foreach (var item in GetFiles("data/*.xml")) {
                var xmldoc = new XmlDocument();
                xmldoc.Load(item.FullName);
                foreach (var elm in xmldoc.DocumentElement.ChildNodes) {
                    var x = elm as XmlElement;
                    if (x.Name.Equals("prefab")) Prefabs.Add(x.GetAttribute("name"), new Prefab(x));
                }
            }
        }



        private static void LoadImages() {
            foreach (var item in GetFiles("data/textures/*.png")) {
                Images.Add(item.Name, SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(item.FullName));
            }
        }

        private static void CompileShadersFromConfig() {
            var configFileName = "data/shaders/ShadersConfig.json";
            if (!File.Exists(configFileName))
                return;

            var j = Json.FromFile(configFileName) as JArray;
            foreach (var item in j) {
                var sc = item as JObject;

                Shader fs, vs;
                if (sc.ContainsKey("source")) {
                    fs = new Shader(ShaderType.FragmentShader, ShaderSourceFiles[sc["source"] as JString].Replace("ShaderType", "SHADER_FRAG"));
                    vs = new Shader(ShaderType.VertexShader, ShaderSourceFiles[sc["source"] as JString].Replace("ShaderType", "SHADER_VERT"));
                } else {
                    fs = new Shader(ShaderType.FragmentShader, ShaderSourceFiles[sc["fragsrc"] as JString]);
                    vs = new Shader(ShaderType.VertexShader, ShaderSourceFiles[sc["vertsrc"] as JString]);
                }

                Shaders.Add(sc["name"] as JString, new ShaderProgram(fs, vs));
                fs.Dispose();
                vs.Dispose();
            }
            Log("Finnished compiling shaders");
        }

        private static void LoadShaderSources() {
            foreach (var item in GetFiles("data/shaders/*.glsl")) {
                ShaderSourceFiles.Add(item.Name, File.ReadAllText(item.FullName));
            }
            Log("Finnished loading shader source code");
        }

        private static void ProcessShaders() {
            var rgx = new Regex("#include +\"(?<filename>[a-zA-Z._]+)\"");
            for (int i = 0; i < ShaderSourceFiles.Count; i++) {
                var item = ShaderSourceFiles.ElementAt(i);
                var m = rgx.Matches(item.Value);
                for (int j = 0; j < m.Count; j++) {
                    var match = m[j];
                    ShaderSourceFiles[item.Key] = item.Value.Replace(match.Value, ShaderSourceFiles[match.Groups["filename"].Value]);
                }
            }
            Log("Finnished processing shader source code");
        }


        private static void LoadObjs() {
            foreach (var item in GetFiles("data/models/*.obj")) {
                Log("loading OBJ file: " + item.Name);
                OBJs.Add(item.Name, OBJ.LoadFile(item.FullName));
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
