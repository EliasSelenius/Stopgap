using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {
    public static class Renderer {

        #region shaders

        public static ShaderProgram defaultShader;
        private readonly static ShaderProgram normalsRenderingShader;
        private readonly static ShaderProgram imageEffectShader;

        #endregion


        #region div props

        public static bool renderNormals = false;

        #endregion


        private static Framebuffer imageEffectFramebuffer;
        private static Texture2D imageEffectColorbuffer;
        private static Texture2D imageEffectBrigthnessbuffer;
        private static Renderbuffer imageEffectDepthbuffer;

        private static VertexArray imageEffectQuadVAO;
        private static Buffer<float> imageEffectQuadVBO;
        private static Buffer<uint> imageEffectQuadEBO;

        //static Texture2D testtex = new Texture2D(TestData.Images.colorfullNoise);

        public interface IRenderable {
            void Render(ShaderProgram shader);
        }

        private static readonly Dictionary<Scene, Dictionary<ShaderProgram, List<IRenderable>>> groups = new Dictionary<Scene, Dictionary<ShaderProgram, List<IRenderable>>>();


        static Renderer() {

            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            //GL.DepthFunc(DepthFunction.Lequal);


            // clear color will always be black, as any other value fucks up my bloom. use a skybox to get background color 
            GL.ClearColor(0, 0, 0, 1);


            defaultShader = createShader(Shaders.ShaderResources.fragement, Shaders.ShaderResources.vertex);
            Assets.Shaders["default"] = defaultShader;

            int camUboIndex = GL.GetUniformBlockIndex(defaultShader.Handle, "Camera");
            GL.UniformBlockBinding(defaultShader.Handle, camUboIndex, 0);

            normalsRenderingShader = createShader(Shaders.ShaderResources.normalsFragment, Shaders.ShaderResources.normalsVertex, Shaders.ShaderResources.normalGeometry);

            //ReinitializeImageBuffers();

            // Shader
            imageEffectShader = createShader(Shaders.ShaderResources.imageFragment, Shaders.ShaderResources.imageVertex);
            imageEffectShader.SetInt("colorBuffer", 0);
            imageEffectShader.SetInt("brightnessBuffer", 1);

            InitializeScreenQuad();
            
            //BlurFilter.ReinitializeBuffers(Game.window.Width, Game.window.Height);

        }

        internal static void ReinitializeImageBuffers() {

            if (imageEffectFramebuffer != null) {
                imageEffectFramebuffer.Dispose();
                imageEffectDepthbuffer.Dispose();
                imageEffectColorbuffer.Dispose();
                imageEffectBrigthnessbuffer.Dispose();
            }

            // image effect stuff:
            imageEffectFramebuffer = new Framebuffer();

            // depth buffer
            imageEffectDepthbuffer = new Renderbuffer(RenderbufferStorage.DepthComponent, Game.window.Width, Game.window.Height);
            imageEffectFramebuffer.Attach(FramebufferAttachment.DepthAttachment, imageEffectDepthbuffer);

            // color buffer
            imageEffectColorbuffer = new Texture2D(Game.window.Width, Game.window.Height) {
                Filter = Filter.Linear,
                Wrap = WrapMode.ClampToEdge
            };
            imageEffectColorbuffer.Apply(false, PixelInternalFormat.Rgba16f);
            imageEffectFramebuffer.Attach(FramebufferAttachment.ColorAttachment0, imageEffectColorbuffer);

            // brightness buffer
            imageEffectBrigthnessbuffer = new Texture2D(Game.window.Width, Game.window.Height) {
                Filter = Filter.Linear,
                Wrap = WrapMode.ClampToEdge
            };
            imageEffectBrigthnessbuffer.Apply(false, PixelInternalFormat.Rgba16f);
            imageEffectFramebuffer.Attach(FramebufferAttachment.ColorAttachment1, imageEffectBrigthnessbuffer);
            
            imageEffectFramebuffer.Bind();
            GL.DrawBuffers(2, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            Console.WriteLine("reinit of main framebuffer " + Game.window.Width + ", " + Game.window.Height + " status: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));

        }

        private static void InitializeScreenQuad() {
            imageEffectQuadVBO = new Buffer<float>();
            imageEffectQuadVBO.Initialize(new float[] {
                -1f, -1f,
                 1f, -1f,
                -1f,  1f,
                 1f,  1f
            }, BufferUsageHint.StaticDraw);
            imageEffectQuadEBO = new Buffer<uint>();
            imageEffectQuadEBO.Initialize(new uint[] {
                0, 1, 2,
                3, 2, 1
            }, BufferUsageHint.StaticDraw);
            imageEffectQuadVAO = new VertexArray();
            imageEffectQuadVAO.SetBuffer(BufferTarget.ArrayBuffer, imageEffectQuadVBO);
            imageEffectQuadVAO.SetBuffer(BufferTarget.ElementArrayBuffer, imageEffectQuadEBO);
            imageEffectQuadVAO.AttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
        }

        private static ShaderProgram createShader(string frag, string vert, string geo = null) {
            var shaders = new List<Shader>() {
                new Shader(ShaderType.FragmentShader, frag),
                new Shader(ShaderType.VertexShader, vert)
            };

            if (geo != null) shaders.Add(new Shader(ShaderType.GeometryShader, geo));

            var res = new ShaderProgram(shaders.ToArray());

            foreach (var item in shaders) item.Dispose();

            return res;
        }



        internal static void Render() {

            // render scene to framebuffer
            imageEffectFramebuffer.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.Enable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Lequal);

            renderScene();

            Framebuffer.BindDefault(FramebufferTarget.Framebuffer);

            GL.Disable(EnableCap.DepthTest);


            // blur brightness texture
            var b = BlurFilter.Blur(imageEffectBrigthnessbuffer, 10);

            // bind screen buffer
            Framebuffer.BindDefault(FramebufferTarget.Framebuffer);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // render image-effects
            imageEffectShader.Use();
            imageEffectColorbuffer.Bind(TextureUnit.Texture0);
            b.Bind(TextureUnit.Texture1);
            RenderScreenQuad();

            Texture2D.Unbind(TextureUnit.Texture0);
            Texture2D.Unbind(TextureUnit.Texture1);


            // gui
            Game.canvas?.Render();
        }

        internal static void RenderScreenQuad() {
            imageEffectQuadVAO.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt);
        }

        private static void renderScene() {
            var so = groups[Game.scene];
            foreach (var item in so) {
                var shader = item.Key;
                shader.Use();

                shader.SetFloat("time", Game.time);

                Camera.MainCamera.UpdateCamUniforms(shader);

                Game.scene.directionalLight.UpdateUniforms(shader);

                // render all objects:
                foreach (var obj in item.Value) {
                    obj.Render(shader);
                }

                // render again with normals
                if (renderNormals) {
                    normalsRenderingShader.Use();
                    foreach (var obj in item.Value) {
                        obj.Render(normalsRenderingShader);
                    }
                }

            }

            // skybox
            //Game.scene.skybox?.Render();
        }

        #region rendring groups controll funcs

        internal static void EnsureScene(Scene scene) {
            if (!groups.ContainsKey(scene))
                groups.Add(scene, new Dictionary<ShaderProgram, List<IRenderable>>());
        }

        internal static void SetObject(Scene scene, ShaderProgram shader, IRenderable renderable) {
            if (!groups.ContainsKey(scene))
                groups.Add(scene, new Dictionary<ShaderProgram, List<IRenderable>>());
            if (!groups[scene].ContainsKey(shader))
                groups[scene].Add(shader, new List<IRenderable>());

            groups[scene][shader].Add(renderable);
        }

        internal static void RemoveObject(Scene scene, ShaderProgram shader, IRenderable renderable) {
            groups[scene][shader].Remove(renderable);
        }

        #endregion

    }
}
