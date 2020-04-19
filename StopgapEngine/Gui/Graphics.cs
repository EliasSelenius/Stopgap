using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;

namespace Stopgap.Gui {
    static class Graphics {

        private readonly static VertexArray vao;
        private static readonly Buffer<float> vbo;
        private static readonly Buffer<uint> ebo;

        static Graphics() {


            vbo = new Buffer<float>();
            vbo.Initialize(new float[] {
                -.5f, -.5f, 1, 0,
                -.5f,  .5f, 0, 0,
                 .5f, -.5f, 1, 1,
                 .5f,  .5f, 0, 1
            }, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);


            ebo = new Buffer<uint>();
            ebo.Initialize(new uint[] {
                0, 2, 1,
                3, 1, 2
            }, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);


            vao = new VertexArray();
            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, vbo);
            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer, ebo);


            vao.AttribPointer(0, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);

        }

        public static void RenderRect() {
            vao.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 6, OpenTK.Graphics.OpenGL4.DrawElementsType.UnsignedInt);
        }
    }
}
