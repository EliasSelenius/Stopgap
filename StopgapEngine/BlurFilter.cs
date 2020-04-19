using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;
using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {
    public static class BlurFilter {

        private static ShaderProgram shader = ShaderProgram.CreateProgram(Shaders.ShaderResources.GaussianBlur, Shaders.ShaderResources.imageVertex);

        private static Framebuffer[] fbo = new Framebuffer[2];
        private static Texture2D[] tex = new Texture2D[2];

        static BlurFilter() {

            shader.SetInt("image", 0);

        }

        internal static void ReinitializeBuffers(int w, int h) {
            
            if (fbo[0] != null) {
                fbo[0].Dispose();
                fbo[1].Dispose();
                tex[0].Dispose();
                tex[1].Dispose();
            }

            Texture2D createAttachment(Framebuffer fbo) {
                var res = new Texture2D(w, h);
                res.Apply(false, PixelInternalFormat.Rgba16f);
                res.Wrap = WrapMode.ClampToEdge;
                res.Filter = Filter.Linear; // NOTE: if we dont specify filter it doesnt work
                fbo.Attach(FramebufferAttachment.ColorAttachment0, res);
                return res;
            }
            
            fbo[0] = new Framebuffer();
            fbo[1] = new Framebuffer();

            tex[0] = createAttachment(fbo[0]);
            tex[1] = createAttachment(fbo[1]);

            fbo[0].Bind();
            Console.WriteLine("reinit of first bloom framebuffer " + Game.window.Width + ", " + Game.window.Height + " status: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
            fbo[1].Bind();
            Console.WriteLine("reinit of second bloom framebuffer " + Game.window.Width + ", " + Game.window.Height + " status: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));


        }

        internal static Texture2D Blur(Texture2D tobeBlured, int amount) {
            shader.Use();
            
            bool even = true;
            for (int i = 0; i < amount; i++) {
                var bufferIndex = even ? 0 : 1;

                fbo[bufferIndex].Bind();
                shader.SetBool("horizontal", even);

                if (i == 0) tobeBlured.Bind(TextureUnit.Texture0);
                else tex[even ? 1 : 0].Bind(TextureUnit.Texture0);
                
                Renderer.RenderScreenQuad();

                even = !even;
            }

            return tex[even ? 0 : 1];
            
            /*
            fbo[0].Bind();
            shader.SetBool("horizontal", true);
            tobeBlured.Bind(TextureUnit.Texture0);
            Renderer.RenderScreenQuad();
            return tex[0];
            */
        }

    }
}
