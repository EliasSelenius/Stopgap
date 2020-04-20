using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL;
using SixLabors.Primitives;

namespace Stopgap {
    /// <summary>
    /// A object containing a VertexArrayObject, VertexBufferObject and ElementBufferObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Vao_vbo_ebo<T> where T : struct {

        private readonly VertexArray vao;
        private readonly Buffer<T> vbo;
        private readonly Buffer<uint> ebo;

        public readonly List<T> vertices = new List<T>();
        public readonly List<uint> indices = new List<uint>();

        public Vao_vbo_ebo() {
            vao = new VertexArray();
            vbo = new Buffer<T>();
            ebo = new Buffer<uint>();
            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, vbo);
            vao.SetBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer, ebo);

        }

        public void Apply() {
            vbo.Initialize(vertices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            ebo.Initialize(indices.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }

        public void AttribPointer(int index, int size, OpenTK.Graphics.OpenGL4.VertexAttribPointerType type, bool normalized, int stride, int offset ) => vao.AttribPointer(index, size, type, normalized, stride, offset);

        public void Draw(OpenTK.Graphics.OpenGL4.PrimitiveType type) => vao.DrawElements(type, indices.Count, OpenTK.Graphics.OpenGL4.DrawElementsType.UnsignedInt);
    }
}
