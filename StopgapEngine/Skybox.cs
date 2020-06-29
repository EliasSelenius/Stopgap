using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Glow;
using Nums;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Stopgap {
    public class Skybox {

        private static Vertexarray boxvao;
        private static Buffer<float> vbo;

        public readonly ShaderProgram shader;

        static Skybox() {

            boxvao = new Vertexarray();
            vbo = Glow.Buffer.create(new float[] {
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
            });

            boxvao.set_buffer(BufferTarget.ArrayBuffer, vbo);
            boxvao.attrib_pointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            //boxvao.set_attribute_pointer(0, AttribType.Vec3, vbo, vec3.bytesize);
        }

        public Skybox(ShaderProgram shader) {
            this.shader = shader;
        }

        internal void render() {
            shader.use();
            on_render();
            boxvao.draw_arrays(PrimitiveType.Triangles, 0, 36);
        }

        protected virtual void on_render() { }

    }


    public class CubemapSkybox : Skybox {


        private static ShaderProgram cubemapShader;
        static CubemapSkybox() {
            cubemapShader = ShaderProgram.create(Shaders.ShaderResources.skyboxFragment, Shaders.ShaderResources.skyboxVertex);
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

        protected override void on_render() {
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapId);
        }

    }

}
