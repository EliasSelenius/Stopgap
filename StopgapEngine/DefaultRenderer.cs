using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {
    /// <summary>
    /// forward rendering
    /// </summary>
    public class DefaultRenderer : Renderer {

        #region shaders

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

        
        //static Texture2D testtex = new Texture2D(TestData.Images.colorfullNoise);


        public DefaultRenderer() {

            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Lequal);

            // clear color will always be black, as any other value fucks up my bloom. use a skybox to get background color 
            GL.ClearColor(0, 0, 0, 1);


            //shader = createShader(Shaders.ShaderResources.fragement, Shaders.ShaderResources.vertex);
            shader = createShader(Shaders.ShaderResources.PBRfrag, Shaders.ShaderResources.vertex);

            int camUboIndex = GL.GetUniformBlockIndex(shader.gl_handle, "Camera");
            GL.UniformBlockBinding(shader.gl_handle, camUboIndex, 0);

            normalsRenderingShader = createShader(Shaders.ShaderResources.normalsFragment, Shaders.ShaderResources.normalsVertex, Shaders.ShaderResources.normalGeometry);

            ReinitializeImageBuffers();

            // Shader
            imageEffectShader = createShader(Shaders.ShaderResources.imageFragment, Shaders.ShaderResources.imageVertex);
            imageEffectShader.set_int("colorBuffer", 0);
            imageEffectShader.set_int("brightnessBuffer", 1);

            
            //BlurFilter.ReinitializeBuffers(Game.window.Width, Game.window.Height);

        }

        internal override void OnWindowResize(int w, int h) {
            //ReinitializeImageBuffers();
            imageEffectFramebuffer.resize(w, h);
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
                wrap = WrapMode.ClampToEdge,
                internal_format = PixelInternalFormat.Rgba16f
            };
            imageEffectColorbuffer.apply(genMipMap:false);
            imageEffectFramebuffer.attach(FramebufferAttachment.ColorAttachment0, imageEffectColorbuffer);

            // brightness buffer
            imageEffectBrigthnessbuffer = new Texture2D(Game.window.Width, Game.window.Height) {
                filter = Filter.Linear,
                wrap = WrapMode.ClampToEdge,
                internal_format = PixelInternalFormat.Rgba16f
            };
            imageEffectBrigthnessbuffer.apply(genMipMap: false);
            imageEffectFramebuffer.attach(FramebufferAttachment.ColorAttachment1, imageEffectBrigthnessbuffer);
            
            imageEffectFramebuffer.bind();
            imageEffectFramebuffer.draw_buffers(DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1);
            //GL.DrawBuffers(2, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            Console.WriteLine("reinit of main framebuffer " + Game.window.Width + ", " + Game.window.Height + " status: " + imageEffectFramebuffer.status);
            //var f = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)
        }

        internal override void Render() {

            // render scene to framebuffer
            imageEffectFramebuffer.bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.Enable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Lequal);

            renderScene();

            
            Framebuffer.default_buffer.bind();

            GL.Disable(EnableCap.DepthTest);


            // blur brightness texture
            var b = BlurFilter.Blur(imageEffectBrigthnessbuffer, 10);

            // bind screen buffer
            Framebuffer.default_buffer.bind();
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

        
    }
}
