using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {


    public interface IRenderable {
        void render();
    }

    public abstract class Renderer {

        private static Vertexarray imageEffectQuadVAO;
        private static Buffer<float> imageEffectQuadVBO;
        private static Buffer<uint> imageEffectQuadEBO;

        internal static void RenderScreenQuad() => imageEffectQuadVAO.draw_elements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt);

        private static void InitializeScreenQuad() {
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
            imageEffectQuadVAO = new Vertexarray();
            imageEffectQuadVAO.set_buffer(BufferTarget.ArrayBuffer, imageEffectQuadVBO);
            imageEffectQuadVAO.set_buffer(BufferTarget.ElementArrayBuffer, imageEffectQuadEBO);
            imageEffectQuadVAO.attrib_pointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
        }

        static Renderer() {
            InitializeScreenQuad();
        }

        public ShaderProgram default_shader {
            get => Assets.Shaders["default"];
            set => Assets.Shaders["default"] = value;
        }



        internal virtual void OnWindowResize(int w, int h) { }

        internal abstract void Render();


        protected void renderScene(Scene scene) {

            var s = default_shader;
            s.use();
            scene.main_camera.update_uniformbuffer();
            scene.directionalLight.UpdateUniforms(s);
            scene.render();

        }
    }
}