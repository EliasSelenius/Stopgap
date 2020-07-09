using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;
using Glow;

namespace Stopgap {
    public class AdvMesh {

        // attribute indexes
        const int POSITION = 0;
        const int TEXCOORD = 1;
        const int NORMAL = 2;

        // buffers
        Vertexarray vao;
        Buffer<vertex> vertex_buffer;
        Buffer<uint> element_buffer;

        // data
        public readonly List<vertex> vertices = new List<vertex>();
        readonly List<uint> indices = new List<uint>();
        readonly Dictionary<Material, int> entries = new Dictionary<Material, int>();

        public void add_vertex(vec3 pos, vec2 texcoord, vec3 normal) {
            vertices.Add(new vertex(pos, texcoord, normal));
        }
        public void add_triangles(Material material, params uint[] indices) {
            if (entries.ContainsKey(material)) 
                entries[material] += indices.Length;
            else entries[material] = indices.Length;

            int offset = 0;
            foreach (var item in entries) {
                if (item.Key == material) {
                    this.indices.InsertRange(offset, indices);
                    break;
                }
                offset += item.Value;
            }
        }


        public AdvMesh() {

            // init buffers:
            vertex_buffer = Glow.Buffer.create(this.vertices.ToArray());
            element_buffer = Glow.Buffer.create(this.indices.ToArray());

            // setup vao:
            vao = new Vertexarray();
            vao.set_buffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, vertex_buffer);
            vao.attrib_pointer(POSITION, AttribType.Vec3, vertex.bytesize, 0);
            vao.attrib_pointer(TEXCOORD, AttribType.Vec2, vertex.bytesize, vec3.bytesize);
            vao.attrib_pointer(NORMAL, AttribType.Vec3, vertex.bytesize, vec3.bytesize + vec2.bytesize);
            vao.set_elementbuffer(element_buffer);
        }

        public void bufferdata(OpenTK.Graphics.OpenGL4.BufferUsageHint hint = OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw) {
            vertex_buffer.bufferdata(vertices.ToArray(), hint);
            element_buffer.bufferdata(indices.ToArray(), hint);
        }

        public void render() {
            vao.bind();
            int offset = 0;
            foreach (var item in entries) {
                item.Key.apply();
                Vertexarray.draw_elements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, item.Value, ElementsType.UnsignedInt, offset * sizeof(uint));
                offset += item.Value;
            }
            Vertexarray.unbind();
        }

    }
}
