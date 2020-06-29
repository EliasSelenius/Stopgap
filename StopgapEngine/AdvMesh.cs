using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;
using Glow;
using SixLabors.Primitives;

namespace Stopgap {
    public class AdvMesh {

        // attribute indexes
        const int POSITION = 0;
        const int NORMAL = 1;
        const int TEXCOORD = 2;
        const int COLOR = 3;

        // buffers
        Vertexarray vao;
        Buffer<vec3> positions_buffer;
        Buffer<vec3> normals_buffer;
        Buffer<vec2> texcoords_buffer;
        Buffer<vec4> colors_buffer;
        Buffer<uint> element_buffer;

        // data
        readonly List<vec3> positions;
        readonly List<vec3> normals;
        readonly List<vec2> texcoords;
        readonly List<vec4> colors;
        readonly List<uint> indices;

        bool has_texcoords => texcoords != null;
        bool has_vertex_colors => colors != null;

        List<Entry> entries = new List<Entry>();
        class Entry {
            Material material;
            int count;
            int offset;
            public void render() {
                material.apply();
                Vertexarray.draw_elements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, count, ElementsType.UnsignedInt, offset * sizeof(uint));
            }
        }

        public void add_vertex(vec3 pos, vec3 normal, vec2 texcoord, vec4 color) {
            positions.Add(pos);
        }
        public void add_triangle(Material material, uint a, uint b, uint c) {
            
        }

        public AdvMesh(OpenTK.Graphics.OpenGL4.BufferUsageHint hint) {

            // init buffers:
            positions_buffer = Glow.Buffer.create(hint, positions.ToArray());
            normals_buffer = Glow.Buffer.create(hint, normals.ToArray());
            if (has_texcoords) texcoords_buffer = Glow.Buffer.create(hint, texcoords.ToArray());
            if (has_vertex_colors) colors_buffer = Glow.Buffer.create(hint, colors.ToArray());
            element_buffer = Glow.Buffer.create(hint, indices.ToArray());

            // setup vao:
            vao = new Vertexarray();
            vao.set_attribute_pointer(POSITION, AttribType.Vec3, positions_buffer, vec3.bytesize);
            vao.set_attribute_pointer(NORMAL, AttribType.Vec3, normals_buffer, vec3.bytesize);
            if (has_texcoords) vao.set_attribute_pointer(TEXCOORD, AttribType.Vec2, texcoords_buffer, vec2.bytesize);
            if (has_vertex_colors) vao.set_attribute_pointer(COLOR, AttribType.Vec4, colors_buffer, vec4.bytesize);
            vao.set_elementbuffer(element_buffer);
        }


        public void render() {
            vao.bind();
            foreach (Entry entry in entries) entry.render();
            Vertexarray.unbind();
        }

    }
}
