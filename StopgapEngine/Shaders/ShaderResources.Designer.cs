﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Stopgap.Shaders {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ShaderResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ShaderResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Stopgap.Shaders.ShaderResources", typeof(ShaderResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///
        ///in VS_OUT {
        ///	vec2 uv;
        ///	vec3 normal;
        ///	vec3 fragPos;
        ///} vInput;
        ///
        ///
        ///
        /////======DATASTRUCTURES========
        ///
        ///struct PointLight {
        ///	vec3 pos;
        ///	
        ///	vec3 ambient;
        ///	vec3 diffuse;
        ///	vec3 specular;
        ///
        ///	float constant;
        ///	float linear;
        ///	float quadratic;
        ///};
        ///
        ///struct DirLight {
        ///	vec3 dir;
        ///	
        ///	vec3 ambient;
        ///	vec3 diffuse;
        ///	vec3 specular;
        ///};
        ///
        ///struct Material {
        ///
        ///	sampler2D diffuseTexture;
        ///	vec3 diffuseColor;
        ///
        ///	vec3 emission;
        ///
        ///	vec3 specular;
        ///	float shininess;
        ///};
        ///
        /////============= [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string fragement {
            get {
                return ResourceManager.GetString("fragement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///out vec4 FragColor;
        ///  
        ///in vec2 uv;
        ///
        ///uniform sampler2D image;
        ///  
        ///uniform bool horizontal;
        ///uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
        ///
        ///void main()
        ///{             
        ///    vec2 tex_offset = 1.0 / textureSize(image, 0); // gets size of single texel
        ///    vec3 result = texture(image, uv).rgb * weight[0]; // current fragment&apos;s contribution
        ///    if(horizontal)
        ///    {
        ///        for(int i = 1; i &lt; 5; ++i)
        ///        {
        ///            result += texture( [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GaussianBlur {
            get {
                return ResourceManager.GetString("GaussianBlur", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout(location = 0) out vec3 gPosition;
        ///layout(location = 1) out vec3 gNormal;
        ///layout(location = 2) out vec4 gAlbedoSpec;
        ///
        ///in VS_OUT {
        ///	vec2 uv;
        ///	vec3 normal;
        ///	vec3 fragPos;
        ///};
        ///
        ///
        ///void main() {
        ///	gPosition = fragPos;
        ///	gNormal = normal; // normalize?
        ///	gAlbedoSpec.rgb = vec3(1.0);
        ///	gAlbedoSpec.a = 1.0;
        ///}.
        /// </summary>
        internal static string gBufferFrag {
            get {
                return ResourceManager.GetString("gBufferFrag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///uniform sampler2D colorBuffer;
        ///uniform sampler2D brightnessBuffer;
        ///uniform float exposure = 1.0;
        ///
        ///in vec2 uv;
        ///
        ///out vec4 FragColor;
        ///
        ///void main() {
        ///	vec4 color = texture(colorBuffer, uv);
        ///	vec4 bColor = texture(brightnessBuffer, uv);
        ///	
        ///	color += bColor;
        ///
        ///	// tone mapping (HDR -&gt; LDR):
        ///	//color = color / (color + vec4(1.0));
        ///	color = vec4(1.0) - exp(-color * exposure);
        ///
        ///	// gamma correction:
        ///	color.rgb = pow(color.rgb, vec3(1.0/2.2));
        ///
        ///	FragColor = color;
        ///}.
        /// </summary>
        internal static string imageFragment {
            get {
                return ResourceManager.GetString("imageFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout (location = 0) in vec2 vPos;
        ///
        ///out vec2 uv;
        ///
        ///void main() {
        ///	uv = (vPos + vec2(1.0)) * 0.5;
        ///	gl_Position = vec4(vPos, 0.0, 1.0);
        ///}.
        /// </summary>
        internal static string imageVertex {
            get {
                return ResourceManager.GetString("imageVertex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///out vec4 FragColor;
        ///  
        ///in vec2 uv;
        ///
        ///
        ///uniform sampler2D gPosition;
        ///uniform sampler2D gNormal;
        ///uniform sampler2D gAlbedoSpec;
        ///
        ///
        ///void main()
        ///{             
        ///    /*
        ///    // retrieve data from G-buffer
        ///    vec3 FragPos = texture(gPosition, TexCoords).rgb;
        ///    vec3 Normal = texture(gNormal, TexCoords).rgb;
        ///    vec3 Albedo = texture(gAlbedoSpec, TexCoords).rgb;
        ///    float Specular = texture(gAlbedoSpec, TexCoords).a;
        ///    
        ///    // then calculate lighting as usual
        ///    vec3 lighting [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string lightPassFrag {
            get {
                return ResourceManager.GetString("lightPassFrag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout (triangles) in;
        ///layout (line_strip, max_vertices = 6) out;
        ///
        ///in VS_OUT {
        ///    vec3 normal;
        ///} gs_in[];
        ///
        ///const float MAGNITUDE = 0.4;
        ///
        ///void GenerateLine(int index)
        ///{
        ///    gl_Position = gl_in[index].gl_Position;
        ///    EmitVertex();
        ///    gl_Position = gl_in[index].gl_Position + vec4(gs_in[index].normal, 0.0) * MAGNITUDE;
        ///    EmitVertex();
        ///    EndPrimitive();
        ///}
        ///
        ///void main()
        ///{
        ///    GenerateLine(0); // first vertex normal
        ///    GenerateLine(1); // second vertex normal
        ///    G [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string normalGeometry {
            get {
                return ResourceManager.GetString("normalGeometry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///out vec4 FragColor;
        ///
        ///void main()
        ///{
        ///    FragColor = vec4(1.0, 1.0, 0.0, 1.0);
        ///} .
        /// </summary>
        internal static string normalsFragment {
            get {
                return ResourceManager.GetString("normalsFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout (std140) uniform Camera {
        ///	mat4 projection;
        ///	mat4 view;
        ///};
        ///
        ///
        ///uniform mat4 obj_transform;
        ///
        ///layout (location = 0) in vec3 v_pos;
        ///layout (location = 1) in vec2 v_uv;
        ///layout (location = 2) in vec3 v_normal;
        ///
        ///out VS_OUT {
        ///	vec3 normal;
        ///};
        ///
        ///void main()
        ///{
        ///    gl_Position = projection * view * obj_transform * vec4(v_pos, 1.0); 
        ///    mat3 normalMatrix = mat3(transpose(inverse(view * obj_transform)));
        ///    normal = normalize(vec3(projection * vec4(normalMatrix * v_normal, [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string normalsVertex {
            get {
                return ResourceManager.GetString("normalsVertex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///out vec4 FragColor;
        ///
        ///
        ///in VS_OUT {
        ///	vec2 uv;
        ///	vec3 normal;
        ///	vec3 fragPos;
        ///} vInput;
        ///
        ///
        ///
        ///// material parameters
        ///uniform vec3  albedo;
        ///uniform float metallic;
        ///uniform float roughness;
        ///
        ///
        ///uniform vec3 cam_pos;
        ///
        ///const float PI = 3.14159265359;
        ///  
        ///float DistributionGGX(vec3 N, vec3 H, float roughness);
        ///float GeometrySchlickGGX(float NdotV, float roughness);
        ///float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
        ///vec3 fresnelSchlick(float cosTheta, vec3 F0);
        ///
        ///vec3 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PBRfrag {
            get {
                return ResourceManager.GetString("PBRfrag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 440 core
        ///
        ///uniform sampler2D font_atlas;
        ///
        ///uniform vec4 color;
        ///
        ///out vec4 out_color;
        ///in vec2 uv;
        ///
        ///void main() {
        ///	vec4 c = color;
        ///
        ///	float r = 0.0;
        ///
        ///	if (uv.y &lt; r &amp;&amp; uv.x &lt; r &amp;&amp; distance(uv, vec2(r)) &gt; r) discard;
        ///	if (uv.y &gt; 1.0-r &amp;&amp; uv.x &gt; 1.0-r &amp;&amp; distance(uv, vec2(1.0-r)) &gt; r) discard;
        ///
        ///	if (uv.y &gt; 1.0-r &amp;&amp; uv.x &lt; r &amp;&amp; distance(uv, vec2(r, 1.0-r)) &gt; r) discard;
        ///	if (uv.y &lt; r &amp;&amp; uv.x &gt; 1.0-r &amp;&amp; distance(uv, vec2(1.0-r, r)) &gt; r) discard;
        ///
        ///	out_color = c;
        ///}.
        /// </summary>
        internal static string rectElementFragment {
            get {
                return ResourceManager.GetString("rectElementFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 440 core
        ///
        ///// vec2 position, vec2 size
        ///uniform vec4 rectTransform;
        ///
        ///layout (location = 0) in vec4 posuv;
        ///
        ///out vec2 uv;
        ///
        ///void main() {
        ///	
        ///	vec2 vp = posuv.xy;
        ///
        ///	vec2 pos = rectTransform.xy + vp * rectTransform.zw;
        ///
        ///	gl_Position = vec4(pos, 0.0, 1.0);
        ///	uv = posuv.zw;
        ///}.
        /// </summary>
        internal static string rectElementVertex {
            get {
                return ResourceManager.GetString("rectElementVertex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core 
        ///
        ///uniform samplerCube skybox;
        ///
        ///in vec3 texCoords;
        ///
        ///layout (location = 0) out vec4 FragColor;
        ///layout (location = 1) out vec4 BrightColor;
        ///
        ///void main() {
        ///	FragColor = texture(skybox, texCoords) * 0.3;
        ///	BrightColor = vec4(0.0);
        ///}.
        /// </summary>
        internal static string skyboxFragment {
            get {
                return ResourceManager.GetString("skyboxFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core 
        ///
        ///layout (std140) uniform Camera {
        ///	mat4 projection;
        ///	mat4 view;
        ///};
        ///
        ///layout (location = 0) in vec3 v_pos;
        ///
        ///out vec3 texCoords;
        ///
        ///void main() {
        ///	vec4 pos = projection * mat4(mat3(view)) * vec4(v_pos, 1.0);
        ///
        ///	gl_Position = pos.xyww;
        ///	texCoords = v_pos;
        ///}
        ///.
        /// </summary>
        internal static string skyboxVertex {
            get {
                return ResourceManager.GetString("skyboxVertex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 440 core
        ///
        ///uniform sampler2D font_atlas;
        ///
        ///uniform vec4 color;
        ///
        ///out vec4 out_color;
        ///in vec2 uv;
        ///
        ///void main() {
        ///	vec4 c = color;
        ///
        ///    c *= texture(font_atlas, uv);
        ///    if (c.a &lt; 0.5) discard;		
        ///
        ///	out_color = c;
        ///}.
        /// </summary>
        internal static string textFragment {
            get {
                return ResourceManager.GetString("textFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 440 core
        ///
        ///// vec2 position, vec2 size
        ///uniform vec2 textPosition;
        ///
        ///layout (location = 0) in vec4 posuv;
        ///
        ///out vec2 uv;
        ///
        ///void main() {
        ///	
        ///	vec2 vp = posuv.xy;
        ///
        ///	vec2 pos = textPosition + vp;
        ///
        ///	gl_Position = vec4(pos, 0.0, 1.0);
        ///	uv = posuv.zw;
        ///}.
        /// </summary>
        internal static string textVertex {
            get {
                return ResourceManager.GetString("textVertex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///
        ///layout (std140) uniform Camera {
        ///	mat4 projection;
        ///	mat4 view;
        ///};
        ///
        ///
        ///uniform mat4 obj_transform;
        ///uniform float time;
        ///
        ///layout (location = 0) in vec3 v_pos;
        ///layout (location = 1) in vec2 v_uv;
        ///layout (location = 2) in vec3 v_normal;
        ///
        ///
        ///out VS_OUT {
        ///	vec2 uv;
        ///	vec3 normal;
        ///	vec3 fragPos;
        ///};
        ///
        ///void main() {
        ///	uv = v_uv;
        ///	normal = normalize((obj_transform * vec4(v_normal, 0.0)).xyz);
        ///	fragPos = (obj_transform * vec4(v_pos, 1.0)).xyz;
        ///
        ///	gl_Position = projection * view * [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string vertex {
            get {
                return ResourceManager.GetString("vertex", resourceCulture);
            }
        }
    }
}
