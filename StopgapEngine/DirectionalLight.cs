using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using Nums;

namespace Stopgap {
    public class DirectionalLight {


        public vec3 direction = (1,-1,-1);

        public vec3 ambient = (.2f, .2f, .2f);
        public vec3 diffuse = (.5f, .5f, .5f);
        public vec3 specular = vec3.one;

        internal void UpdateUniforms(ShaderProgram shader) {
            shader.SetVec3("dirLight.dir", direction);
            
            shader.SetVec3("dirLight.ambient", ambient);
            shader.SetVec3("dirLight.diffuse", diffuse);
            shader.SetVec3("dirLight.specular", specular);
        }

    }
}
