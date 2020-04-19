using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;

namespace Stopgap {
    public class MeshRenderer : Component, Renderer.IRenderable {

        public readonly ShaderProgram shader;

        public readonly Mesh mesh;
        public Material material;
        
        public OpenTK.Graphics.OpenGL4.PrimitiveType primitiveType;



        public MeshRenderer(Mesh m, Material mat) : this(m, mat, Renderer.defaultShader, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles) { }

        public MeshRenderer(Mesh m, Material mat, ShaderProgram shader) : this(m, mat, shader, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles) { }

        public MeshRenderer(Mesh m, Material mat, ShaderProgram shader, OpenTK.Graphics.OpenGL4.PrimitiveType ptype) {
            mesh = m;
            material = mat;
            this.shader = shader;
            primitiveType = ptype;
        }

        public override void OnEnter() {
            Renderer.SetObject(gameObject.scene, shader, this);
        }

        public override void OnLeave() {
            Renderer.RemoveObject(gameObject.scene, shader, this);
        }

        public void Render(ShaderProgram shader) {
            shader.SetMat4("obj_transform", gameObject.ModelMatrix);
            material.Apply(shader);

            mesh.Render(primitiveType);
        }

        public override void Start() {
            mesh.Init();
        }
    }
}
