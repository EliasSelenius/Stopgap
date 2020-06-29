using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;

namespace Stopgap {
    public class MeshRenderer : Component, IRenderable {

        public readonly Mesh mesh;
        public Material material;
        
        public OpenTK.Graphics.OpenGL4.PrimitiveType primitiveType;



        public MeshRenderer(Mesh m, Material mat) : this(m, mat, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles) { }

        public MeshRenderer(Mesh m, Material mat, OpenTK.Graphics.OpenGL4.PrimitiveType ptype) {
            mesh = m;
            material = mat;
            primitiveType = ptype;
        }

        protected override void OnEnter() {
            Game.renderer.SetObject(gameObject.scene, material.shader, this);
        }

        protected override void OnLeave() {
            Game.renderer.RemoveObject(gameObject.scene, material.shader, this);
        }

        public void Render(ShaderProgram shader) {
            shader.set_mat4("obj_transform", gameObject.model_matrix);
            material.apply();

            mesh.Render(primitiveType);
        }

        public override void Start() {
            mesh.Init();
        }
    }
}
