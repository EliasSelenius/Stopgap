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
            shader.set_vec3("dirLight.dir", direction);
            
            shader.set_vec3("dirLight.ambient", ambient);
            shader.set_vec3("dirLight.diffuse", diffuse);
            shader.set_vec3("dirLight.specular", specular);
        }

    }
}
