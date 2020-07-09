using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using Glow;
using Nums;
using OpenTK;

using OpenTK.Graphics.OpenGL4;

namespace Stopgap {
    public class Camera : Component {

        private static int ubo;

        static Camera() {
            ubo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
            GL.BufferData(BufferTarget.UniformBuffer, 128, IntPtr.Zero, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ubo);

        }


        public Matrix4 view_matrix { get; private set; }
        public Matrix4 projection { get; private set; }
        public float FOV = 70;
        public float NearPlane = .1f;
        public float FarPlane = 100000;

        public void use() {
            scene.main_camera = this;
        }

        protected override void OnEnter() {
            use();
        }

        protected override void OnLeave() {
            if (scene.main_camera == this) scene.main_camera = null;
        }

        internal void update_uniformbuffer() {
            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
            
            // Projection:
            var p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), (float)Game.window.Width / Game.window.Height, NearPlane, FarPlane);
            projection = p;
            //program.SetMat4("cam_projection", p);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)0, 64, ref p);


            // View:
            var view = Matrix4.LookAt(transform.position.ToOpenTKVec(), (transform.position + transform.forward).ToOpenTKVec(), transform.up.ToOpenTKVec());
            view_matrix = view;
            //program.SetMat4("cam_view", lookat);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)64, 64, ref view);


            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public vec3 screenToRay(vec2 ndc) {

            var screenPoint = new Vector4(ndc.x, ndc.y, -1, 1);
            screenPoint = screenPoint * projection.Inverted();
            screenPoint.Z = -1;
            screenPoint.W = 0;

            var point = (screenPoint * view_matrix.Inverted()).Xyz;
            point.Normalize();
            return point.ToNumsVec();
        }
    }
}
