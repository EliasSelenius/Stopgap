using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;
using Glow;
using System.Xml;
using System.CodeDom;

namespace Stopgap {

    public class Material {
        public ShaderProgram shader { get; private set; }

        readonly Dictionary<string, propvalue> props = new Dictionary<string, propvalue>();

        public Material() { }
        public Material(ShaderProgram shader) {
            this.shader = shader;
        }

        public void setFromXml(XmlElement xml) {
            shader = Assets.getShader(xml["shader"].InnerText);

            foreach (var item in xml.SelectNodes("prop")) {
                var x = item as XmlElement;
                setProperty(x.GetAttribute("name"), Utils.getValueFromXml(x));
            }
        }

        #region classes
        interface propvalue {
            void set(string name, ShaderProgram s);
        }
        class float_prop : propvalue {
            public float value;
            public void set(string name, ShaderProgram s) => s.SetFloat(name, value);
        }
        class vec2_prop : propvalue {
            public vec2 value;
            public void set(string name, ShaderProgram s) => s.set_vec2(name, value);
        }
        class vec3_prop : propvalue {
            public vec3 value;
            public void set(string name, ShaderProgram s) => s.set_vec3(name, value);
        }
        class vec4_prop : propvalue {
            public vec4 value;
            public void set(string name, ShaderProgram s) => s.set_vec4(name, value);
        }
        #endregion

        public void setProperty(string name, object value) {
            if (value is float) set_float(name, (float)value);
            else if (value is vec2) set_vec2(name, (vec2)value);
            else if (value is vec3) set_vec3(name, (vec3)value);
            else if (value is vec4) set_vec4(name, (vec4)value);
            else throw new Exception(value.GetType().Name + " is not a valid material property");
        }

        public void set_float(string name, float value) => props[name] = new float_prop { value = value };
        public float get_float(string name) => (props[name] as float_prop).value;

        public void set_vec2(string name, vec2 value) => props[name] = new vec2_prop { value = value };
        public vec2 get_vec2(string name) => (props[name] as vec2_prop).value;
        
        public void set_vec3(string name, vec3 value) => props[name] = new vec3_prop { value = value };
        public vec3 get_vec3(string name) => (props[name] as vec3_prop).value;

        public void set_vec4(string name, vec4 value) => props[name] = new vec4_prop { value = value };
        public vec4 get_vec4(string name) => (props[name] as vec4_prop).value;

        internal void apply() {
            foreach (var item in props) {
                item.Value.set(item.Key, shader);
            }
        }
    }

 
    public class PBRMaterial : Material {


        public vec3 albedo {
            get => this.get_vec3("albedo");
            set => this.set_vec3("albedo", value);
        }
        public float metallic {
            get => this.get_float("metallic");
            set => this.set_float("metallic", value);
        }
        public float roughness {
            get => this.get_float("roughness");
            set => this.set_float("roughness", value);
        }
        public vec3 emission {
            get => this.get_vec3("emission");
            set => this.set_vec3("emission", value);
        }

        public PBRMaterial() : base(Game.renderer.default_shader) {
            albedo = 1;
            metallic = 0;
            roughness = 0;
            emission = 0;
        }

        public static readonly PBRMaterial Default = new PBRMaterial {
            albedo = (1, 1, 1),
            metallic = 0,
            roughness = 0.3f
        };
    }

    public class BlinnPhongMaterial {

        public Texture2D diffuseTexture;
        public vec3 diffuseColor;

        public vec3 emission;

        public vec3 specular;
        public float shininess;

        public void Apply(ShaderProgram shader) {

            if (diffuseTexture != null)
                diffuseTexture?.bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            else Texture.unbind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);

            shader.set_int("material.diffuseTexture", 0);
            shader.set_vec3("material.diffuseColor", diffuseColor);

            shader.set_vec3("material.emission", emission);

            shader.set_vec3("material.specular", specular);
            shader.SetFloat("material.shininess", shininess);

            shader.set_vec4("tint", vec4.zero);
        }


        #region Static materials
        public static BlinnPhongMaterial Default => new BlinnPhongMaterial {
            diffuseColor = (.7f, .7f, .7f),
            specular = vec3.one,
            shininess = 1f
        };

