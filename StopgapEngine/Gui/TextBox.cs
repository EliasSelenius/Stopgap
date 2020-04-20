using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Glow;
using Nums;

namespace Stopgap.Gui {
    public class TextBox : Element {

        public readonly Font font = Font.Arial;
        public readonly float lineSpace = .12f;
        public readonly float font_size = .6f;

        private readonly List<Quad> quads = new List<Quad>();

        private VertexArray vao;
        private Buffer<vec4> vbo;
        private Buffer<uint> ebo;

        private Vao_vbo_ebo<vec4> vve_cursor;

        private vec2 cursor;

        private List<vec4> vertices = new List<vec4>();
        private List<uint> indices = new List<uint>();

        public vec4 text_color = vec4.one;
        public bool editable = false;
        private bool currently_editing = false;

        public TextBox() {

            vao = new VertexArray();

            vbo = new Buffer<vec4>();
            ebo = new Buffer<uint>();

            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, vbo);
            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer, ebo);

            vao.AttribPointer(Canvas.guiShader.GetAttribLocation("posuv"), 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);


            Game.window.KeyPress += Window_KeyPress;

            vve_cursor = new Vao_vbo_ebo<vec4>();
            vve_cursor.AttribPointer(Canvas.guiShader.GetAttribLocation("posuv"), 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            addQuadData(new Quad(font.GetChar('_')), 0, vve_cursor.vertices, vve_cursor.indices);
            vve_cursor.Apply();


        }

        

        public class Quad {

            //    v3  v4
            //     o--o
            //     |\ |
            //     | \|
            //     o--o
            //    v1  v2

            public readonly Font.Glyph glyph;
            public readonly vec4 v1, v2, v3, v4;

            public Quad(Font.Glyph ch) {

                glyph = ch;
                
                var p = ch.pos;
                var n = ch.size;
                var pn = p + n;

                // UVs
                v1.zw = (pn.y, p.x);
                v2.zw = pn.yx;
                v3.zw = p.yx;
                v4.zw = (p.y, pn.x);

                // positions
                v1.xy = (glyph.offset + (0, glyph.size.y)) * new vec2(1, -1);
                v2.xy = (glyph.offset + glyph.size) * new vec2(1, -1);
                v3.xy = glyph.offset * new vec2(1, -1);
                v4.xy = (glyph.offset + (glyph.size.x, 0)) * new vec2(1, -1);
            }
        }

        private void addQuadData(Quad quad, uint quadIndex, List<vec4> vertices, List<uint> indices) {
            void _addv(vec4 v) {
                v.xy *= font_size;
                v.x *= aspect;
                v.xy += cursor;
                vertices.Add(v);
            }

            _addv(quad.v1);
            _addv(quad.v2);
            _addv(quad.v3);
            _addv(quad.v4);

            uint n = quadIndex * 4;

            indices.Add(n + 0); indices.Add(n + 1); indices.Add(n + 2);
            indices.Add(n + 1); indices.Add(n + 3); indices.Add(n + 2);
        }

        public void Apply() {

            vertices.Clear();
            indices.Clear();

            var hs = vec2.one * .5f;
            cursor = new vec2(-hs.x, hs.y);

            for (int i = 0; i < quads.Count; i++) {
                var quad = quads[i];

                addQuadData(quad, (uint)i, vertices, indices);

                cursor.x += quad.glyph.advance * aspect * font_size;
                var next = i == quads.Count - 1 ? 0 : quads[i + 1].glyph.size.x;
                if (cursor.x + next > hs.x) {
                    cursor.x = -hs.x;
                    cursor.y -= lineSpace * font_size;
                }
            }

            vbo.Initialize(vertices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            ebo.Initialize(indices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }

        private IEnumerable<Quad> getQuads(string text) => text.Select(x => new Quad(font.GetChar(x)));
        

        private void addChar(char c) {
            quads.Add(new Quad(font.GetChar(c)));
        }

        public void InsertText(int index, string text) {
            quads.InsertRange(index, getQuads(text));
            Apply();
        }

        public void AppendText(string text) {
            foreach (var item in text) addChar(item);
            Apply();
        }

        
        protected override void OnFocus() {
            if (editable) currently_editing = true;
        }

        protected override void OnLostFocus() {
            currently_editing = false;
        }

        protected override void OnRender() {
            Canvas.guiShader.SetBool("is_text", true);
            Canvas.guiShader.SetVec4("color", text_color);
            font.Atlas.Bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            vao.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, indices.Count, OpenTK.Graphics.OpenGL4.DrawElementsType.UnsignedInt);

            // draw cursor:
            if (currently_editing) {
                vec2 c = cursor;
                c.x *= canvas.aspectRatio;
                vec2 p = pos_ndc + c, s = size_ndc;
                Canvas.guiShader.SetVec4("rectTransform", p.x, p.y, s.x, s.y);
                vve_cursor.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);
            }

            Texture2D.Unbind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            Canvas.guiShader.SetBool("is_text", false);
        }

        private void Window_KeyPress(object sender, OpenTK.KeyPressEventArgs e) {
            if (currently_editing) {
                AppendText(e.KeyChar.ToString());
            }
        }
    }
}