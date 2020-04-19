using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;

namespace Stopgap {
    public class OBJ {
        public readonly List<vec3> Vertices = new List<vec3>();
        public readonly List<vec3> Normals = new List<vec3>();
        public readonly List<vec2> UVs = new List<vec2>();

        public readonly List<Face> Faces = new List<Face>();

        public class VertexIndices {
            public readonly int PositionIndex;
            public readonly int UVIndex;
            public readonly int NormalIndex;

            public VertexIndices(int pos, int uv, int normal) {
                PositionIndex = pos;
                UVIndex = uv;
                NormalIndex = normal;
            }
        }

        public class Face {
            public readonly VertexIndices[] vertices;
            public Face(IEnumerable<VertexIndices> verts) {
                vertices = verts.ToArray();
            }
        }

        private vec2 GetUv(int i) {
            if (i == 0) {
                return vec2.zero;
            }
            return UVs[i - 1];
        }

        private vec3 GetPos(int i) {
            if (i == 0) {
                return vec3.zero;
            }
            return Vertices[i - 1];
        }

        private vec3 GetNormal(int i) {
            if (i == 0) {
                return vec3.zero;
            }
            return Normals[i - 1];
        }

        public Mesh GenMesh() {
            var meshVerts = new List<Vertex>();
            var meshindc = new List<uint>();

            uint tr = 0;
            for (int f = 0; f < Faces.Count; f++) {
                var face = Faces[f];
                for (uint v = 0; v < face.vertices.Length; v++) {
                    var vert = face.vertices[v];
                    meshVerts.Add(new Vertex(GetPos(vert.PositionIndex), GetUv(vert.UVIndex), GetNormal(vert.NormalIndex)));
                    meshindc.Add(tr + v);
                }
                tr += 3;
            }

            return new Mesh(meshVerts, meshindc);
        }

        #region parsing

        public static OBJ LoadFile(string filepath) => Load(System.IO.File.ReadAllLines(filepath));

        public static OBJ Load(string[] source) {
            OBJ res = new OBJ();

            for (int i = 0; i < source.Length; i++) {
                var line = source[i];

                // suppress if line is a comment
                if (line.StartsWith("#")) {
                    continue;
                }

                if (LineMatch(line, "v", out string p)) {
                    // vertex position

                    if (ParseFloats(p, 3, out float[] o)) {
                        res.Vertices.Add(new vec3(o[0], o[1], o[2]));
                    } else {
                        //Console.WriteLine("(OBJ parser) problem parsing vertex position, at line " + (i + 1));
                        Log("problem parsing vertex position", i + 1);
                    }

                } else if (LineMatch(line, "vt", out p)) {
                    // vertex UV

                    if (ParseFloats(p, 2, out float[] o)) {
                        res.UVs.Add(new vec2(o[0], o[1]));
                    } else {
                        //Console.WriteLine("(OBJ parser) problem parsing vertex texture cordinates, at line " + (i + 1));
                        Log("problem parsing vertex texture cordinates", i + 1);
                    }

                } else if (LineMatch(line, "vn", out p)) {
                    // vertex normal

                    if (ParseFloats(p, 3, out float[] o)) {
                        res.Normals.Add(new vec3(o[0], o[1], o[2]));
                    } else {
                        //Console.WriteLine("(OBJ parser) problem parsing vertex normal vector, at line " + (i + 1));
                        Log("problem parsing vertex normal vector", i + 1);
                    }

                } else if (LineMatch(line, "f", out p)) {
                    // face

                    if (ParseFace(p, out OBJ.Face f)) {
                        res.Faces.Add(f);
                    } else {
                        Log("problem parsing face", i + 1);
                    }

                } else if (LineMatch(line, "o", out p)) {
                    Log("o is not supported", i + 1);
                } else if (LineMatch(line, "mtllib", out p)) {
                    Log("mtllib is not supported", i + 1);
                } else if (LineMatch(line, "usemtl", out p)) {
                    Log("usemtl is not supported", i + 1);
                } else if (LineMatch(line, "s", out p)) {
                    Log("s is not supported", i + 1);
                } else {
                    // unrecognized
                    //Console.WriteLine("(OBJ parser) did not recognize: " + line + " at line " + (i + 1));
                    Log("did not recognize: " + line, i + 1);
                }

                //Console.WriteLine(p);

            }

            return res;
        }



        private static void Log(string msg, int line) {
            Console.WriteLine($"[OBJ Parser at line {line}] {msg}");
        }

        private static bool ParseFace(string str, out OBJ.Face face) {
            var list = str.Split(' ');
            var verts = new List<OBJ.VertexIndices>();
            var notfail = true;
            for (int i = 0; i < list.Length && notfail; i++) {
                notfail = ParseObjVertex(list[i], out OBJ.VertexIndices v);
                verts.Add(v);
            }
            face = new OBJ.Face(verts);
            return notfail;
        }

        private static bool ParseObjVertex(string str, out OBJ.VertexIndices objvertex) {
            try {
                var nums = str.Split('/').Select(x => x == string.Empty ? 0 : int.Parse(x));

                var pos = nums.ElementAt(0);
                var uv = nums.ElementAt(1);
                var normal = 0;
                if (nums.Count() == 3) {
                    normal = nums.ElementAt(2);
                }

                objvertex = new OBJ.VertexIndices(pos, uv, normal);
                return true;

            } catch (Exception) {
                objvertex = null;
                return false;
            }
        }

        private static bool ParseFloats(string str, int length, out float[] result) {
            var list = str.Split(' ');
            if (list.Length != length) {
                result = null;
                return false;
            }
            result = new float[length];
            for (int i = 0; i < result.Length; i++) {
                result[i] = float.Parse(list[i]);
            }
            return true;
        }

        private static bool LineMatch(string line, string start, out string parameters) {
            var s = start + " ";
            if (line.StartsWith(s)) {
                parameters = line.Substring(s.Length);
                return true;
            }
            parameters = null;
            return false;
        }

        #endregion

    }
}