        public static BlinnPhongMaterial Greenglow => new BlinnPhongMaterial {
            diffuseColor = new vec3(0.07568f, 0.61424f, 0.07568f),
            specular = new vec3(0.633f, 0.727811f, 0.633f),
            shininess = 0.6f,
            emission = new vec3(0.07568f, 0.61424f, 0.07568f) * 3f
        };
        public static BlinnPhongMaterial Emerald => new BlinnPhongMaterial {
            //ambient = new vec3(0.0215f, 0.1745f, 0.0215f),
            diffuseColor = new vec3(0.07568f, 0.61424f, 0.07568f),
            specular = new vec3(0.633f, 0.727811f, 0.633f),
            shininess = 0.6f
        };
        public static BlinnPhongMaterial Emerald_new => new BlinnPhongMaterial {
            //ambient = new vec3(0.0215f, 0.1745f, 0.0215f),
            //diffuseColor = new vec3(0.07568f, 0.61424f, 0.07568f),
            diffuseColor = new vec3(0.09718f, 0.78874f, 0.09718f),
            specular = new vec3(0.633f, 0.727811f, 0.633f),
            shininess = 0.6f
        };
        public static BlinnPhongMaterial Jade => new BlinnPhongMaterial {
            //ambient = new vec3(0.135f, 0.2225f, 0.1575f),
            diffuseColor = new vec3(0.54f, 0.89f, 0.63f),
            specular = new vec3(0.316228f, 0.316228f, 0.316228f),
            shininess = 0.1f
        };
        public static BlinnPhongMaterial Jade_new => new BlinnPhongMaterial {
            //ambient = new vec3(0.135f, 0.2225f, 0.1575f),
            //diffuseColor = new vec3(0.54f, 0.89f, 0.63f),
            diffuseColor = new vec3(0.675f, 1.1125f, 0.7875f),
            specular = new vec3(0.316228f, 0.316228f, 0.316228f),
            shininess = 0.1f
        };
        public static BlinnPhongMaterial Obsidian => new BlinnPhongMaterial {
            //ambient = new vec3(0.05375f, 0.05f, 0.06625f),
            diffuseColor = new vec3(0.18275f, 0.17f, 0.22525f),
            specular = new vec3(0.332741f, 0.328634f, 0.346435f),
            shininess = 0.3f
        };
        public static BlinnPhongMaterial Obsidian_new => new BlinnPhongMaterial {
            //ambient = new vec3(0.05375f, 0.05f, 0.06625f),
            //diffuseColor = new vec3(0.18275f, 0.17f, 0.22525f),
            diffuseColor = new vec3(0.2365f, 0.22f, 0.2915f),
            specular = new vec3(0.332741f, 0.328634f, 0.346435f),
            shininess = 0.3f
        };
        public static BlinnPhongMaterial Pearl => new BlinnPhongMaterial {
            //ambient = new vec3(0.25f, 0.20725f, 0.20725f),
            diffuseColor = new vec3(1f, 0.829f, 0.829f),
            specular = new vec3(0.296648f, 0.296648f, 0.296648f),
            shininess = 0.088f
        };
        public static BlinnPhongMaterial Ruby => new BlinnPhongMaterial {
            //ambient = new vec3(0.1745f, 0.01175f, 0.01175f),
            diffuseColor = new vec3(0.61424f, 0.04136f, 0.04136f),
            specular = new vec3(0.727811f, 0.626959f, 0.626959f),
            shininess = 0.6f
        };
        public static BlinnPhongMaterial Turquoise => new BlinnPhongMaterial {
            //ambient = new vec3(0.1f, 0.18725f, 0.1745f),
            diffuseColor = new vec3(0.396f, 0.74151f, 0.69102f),
            specular = new vec3(0.297254f, 0.30829f, 0.306678f),
            shininess = 0.1f
        };
        public static BlinnPhongMaterial Brass => new BlinnPhongMaterial {
            //ambient = new vec3(0.329412f, 0.223529f, 0.027451f),
            diffuseColor = new vec3(0.780392f, 0.568627f, 0.113725f),
            specular = new vec3(0.992157f, 0.941176f, 0.807843f),
            shininess = 0.2179487f
        };
        public static BlinnPhongMaterial Bronze => new BlinnPhongMaterial {
            //ambient = new vec3(0.2125f, 0.1275f, 0.054f),
            diffuseColor = new vec3(0.714f, 0.4284f, 0.18144f),
            specular = new vec3(0.393548f, 0.271906f, 0.166721f),
            shininess = 0.2f
        };
        public static BlinnPhongMaterial Chrome => new BlinnPhongMaterial {
            //ambient = new vec3(0.25f, 0.25f, 0.25f),
            diffuseColor = new vec3(0.4f, 0.4f, 0.4f),
            specular = new vec3(0.774597f, 0.774597f, 0.774597f),
            shininess = 0.6f
        };
        public static BlinnPhongMaterial Copper => new BlinnPhongMaterial {
            //ambient = new vec3(0.19125f, 0.0735f, 0.0225f),
            diffuseColor = new vec3(0.7038f, 0.27048f, 0.0828f),
            specular = new vec3(0.256777f, 0.137622f, 0.086014f),
            shininess = 0.1f
        };
        public static BlinnPhongMaterial Gold => new BlinnPhongMaterial {
            //ambient = new vec3(0.24725f, 0.1995f, 0.0745f),
            diffuseColor = new vec3(0.75164f, 0.60648f, 0.22648f),
            specular = new vec3(0.628281f, 0.555802f, 0.366065f),
            shininess = 0.4f
        };
        public static BlinnPhongMaterial Silver => new BlinnPhongMaterial {
            //ambient = new vec3(0.19225f, 0.19225f, 0.19225f),
            diffuseColor = new vec3(0.50754f, 0.50754f, 0.50754f),
            specular = new vec3(0.508273f, 0.508273f, 0.508273f),
            shininess = 0.4f
        };
        public static BlinnPhongMaterial BlackPlastic => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0f, 0f),
            diffuseColor = new vec3(0.01f, 0.01f, 0.01f),
            specular = new vec3(0.5f, 0.5f, 0.5f),
            shininess = 0.25f
        };
        public static BlinnPhongMaterial CyanPlastic => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0.1f, 0.06f),
            diffuseColor = new vec3(0f, 0.5098039f, 0.5098039f),
            specular = new vec3(0.5019608f, 0.5019608f, 0.5019608f),
            shininess = 0.25f
        };
        public static BlinnPhongMaterial GreenPlastic => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0f, 0f),
            diffuseColor = new vec3(0.1f, 0.35f, 0.1f),
            specular = new vec3(0.45f, 0.55f, 0.45f),
            shininess = 0.25f
        };
        public static BlinnPhongMaterial RedPlastic => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0f, 0f),
            diffuseColor = new vec3(0.5f, 0f, 0f),
            specular = new vec3(0.7f, 0.6f, 0.6f),
            shininess = 0.25f
        };
        public static BlinnPhongMaterial WhitePlastic => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0f, 0f),
            diffuseColor = new vec3(0.55f, 0.55f, 0.55f),
            specular = new vec3(0.7f, 0.7f, 0.7f),
            shininess = 0.25f
        };
        public static BlinnPhongMaterial YellowPlastic => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0f, 0f),
            diffuseColor = new vec3(0.5f, 0.5f, 0f),
            specular = new vec3(0.6f, 0.6f, 0.5f),
            shininess = 0.25f
        };
        public static BlinnPhongMaterial BlackRubber => new BlinnPhongMaterial {
            //ambient = new vec3(0.02f, 0.02f, 0.02f),
            diffuseColor = new vec3(0.01f, 0.01f, 0.01f),
            specular = new vec3(0.4f, 0.4f, 0.4f),
            shininess = 0.078125f
        };
        public static BlinnPhongMaterial CyanRubber => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0.05f, 0.05f),
            diffuseColor = new vec3(0.4f, 0.5f, 0.5f),
            specular = new vec3(0.04f, 0.7f, 0.7f),
            shininess = 0.078125f
        };
        public static BlinnPhongMaterial GreenRubber => new BlinnPhongMaterial {
            //ambient = new vec3(0f, 0.05f, 0f),
            diffuseColor = new vec3(0.4f, 0.5f, 0.4f),
            specular = new vec3(0.04f, 0.7f, 0.04f),
            shininess = 0.078125f
        };
        public static BlinnPhongMaterial RedRubber => new BlinnPhongMaterial {
            //ambient = new vec3(0.05f, 0f, 0f),
            diffuseColor = new vec3(0.5f, 0.4f, 0.4f),
            specular = new vec3(0.7f, 0.04f, 0.04f),
            shininess = 0.078125f
        };
        public static BlinnPhongMaterial WhiteRubber => new BlinnPhongMaterial {
            //ambient = new vec3(0.05f, 0.05f, 0.05f),
            diffuseColor = new vec3(0.5f, 0.5f, 0.5f),
            specular = new vec3(0.7f, 0.7f, 0.7f),
            shininess = 0.078125f
        };
        public static BlinnPhongMaterial YellowRubber => new BlinnPhongMaterial {
            //ambient = new vec3(0.05f, 0.05f, 0f),
            diffuseColor = new vec3(0.5f, 0.5f, 0.4f),
            specular = new vec3(0.7f, 0.7f, 0.04f),
            shininess = 0.078125f
        };
        
        #endregion

    }
}
