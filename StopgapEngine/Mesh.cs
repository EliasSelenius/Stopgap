using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Nums;
using Glow;
using System.Security.Permissions;

namespace Stopgap {
    [StructLayout(LayoutKind.Sequential)]
    public struct vertex {
        public vec3 pos;
        public vec2 texcoord;
        public vec3 normal;

        public const int bytesize = 2 * vec3.bytesize + vec2.bytesize;

        public vertex(vec3 p, vec2 u, vec3 n) {
            pos = p; texcoord = u; normal = n;
        }

        public vertex lerp(vertex other, float time) {
            return new vertex(pos.lerp(other.pos, time), texcoord.lerp(other.texcoord, time), normal.lerp(other.normal, time));
        }
    }


    public class Mesh {

        private Vertexarray vao;
        private Buffer<vertex> vbo;
        private Buffer<uint> ebo;

        public readonly List<vertex> vertices;
        public readonly List<uint> indices;

        public bool IsInitialized => vao != null;

        public List<Tuple<uint, uint, uint>> TriangleIndices {
            get {
                var res = new List<Tuple<uint, uint, uint>>();
                for (int i = 0; i < indices.Count; i += 3) {
                    res.Add(new Tuple<uint, uint, uint>(indices[i], indices[i + 1], indices[i + 2]));
                }
                return res;
            }
        }

        public Triangle[] Triangles {
            get {
                var tris = TriangleIndices;
                var numTris = tris.Count;
                var res = new Triangle[numTris];
                for (int i = 0; i < numTris; i++) {
                    res[i] = new Triangle(vertices[(int)tris[i].Item1], vertices[(int)tris[i].Item2], vertices[(int)tris[i].Item3],
                        tris[i].Item1, tris[i].Item2, tris[i].Item3);
                }
                return res;
            }
        }

        public struct Triangle {
            public readonly vertex v1, v2, v3;
            public readonly uint i1, i2, i3;


            public Triangle(vertex v1, vertex v2, vertex v3, uint i1, uint i2, uint i3) {
                this.v1 = v1; this.v2 = v2; this.v3 = v3;
                this.i1 = i1; this.i2 = i2; this.i3 = i3;
            }

            public vec3 normal => Mesh.GenNormal(v1, v2, v3);
            public float area => (v2.pos - v1.pos).cross(v3.pos - v1.pos).length / 2f;
            

        }

        public Mesh() {
            vertices = new List<vertex>();
            indices = new List<uint>();
        }

        public Mesh(IEnumerable<vertex> verts, IEnumerable<uint> indc) {
            vertices = verts.ToList();
            indices = indc.ToList();
        }


        public void Init() {

            if (IsInitialized) {
                //throw new Exception("Mesh is already initialized");
                //Console.WriteLine("skipping Mesh init() because it was already initialized");
                return;
            }

            vao = new Vertexarray();

            vbo = new Buffer<vertex>();
            vbo.bufferdata(vertices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

            ebo = new Buffer<uint>();
            ebo.bufferdata(indices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

            vao.set_buffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, vbo);
            vao.set_buffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer, ebo);

            /*
            vao.attrib_pointer(0, 3, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 8, 0);
            vao.attrib_pointer(1, 2, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);
            vao.attrib_pointer(2, 3, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 5);
            */
            const int stride = sizeof(float) * 8;
            vao.attrib_pointer(0, AttribType.Vec3, stride, 0);
            vao.attrib_pointer(1, AttribType.Vec2, stride, sizeof(float) * 3);
            vao.attrib_pointer(2, AttribType.Vec3, stride, sizeof(float) * 5);
        }

