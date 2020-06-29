using Glow;
using Nums;
using OpenTK;
using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stopgap {
    public class Billboard : Component, IRenderable {

        private static readonly Mesh quad;

        static Billboard() {
            quad = Mesh.GenQuad(); quad.Init();
        }


        public Material material;
        
        public Billboard(Material material) {
            this.material = material;
        }

        protected override void OnEnter() {
            Game.renderer.SetObject(gameObject.scene, material.shader, this);
        }

        protected override void OnLeave() {
            Game.renderer.RemoveObject(gameObject.scene, material.shader, this);
        }

        public void Render(ShaderProgram shader) {

            var m = Matrix4.CreateTranslation(transform.position.ToOpenTKVec());
            var v = Camera.MainCamera.viewMatrix;

            /*
             m11 m12 m13
             m21 m22 m23
             m31 m32 m33
             */

            m.M11 = v.M11;
            m.M12 = v.M21;
            m.M13 = v.M31;

            m.M21 = v.M12;
            m.M22 = v.M22;
            m.M23 = v.M32;

            m.M31 = v.M13;
            m.M32 = v.M23;
            m.M33 = v.M33;
            

            m *= Matrix4.CreateRotationZ(transform.rotation.ToAxisAngle().W);
            m *= Matrix4.CreateScale(transform.scale.ToOpenTKVec());
            

            shader.set_mat4("obj_transform", m);
            material.apply();
            quad.Render();
        }

        
    }
}
