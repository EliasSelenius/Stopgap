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


        public MeshRenderer() {
            mesh = Assets.GetMesh("cube");
            material = PBRMaterial.Default;
            primitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles;
        }

        public MeshRenderer(Mesh m, Material mat) : this(m, mat, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles) { }

        public MeshRenderer(Mesh m, Material mat, OpenTK.Graphics.OpenGL4.PrimitiveType ptype) {
            mesh = m;
            material = mat;
            primitiveType = ptype;
        }

        protected override void OnEnter() {
            //Game.renderer.SetObject(gameObject.scene, material.shader, this);
            scene.renderables.Add(this);
        }

        protected override void OnLeave() {
            //Game.renderer.RemoveObject(gameObject.scene, material.shader, this);
            scene.renderables.Remove(this);
        }

        public void render() {
            material.shader.set_mat4("model", gameObject.model_matrix);
            material.apply();

            mesh.Render(primitiveType);
        }

        public override void Start() {
            mesh.Init();
        }
    }

    public class AdvMeshRenderer : Component, IRenderable {
        public AdvMesh mesh;
        public void render() {
            // TODO: fix this:
            Game.renderer.default_shader.set_mat4("model", gameObject.model_matrix);
            mesh.render();
        }
        protected override void OnEnter() {
            scene.renderables.Add(this);
        }

        protected override void OnLeave() {
            scene.renderables.Remove(this);
        }
    }
}
