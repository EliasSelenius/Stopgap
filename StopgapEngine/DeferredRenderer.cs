using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {

    public class GBuffer {
        public readonly Framebuffer framebuffer;

        private readonly Texture2D positions;
        private readonly Texture2D normals;
        private readonly Texture2D colorSpec;

        public GBuffer(int w, int h) {
            framebuffer = new Framebuffer();

            Texture2D genTex(PixelInternalFormat f) {
                var t = new Texture2D(w, h) {
                    internal_format = f,
                    filter = Filter.Nearest,
                    wrap = WrapMode.ClampToEdge
                };
                t.apply(genMipMap: false);
                return t;
            }

            positions = genTex(PixelInternalFormat.Rgba16f);
            normals = genTex(PixelInternalFormat.Rgba16f);
            colorSpec = genTex(PixelInternalFormat.Rgba);

            framebuffer.attach(FramebufferAttachment.ColorAttachment0, positions);
            framebuffer.attach(FramebufferAttachment.ColorAttachment1, normals);
            framebuffer.attach(FramebufferAttachment.ColorAttachment2, colorSpec);
            framebuffer.attach(FramebufferAttachment.DepthAttachment, new Renderbuffer(RenderbufferStorage.DepthComponent, w, h));

            framebuffer.draw_buffers(DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2);

            if (framebuffer.status != FramebufferStatus.FramebufferComplete)
                throw new Exception("GBuffer failed to initialize");


        }

        public void bind() => framebuffer.bind();

        public void bind_textures() {
            positions.bind(TextureUnit.Texture0);
            normals.bind(TextureUnit.Texture1);
            colorSpec.bind(TextureUnit.Texture2);
        }

        public void resize(int w, int h) => framebuffer.resize(w, h);
    }


    public class DeferredRenderer : Renderer {


        private readonly GBuffer gBuffer;
        private readonly ShaderProgram gBufferShader;

        private readonly ShaderProgram lightningPassShader;

        public DeferredRenderer() {
            gBuffer = new GBuffer(Game.window.Width, Game.window.Height);
            
            gBufferShader = ShaderProgram.create(Shaders.ShaderResources.gBufferFrag, Shaders.ShaderResources.vertex);
            default_shader = gBufferShader;
            
            lightningPassShader = ShaderProgram.create(Shaders.ShaderResources.lightPassFrag, Shaders.ShaderResources.imageVertex);
            lightningPassShader.set_int("gPosition", 0);
            lightningPassShader.set_int("gNormal", 1);
            lightningPassShader.set_int("gAlbedoSpec", 2);

            
        }
        
        internal override void Render() {
            // geometry pass
            gBuffer.bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            gBufferShader.use();

            renderScene(Game.scene);

            int w = Game.window.Width, h = Game.window.Height;
            Framebuffer.copy_region(gBuffer.framebuffer, 0, 0, w, h, Framebuffer.default_buffer, 0, 0, w, h, ClearBufferMask.DepthBufferBit, Filter.Nearest);

            // lightning pass
            Framebuffer.default_buffer.bind();
            //GL.Disable(EnableCap.DepthTest);
            lightningPassShader.use();
            gBuffer.bind_textures();
            Renderer.RenderScreenQuad();


            //Console.WriteLine("GLERROR: " + GL.GetError());

        }

        internal override void OnWindowResize(int w, int h) {
            gBuffer.resize(w, h);
        }

    }
}
