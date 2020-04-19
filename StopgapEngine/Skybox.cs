using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;

using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Stopgap {
    public class Skybox {

        public static ShaderProgram shader;

        private static VertexArray boxvao;
        private static Buffer<float> vbo;

        private readonly int cubeMapId;

        static Skybox() {

            shader = ShaderProgram.CreateProgram(Shaders.ShaderResources.skyboxFragment, Shaders.ShaderResources.skyboxVertex);
            Assets.Shaders["skybox"] = shader;

            boxvao = new VertexArray();

            vbo = new Buffer<float>();
            vbo.Initialize(new float[] {
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f
            }, BufferUsageHint.StaticDraw);


            boxvao.SetBuffer(BufferTarget.ArrayBuffer, vbo);

            boxvao.AttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

        }

        public Skybox(Image<Rgba32>[] images) {
            cubeMapId = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapId);

            for (int i = 0; i < images.Length; i++) {
                var image = images[i];
                List<byte> pxs = new List<byte>();
                foreach (var item in image.GetPixelSpan()) {
                    pxs.Add(item.R); pxs.Add(item.G); pxs.Add(item.B); pxs.Add(item.A);
                }
                Console.WriteLine("done loading cubemap texture");
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pxs.ToArray());
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)All.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
        }


        internal void Render() {
            shader.Use();
            Camera.MainCamera.UpdateCamUniforms(shader);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapId);
            boxvao.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

    }
}
