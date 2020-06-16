using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;

namespace Stopgap {
    public class MeshRenderer : Component, IRenderable {

        public readonly ShaderProgram shader;

        public readonly Mesh mesh;
        public IMaterial material;
        
        public OpenTK.Graphics.OpenGL4.PrimitiveType primitiveType;



        public MeshRenderer(Mesh m, IMaterial mat) : this(m, mat, Assets.Shaders["default"], OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles) { }

        public MeshRenderer(Mesh m, IMaterial mat, ShaderProgram shader) : this(m, mat, shader, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles) { }

        public MeshRenderer(Mesh m, IMaterial mat, ShaderProgram shader, OpenTK.Graphics.OpenGL4.PrimitiveType ptype) {
            mesh = m;
            material = mat;
            this.shader = shader;
            primitiveType = ptype;
        }

        protected override void OnEnter() {
            Game.renderer.SetObject(gameObject.scene, shader, this);
        }

        protected override void OnLeave() {
            Game.renderer.RemoveObject(gameObject.scene, shader, this);
        }

        public void Render(ShaderProgram shader) {
            shader.set_mat4("obj_transform", gameObject.model_matrix);
            material.Apply(shader);

            mesh.Render(primitiveType);
        }

        public override void Start() {
            mesh.Init();
        }
    }
}
