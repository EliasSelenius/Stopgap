using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {
    internal class ImageeffectBuffers {

        private readonly ShaderProgram image_effect_shader;

        private Framebuffer image_effect_fbo;
        private Texture2D image_effect_colorbuffer;
        private Texture2D image_effect_brigthnessbuffer;
        private Renderbuffer image_effect_depthbuffer;


        public ImageeffectBuffers() {
            image_effect_shader = ShaderProgram.create(Shaders.ShaderResources.imageFragment, Shaders.ShaderResources.imageVertex);
            image_effect_shader.set_int("colorBuffer", 0);
            image_effect_shader.set_int("brightnessBuffer", 1);

            ReinitializeImageBuffers();
        }

        public void bind() {
            image_effect_fbo.bind();
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.DepthTest);
        }

        public void render() {
            var b = BlurFilter.Blur(image_effect_brigthnessbuffer, 10);

            Framebuffer.default_buffer.bind();
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            image_effect_shader.use();
            image_effect_colorbuffer.bind(TextureUnit.Texture0);
            b.bind(TextureUnit.Texture1);
            Renderer.RenderScreenQuad();

            Texture.unbind(TextureUnit.Texture0);
            Texture.unbind(TextureUnit.Texture1);
        }

        public void resize(int w, int h) {
            image_effect_fbo.resize(w, h);
        }

        internal void ReinitializeImageBuffers() {

            if (image_effect_fbo != null) {
                image_effect_fbo.Dispose();
                image_effect_depthbuffer.Dispose();
                image_effect_colorbuffer.Dispose();
                image_effect_brigthnessbuffer.Dispose();
            }

            // image effect stuff:
            image_effect_fbo = new Framebuffer();

            // depth buffer
            image_effect_depthbuffer = new Renderbuffer(RenderbufferStorage.DepthComponent, Game.window.Width, Game.window.Height);
            image_effect_fbo.attach(FramebufferAttachment.DepthAttachment, image_effect_depthbuffer);

            // color buffer
            image_effect_colorbuffer = new Texture2D(Game.window.Width, Game.window.Height) {
                filter = Filter.Linear,
                wrap = WrapMode.ClampToEdge,
                internal_format = PixelInternalFormat.Rgba16f
            };
            image_effect_colorbuffer.apply(genMipMap: false);
            image_effect_fbo.attach(FramebufferAttachment.ColorAttachment0, image_effect_colorbuffer);

            // brightness buffer
            image_effect_brigthnessbuffer = new Texture2D(Game.window.Width, Game.window.Height) {
                filter = Filter.Linear,
                wrap = WrapMode.ClampToEdge,
                internal_format = PixelInternalFormat.Rgba16f
            };
            image_effect_brigthnessbuffer.apply(genMipMap: false);
            image_effect_fbo.attach(FramebufferAttachment.ColorAttachment1, image_effect_brigthnessbuffer);

            image_effect_fbo.draw_buffers(DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1);

            Console.WriteLine("reinit of main framebuffer " + Game.window.Width + ", " + Game.window.Height + " status: " + image_effect_fbo.status);
        }

    }
}
