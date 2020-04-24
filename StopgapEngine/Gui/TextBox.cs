using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Glow;
using Nums;

namespace Stopgap.Gui {
    public class TextBox : Element {

        public readonly Font font = Font.Arial;
        public float lineSpace = .12f;
        public float font_size = 0.6f;

        public string text { get; private set; } = "";
        private readonly List<Quad> quads = new List<Quad>();

        private VertexArray vao;
        private Buffer<vec4> vbo;
        private Buffer<uint> ebo;


        private vec2 cursor;
        private int cursorIndex = -1;
        private Quad cursor_quad;

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

            vao.AttribPointer(Canvas.rectShader.GetAttribLocation("posuv"), 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);


            Game.window.KeyPress += Window_KeyPress;
            Game.window.KeyDown += Window_KeyDown;
            base.OnFocus += TextBox_OnFocus;
            base.OnUnfocus += TextBox_OnUnfocus;

            this.initCursor();
        }

        private void initCursor() {
            cursor_quad = new Quad(font.GetChar('|'));
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
                v.x *= canvas.aspectRatio;
                
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

            var hs = size_ndc * .5f;
            cursor = new vec2(-hs.x, hs.y);

            uint quadsCount = 0;
            for (int i = 0; i < quads.Count; i++) {

                // place cursor
                if (cursorIndex == i) {
                    addQuadData(cursor_quad, quadsCount, vertices, indices);
                    quadsCount++;
                }
                
                var quad = quads[i];

                addQuadData(quad, quadsCount, vertices, indices);
                quadsCount++;

                cursor.x += quad.glyph.advance * font_size * canvas.aspectRatio;
                var next = i == quads.Count - 1 ? 0 : quads[i + 1].glyph.size.x;
                if (cursor.x + next > hs.x) {
                    cursor.x = -hs.x;
                    cursor.y -= lineSpace * font_size;
                }
            }

            // place cursor
            if (cursorIndex == quads.Count) 
                addQuadData(cursor_quad, quadsCount, vertices, indices);


            vbo.Initialize(vertices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            ebo.Initialize(indices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

            //reInitCursor();
        }

        private IEnumerable<Quad> getQuads(string text) => text.Select(x => new Quad(font.GetChar(x)));
        

        private void addChar(char c) {
            quads.Add(new Quad(font.GetChar(c)));
            text += c;
            cursorIndex++;
        }
        private void subChar(int index) {
            quads.RemoveAt(index);
            text = text.Remove(index);
            cursorIndex--;
        }

        public void InsertText(int index, string text) {
            quads.InsertRange(index, getQuads(text));
            text.Insert(index, text);
            Apply();
        }

        public void AppendText(string text) {
            foreach (var item in text) addChar(item);
            Apply();
        }

        public void RemoveText(int index, int count) {
            quads.RemoveRange(index, count);
            text = text.Remove(index, count);
            Apply();
        }

        public void SetText(string text) {
            quads.Clear();
            this.text = "";
            foreach (var item in text) addChar(item);
            Apply();
        }
        

        private void TextBox_OnFocus(Element obj) {
            if (editable) {
                currently_editing = true;
                cursorIndex = text.Length;
                Apply();
            } 
        }
        private void TextBox_OnUnfocus(Element obj) {
            currently_editing = false;
            cursorIndex = -1;
            Apply();
        }


        protected override void Draw() {

            Canvas.textShader.Use();

            Canvas.textShader.SetVec2("textPosition", pos_ndc);
            Canvas.textShader.SetVec4("color", text_color);
            font.Atlas.Bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            vao.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, indices.Count, OpenTK.Graphics.OpenGL4.DrawElementsType.UnsignedInt);
            Texture2D.Unbind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
        }

        public event Action<TextBox, OpenTK.Input.KeyboardKeyEventArgs> OnInput;

        private void Window_KeyPress(object sender, OpenTK.KeyPressEventArgs e) {
            if (currently_editing) {
                AppendText(e.KeyChar.ToString());
            }
        }

        private void Window_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e) {
            if (currently_editing) {
                OnInput?.Invoke(this, e);
                if (e.Key == OpenTK.Input.Key.BackSpace && quads.Count > 0) {
                    RemoveText(quads.Count - 1, 1); 
                } else {
                    if (e.Key == OpenTK.Input.Key.Left && cursorIndex != 0) {
                        cursorIndex--; Apply();
                    } else if (e.Key == OpenTK.Input.Key.Right && cursorIndex != text.Length) { 
                        cursorIndex++; Apply();
                    }
                }
            }
        }
    }
}