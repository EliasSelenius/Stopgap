using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using OpenTK.Graphics.OpenGL4;

namespace Stopgap {


    public interface IRenderable {
        void Render(ShaderProgram shader);
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

        public ShaderProgram shader {
            get => Assets.Shaders["default"];
            set => Assets.Shaders["default"] = value;
        }

        #region rendring groups controll funcs

        protected readonly Dictionary<Scene, Dictionary<ShaderProgram, List<IRenderable>>> groups = new Dictionary<Scene, Dictionary<ShaderProgram, List<IRenderable>>>();


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


        internal virtual void OnWindowResize(int w, int h) { }

        internal abstract void Render();


        public static ShaderProgram createShader(string frag, string vert, string geo = null) {
            var shaders = new List<Shader>() {
                new Shader(ShaderType.FragmentShader, frag),
                new Shader(ShaderType.VertexShader, vert)
            };

            if (geo != null) shaders.Add(new Shader(ShaderType.GeometryShader, geo));

            var res = new ShaderProgram(shaders.ToArray());

            foreach (var item in shaders) item.Dispose();

            return res;
        }

        protected void renderScene() {
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
                /*if (renderNormals) {
                    normalsRenderingShader.use();
                    foreach (var obj in item.Value) {
                        obj.Render(normalsRenderingShader);
                    }
                }*/

            }

            // skybox
            Game.scene.skybox?.Render();
        }
    }
}
