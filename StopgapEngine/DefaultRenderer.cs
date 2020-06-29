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

        #endregion


        #region div props

        public bool renderNormals = false;

        #endregion


        ImageeffectBuffers image_effect_buffers;

        public DefaultRenderer() {

            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Lequal);

            // clear color will always be black, as any other value fucks up my bloom. use a skybox to get background color 
            GL.ClearColor(0, 0, 0, 1);


            //shader = createShader(Shaders.ShaderResources.fragement, Shaders.ShaderResources.vertex);
            default_shader = ShaderProgram.create(Shaders.ShaderResources.PBRfrag, Shaders.ShaderResources.vertex);

            default_shader.uniformblock_binding("Camera", 0);

            normalsRenderingShader = ShaderProgram.create(Shaders.ShaderResources.normalsFragment, Shaders.ShaderResources.normalsVertex, Shaders.ShaderResources.normalGeometry);


            image_effect_buffers = new ImageeffectBuffers();
        }

        internal override void OnWindowResize(int w, int h) {
            image_effect_buffers.resize(w, h);
        }

        

        internal override void Render() {

            // bind these buffers to be rendererd on to
            image_effect_buffers.bind();
            renderScene(); // render the scene to it
            image_effect_buffers.render(); // finaly render the buffers on to the screen


            // gui
            Game.canvas?.Render();
        }

        
    }
}
