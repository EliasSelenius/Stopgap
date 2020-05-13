using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {
    public class Renderer {

        #region shaders

        public ShaderProgram defaultShader;
        private readonly ShaderProgram normalsRenderingShader;
        private readonly ShaderProgram imageEffectShader;

        #endregion


        #region div props

        public bool renderNormals = false;

        #endregion


        private Framebuffer imageEffectFramebuffer;
        private Texture2D imageEffectColorbuffer;
        private Texture2D imageEffectBrigthnessbuffer;
        private Renderbuffer imageEffectDepthbuffer;

        private VertexArray imageEffectQuadVAO;
        private Buffer<float> imageEffectQuadVBO;
        private Buffer<uint> imageEffectQuadEBO;

        //static Texture2D testtex = new Texture2D(TestData.Images.colorfullNoise);

        public interface IRenderable {
            void Render(ShaderProgram shader);
        }

        private readonly Dictionary<Scene, Dictionary<ShaderProgram, List<IRenderable>>> groups = new Dictionary<Scene, Dictionary<ShaderProgram, List<IRenderable>>>();


        public Renderer() {

            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Lequal);

            // clear color will always be black, as any other value fucks up my bloom. use a skybox to get background color 
            GL.ClearColor(0, 0, 0, 1);


            defaultShader = createShader(Shaders.ShaderResources.fragement, Shaders.ShaderResources.vertex);
            Assets.Shaders["default"] = defaultShader;

            int camUboIndex = GL.GetUniformBlockIndex(defaultShader.gl_handle, "Camera");
            GL.UniformBlockBinding(defaultShader.gl_handle, camUboIndex, 0);

            normalsRenderingShader = createShader(Shaders.ShaderResources.normalsFragment, Shaders.ShaderResources.normalsVertex, Shaders.ShaderResources.normalGeometry);

            //ReinitializeImageBuffers();

            // Shader
            imageEffectShader = createShader(Shaders.ShaderResources.imageFragment, Shaders.ShaderResources.imageVertex);
            imageEffectShader.set_int("colorBuffer", 0);
            imageEffectShader.set_int("brightnessBuffer", 1);

            InitializeScreenQuad();
            
            //BlurFilter.ReinitializeBuffers(Game.window.Width, Game.window.Height);

        }

        internal void ReinitializeImageBuffers() {

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
            imageEffectFramebuffer.attach(FramebufferAttachment.DepthAttachment, imageEffectDepthbuffer);

            // color buffer
            imageEffectColorbuffer = new Texture2D(Game.window.Width, Game.window.Height) {
                filter = Filter.Linear,
                wrap = WrapMode.ClampToEdge
            };
            imageEffectColorbuffer.apply(false, PixelInternalFormat.Rgba16f);
            imageEffectFramebuffer.attach(FramebufferAttachment.ColorAttachment0, imageEffectColorbuffer);

            // brightness buffer
            imageEffectBrigthnessbuffer = new Texture2D(Game.window.Width, Game.window.Height) {
                filter = Filter.Linear,
                wrap = WrapMode.ClampToEdge
            };
            imageEffectBrigthnessbuffer.apply(false, PixelInternalFormat.Rgba16f);
            imageEffectFramebuffer.attach(FramebufferAttachment.ColorAttachment1, imageEffectBrigthnessbuffer);
            
            imageEffectFramebuffer.bind();
            GL.DrawBuffers(2, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            Console.WriteLine("reinit of main framebuffer " + Game.window.Width + ", " + Game.window.Height + " status: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));

        }

        private void InitializeScreenQuad() {
            imageEffectQuadVBO = new Buffer<float>();
            imageEffectQuadVBO.bufferdata(new float[] {
                -1f, -1f,
                 1f, -1f,
                -1f,  1f,
                 1f,  1f
            }, BufferUsageHint.StaticDraw);
            imageEffectQuadEBO = new Buffer<uint>();
            imageEffectQuadEBO.bufferdata(new uint[] {
                0, 1, 2,
                3, 2, 1
            }, BufferUsageHint.StaticDraw);
            imageEffectQuadVAO = new VertexArray();
            imageEffectQuadVAO.set_buffer(BufferTarget.ArrayBuffer, imageEffectQuadVBO);
            imageEffectQuadVAO.set_buffer(BufferTarget.ElementArrayBuffer, imageEffectQuadEBO);
            imageEffectQuadVAO.attrib_pointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
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



        internal void Render() {

            // render scene to framebuffer
            imageEffectFramebuffer.bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.Enable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Lequal);

            renderScene();

            Framebuffer.bind_default(FramebufferTarget.Framebuffer);

            GL.Disable(EnableCap.DepthTest);


            // blur brightness texture
            var b = BlurFilter.Blur(imageEffectBrigthnessbuffer, 10);

            // bind screen buffer
            Framebuffer.bind_default(FramebufferTarget.Framebuffer);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // render image-effects
            imageEffectShader.use();
            imageEffectColorbuffer.bind(TextureUnit.Texture0);
            b.bind(TextureUnit.Texture1);
            RenderScreenQuad();

            Texture.unbind(TextureUnit.Texture0);
            Texture.unbind(TextureUnit.Texture1);


            // gui
            Game.canvas?.Render();
        }

        internal void RenderScreenQuad() {
            imageEffectQuadVAO.draw_elements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt);
        }

        private void renderScene() {
            var so = groups[Game.scene];
            foreach (var item in so) {
                var shader = item.Key;
                shader.use();

                shader.SetFloat("time", Game.time);

                Camera.MainCamera.UpdateCamUniforms(shader);

                Game.scene.directionalLight.UpdateUniforms(shader);

                // render all objects:
                foreach (var obj in item.Value) {
                    obj.Render(shader);
                }

                // render again with normals
                if (renderNormals) {
                    normalsRenderingShader.use();
                    foreach (var obj in item.Value) {
                        obj.Render(normalsRenderingShader);
                    }
                }

            }

            // skybox
            Game.scene.skybox?.Render();
        }

        #region rendring groups controll funcs

        internal void EnsureScene(Scene scene) {
            if (!groups.ContainsKey(scene))
                groups.Add(scene, new Dictionary<ShaderProgram, List<IRenderable>>());
        }

        internal void SetObject(Scene scene, ShaderProgram shader, IRenderable renderable) {
            if (!groups.ContainsKey(scene))
                groups.Add(scene, new Dictionary<ShaderProgram, List<IRenderable>>());
            if (!groups[scene].ContainsKey(shader))
                groups[scene].Add(shader, new List<IRenderable>());

            groups[scene][shader].Add(renderable);
        }

        internal void RemoveObject(Scene scene, ShaderProgram shader, IRenderable renderable) {
            groups[scene][shader].Remove(renderable);
        }

        #endregion

    }
}
