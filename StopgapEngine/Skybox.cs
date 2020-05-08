using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Glow;

using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Stopgap {
    public class Skybox {

        private static VertexArray boxvao;
        private static Buffer<float> vbo;

        public readonly ShaderProgram shader;

        static Skybox() {

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

        public Skybox(ShaderProgram shader) {
            this.shader = shader;
        }

        internal void Render() {
            shader.Use();
            Camera.MainCamera.UpdateCamUniforms(shader);
            onRender();
            boxvao.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        protected virtual void onRender() { }

    }


    public class CubemapSkybox : Skybox {


        private static ShaderProgram cubemapShader;
        static CubemapSkybox() {
            cubemapShader = ShaderProgram.CreateProgram(Shaders.ShaderResources.skyboxFragment, Shaders.ShaderResources.skyboxVertex);
            Assets.Shaders["cubemap_skybox"] = cubemapShader;
        }

        private readonly int cubeMapId;

        public CubemapSkybox(Image<Rgba32>[] images) : base(cubemapShader) {
            cubeMapId = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapId);

            for (int i = 0; i < images.Length; i++) {
                var image = images[i];
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.GetPixelSpan().ToArray());
                Console.WriteLine("done loading cubemap texture");
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)All.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
        }

        protected override void onRender() {
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapId);
        }

    }

}