        public void Apply() {

            if (!IsInitialized) this.Init();

            vbo.bufferdata(vertices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            ebo.bufferdata(indices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }


        public vertex VertexFurthestAwayFromPivot() {
            vertex res = this.vertices[0];
            for(int i = 1; i < this.vertices.Count; i++) {
                if (res.pos.sqlength < this.vertices[i].pos.sqlength) {
                    res = vertices[i];
                }
            }

            return res;
        }


        public void Render() => Render(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);
        public void Render(OpenTK.Graphics.OpenGL4.PrimitiveType ptype) {
            vao.draw_elements(ptype, indices.Count, OpenTK.Graphics.OpenGL4.DrawElementsType.UnsignedInt);
        }

        public void RenderNormals() {
            // TODO: implement
        }

        public void Mutate(Func<vertex, vertex> func) {
            for (int i = 0; i < vertices.Count; i++) {
                vertices[i] = func(vertices[i]);
            }
        }

        public void AddVertex(vec3 p, vec2 u, vec3 n) => vertices.Add(new vertex(p, u, n));

        public void AddTriangle(uint a, uint b, uint c) {
            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
        }

        public void AddTriangle(int a, int b, int c) => AddTriangle((uint)a, (uint)b, (uint)c);

        public void Subdivide(int subdivisions = 1) {
            for (int i = 0; i < subdivisions; i++)
                subdivide(); 
            //GenNormals();
        }

        private void subdivide() {

            int icount = indices.Count;

            var ind = new uint[icount];
            indices.CopyTo(ind);
            indices.Clear();

            var vex = new vertex[vertices.Count];
            vertices.CopyTo(vex);
            vertices.Clear();

            void _vertex(vertex v) {
                var i = vertices.IndexOf(v);
                if (i == -1) {
                    vertices.Add(v);
                    i = vertices.Count - 1;
                }
                indices.Add((uint)i);
            }

            for (int i = 0; i < icount; i += 3) {

                uint i1 = ind[i],
                     i2 = ind[i + 1],
                     i3 = ind[i + 2];

                vertex v1 = vex[i1],
                       v2 = vex[i2],
                       v3 = vex[i3];

                /*     v3
                        o
                       / \
                  vm2 o---o vm3
                     / \ / \
                    o---o---o
                 v1    vm1    v2  */
                
                vertex vm1 = v1.lerp(v2, .5f),
                       vm2 = v1.lerp(v3, .5f),
                       vm3 = v2.lerp(v3, .5f);

                // triangle 1 (lower left)
                _vertex(v1); _vertex(vm1); _vertex(vm2);

                // triangle 2 (middle)
                _vertex(vm1); _vertex(vm3); _vertex(vm2);

                // triangle 3 (lower right)
                _vertex(vm1); _vertex(v2); _vertex(vm3);

                // triangle 4 (top)
                _vertex(vm2); _vertex(vm3); _vertex(v3);
            }
        }

        
        private vec3 GenNormal(Tuple<uint, uint, uint> face) => GenNormal(face.Item1, face.Item2, face.Item3);
        private vec3 GenNormal(uint a, uint b, uint c) => GenNormal((int)a, (int)b, (int)c);
        private vec3 GenNormal(int a, int b, int c) => GenNormal(vertices[a], vertices[b], vertices[c]);

        private static vec3 GenNormal(vertex a, vertex b, vertex c) {
            var dir1 = a.pos - c.pos;
            var dir2 = b.pos - c.pos;
            return dir1.cross(dir2);
            //return OpenTK.Vector3.Cross(dir1.ToOpenTKVec(), dir2.ToOpenTKVec()).ToNumsVec();
        }

        public void FlipIndices() {
            for (int i = 0; i < indices.Count; i += 3) {
                var t = indices[i];
                indices[i] = indices[i + 2];
                indices[i + 2] = t;
            }
        }



        public void GenNormals() {


            for (int i = 0; i < vertices.Count; i++) {
                var v = vertices[i];
                v.normal = vec3.zero;
                vertices[i] = v;
            }


            var tris = TriangleIndices;
            for (int i = 0; i < tris.Count; i++) {
                var tri = tris[i];
                var v1 = (int)tri.Item1;
                var v2 = (int)tri.Item2;
                var v3 = (int)tri.Item3;

                var no = (vertices[v1].pos - vertices[v3].pos).cross(vertices[v2].pos - vertices[v3].pos);

                var v = vertices[v1];
                v.normal += no;
                vertices[v1] = v;

                v = vertices[v2];
                v.normal += no;
                vertices[v2] = v;

                v = vertices[v3];
                v.normal += no;
                vertices[v3] = v;
            }

            for (int i = 0; i < vertices.Count; i++) {
                var v = vertices[i];
                v.normal = v.normal.normalized();
                vertices[i] = v;
            }


            /*
            var ti = TriangleIndices;

            for (int i = 0; i < vertices.Count; i++) {
                var vert = vertices[i];

                var norms = ti.Where(x => x.Item1 == i || x.Item2 == i || x.Item3 == i)
                    .Select(x => GenNormal(x));

                var avg = norms.Aggregate((x, y) => x + y) / norms.Count();

                vert.normal = avg.normalized;

                vertices[i] = vert;
            }
            /*
            
            /*for (int i = 0; i < vertices.Count; i++) {
                var verts = from o in TriangleIndices
                            where o.Item1 == i || o.Item2 == i || o.Item3 == i
                            select (vertices[(int)o.Item1].pos + vertices[(int)o.Item2].pos + vertices[(int)o.Item3].pos) - vertices[i].pos;

                var vert = vertices[i];
                vert.normal = (vertices[i].pos - MyMath.AvgVec(verts.ToArray())).normalized;
                vertices[i] = vert;
            }*/
        }

        public static Mesh GenIcosphere() {
            const float X = .525731112119133606f;
            const float Z = .850650808352039932f;
            const float N = 0f;
            var verts = new List<vec3> {
                new vec3(-X,N,Z), new vec3(X,N,Z), new vec3(-X,N,-Z), new vec3(X,N,-Z),
                new vec3(N,Z,X), new vec3(N,Z,-X), new vec3(N,-Z,X), new vec3(N,-Z,-X),
                new vec3(Z,X,N), new vec3(-Z,X, N), new vec3(Z,-X,N), new vec3(-Z,-X, N)
            };

            var ind = new uint[] {
                1,4,0 ,4,9,0 ,4,5,9 ,8,5,4 ,1,8,4 ,
                1,10,8 ,10,3,8 ,8,3,5 ,3,2,5 ,3,7,2 ,
                3,10,7 ,10,6,7 ,6,11,7 ,6,0,11 ,6,1,0 ,
                10,1,6 ,11,0,9 ,2,11,9 ,5,2,9 ,11,2,7
            };

            var m = new Mesh(verts.Select(x => new vertex(x, vec2.zero, vec3.unity)), ind);
            m.GenNormals();
            return m;
        }

        public static Mesh GenCube() {
            const float k = 0.5f;
            const float n = -k;
            var verts = new vertex[] {
                // front side
                new vertex(new vec3(n,n,n), new vec2(), new vec3()),
                new vertex(new vec3(k,n,n), new vec2(), new vec3()),
                new vertex(new vec3(n,k,n), new vec2(), new vec3()),
                new vertex(new vec3(k,k,n), new vec2(), new vec3()),

                // back side
                new vertex(new vec3(n,n,k), new vec2(), new vec3()),
                new vertex(new vec3(k,n,k), new vec2(), new vec3()),
                new vertex(new vec3(n,k,k), new vec2(), new vec3()),
                new vertex(new vec3(k,k,k), new vec2(), new vec3()),

                // right side
                new vertex(new vec3(n,n,n), new vec2(), new vec3()),
                new vertex(new vec3(n,k,n), new vec2(), new vec3()),
                new vertex(new vec3(n,n,k), new vec2(), new vec3()),
                new vertex(new vec3(n,k,k), new vec2(), new vec3()),

                // left side
                new vertex(new vec3(k,n,n), new vec2(), new vec3()),
                new vertex(new vec3(k,n,k), new vec2(), new vec3()),
                new vertex(new vec3(k,k,n), new vec2(), new vec3()),
                new vertex(new vec3(k,k,k), new vec2(), new vec3()),

                // top side
                new vertex(new vec3(n,k,n), new vec2(), new vec3()),
                new vertex(new vec3(k,k,n), new vec2(), new vec3()),
                new vertex(new vec3(k,k,k), new vec2(), new vec3()),
                new vertex(new vec3(n,k,k), new vec2(), new vec3()),

                // bottom side
                new vertex(new vec3(n,n,n), new vec2(), new vec3()),
                new vertex(new vec3(k,n,n), new vec2(), new vec3()),
                new vertex(new vec3(k,n,k), new vec2(), new vec3()),
                new vertex(new vec3(n,n,k), new vec2(), new vec3()),

            };
            var ind = new uint[] {
                /*0,1,2,     2,1,3,    // front
                4,6,5,     7,5,6,    // back
                8,9,10,    11,10,9,  // right
                12,13,14,  15,14,13, // left
                16,17,18,  19,16,18, // top
                20,23,22,  21,20,22  // bottom
                */

                2,1,0,     3,1,2,    // front
                5,6,4,     6,5,7,    // back
                10,9,8,    9,10,11,  // right
                14,13,12,  13,14,15, // left
                18,17,16,  18,16,19, // top
                22,23,20,  22,20,21  // bottom

            };
            var m = new Mesh(verts, ind);
            m.GenNormals();
            return m;
        }

        public static Mesh GenPlane(int width, int height) {
            var m = new Mesh();

            float halfWidth = width / 2f,
                  halfHeight = height / 2f;
            
            int i = 0;

            for (int x = 0; x < width; x++) {
                for (int z = 0; z < height; z++) {
                    m.AddVertex(
                        new vec3(x - halfWidth, 0, z - halfHeight), // pos
                        new vec2((float)x / width, (float)z / height), // uv
                        vec3.unity); // normal

                    if (x < width - 1 && z < height - 1) {
                        m.AddTriangle(
                            i,
                            i + 1,
                            i + width + 1);
                        m.AddTriangle(
                            i,
                            i + width + 1,
                            i + width);
                    }


                    i++;
                }
            }


            /*
            for (int x = 1; x <= width + 1; x++) {
                for (int y = 1; y <= height + 1; y++) {
                    m.AddVertex(new vec3(x, y, 0), new vec2((float)x / width, (float)y / height), vec3.unitz);

                    if (x > width || y > height)
                        continue;

                    m.AddTriangle(y * height + x, x * height + width, x + 1);
                }
            }*/

            return m;
        }
        
        public static Mesh GenSphere(int size) {
            var m = new Mesh();

            var vertfunc = new Func<vec3, vec3>[] {
                v => v,
                v => v.zyx * new vec3(1, -1, 1),
                v => v.yzx,
                v => v.yxz * new vec3(-1, 1, 1),
                v => v.zxy,
                v => v.xzy * new vec3(1, 1, -1)
            };

            for (int f = 0; f < 6; f++) {
                int i = (size * size) * f;
                for (int x = 0; x < size; x++) {
                    for (int z = 0; z < size; z++) {

                        vec3 v = new vec3((float)x / (size - 1) - 0.5f, 0.5f, (float)z / (size - 1) - 0.5f).normalized();

                        m.AddVertex(
                            vertfunc[f](v), // pos
                            new vec2((float)x / size, (float)z / size), // uv
                            vec3.unity); // normal


                        if (x < size - 1 && z < size - 1) {
                            m.AddTriangle(
                                i,
                                i + 1,
                                i + size + 1);
                            m.AddTriangle(
                                i,
                                i + size + 1,
                                i + size);
                        }
                        i++;
                    }
                }
            }
            m.GenNormals();
            return m;
        }

        public static Mesh GenQuad() {
            return new Mesh(new vertex[] {
                new vertex((-.5f, -.5f, 0f), (0f, 0f), vec3.unitz),
                new vertex((.5f, -.5f, 0f), (1f, 0f), vec3.unitz),
                new vertex((-.5f, .5f, 0f), (0f, 1f), vec3.unitz),
                new vertex((.5f, .5f, 0f), (1f, 1f), vec3.unitz)
            },
            new uint[] { 
                0, 1, 2,
                1, 3, 2
            });
        }


        public Mesh Copy() {
            return new Mesh(this.vertices, this.indices);
        }

    }


}
