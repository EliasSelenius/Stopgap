using System;
using System.Collections.Generic;
using System.Linq;
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


        public void AddChar(char c) {
            quads.Add(new Quad(font.GetChar(c)));
        }

        public void Apply() {

            vertices.Clear();
            indices.Clear();

            var hs = this.size * .5f;
            var cursor = new vec4(-hs.x, hs.y, 0, 0);

            var fontSize = new vec4(font_size, font_size, 1, 1);

            for (int i = 0; i < quads.Count; i++) {
                var quad = quads[i];

                vertices.Add(cursor + quad.v1 * fontSize);
                vertices.Add(cursor + quad.v2 * fontSize);
                vertices.Add(cursor + quad.v3 * fontSize);
                vertices.Add(cursor + quad.v4 * fontSize);

                uint n = (uint)i * 4; 

                indices.Add(n + 0); indices.Add(n + 1); indices.Add(n + 2);
                indices.Add(n + 1); indices.Add(n + 3); indices.Add(n + 2);

                cursor.x += quad.glyph.advance * font_size;
                var next = i == quads.Count - 1 ? 0 : quads[i + 1].glyph.size.x;
                if (cursor.x + next > hs.x) {
                    cursor.x = -hs.x;
                    cursor.y -= lineSpace * font_size;
                }

            }

            vbo.Initialize(vertices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            ebo.Initialize(indices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }

        public void AppendText(string text) {
            foreach (var item in text) {
                AddChar(item);
            }
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
            Texture2D.Unbind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            Canvas.guiShader.SetBool("is_text", false);
        }

        static string chars = "   abcdefghijklmnopqrstuvw";

        protected override void OnUpdate() {
            if (currently_editing) {
                var r = (int)((Noise.Random() + 1) * .5f * (chars.Length - 1));
                AppendText("" + chars[r]);
            }
        }
    }
}