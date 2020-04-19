using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
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


        public static Camera MainCamera;

        public float FOV = 70;

        public float NearPlane = .1f;
        public float FarPlane = 2000;

        public void SetToMain() {
            MainCamera = this;
        }

        public override void Start() {
            SetToMain();
        }



        // TODO: remove necessity for ShaderProgram argument e.g: remove cam_pos use from shaders
        internal void UpdateCamUniforms(ShaderProgram program) {
            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
            
            // Projection:
            var p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), (float)Game.window.Width / Game.window.Height, NearPlane, FarPlane);
            //program.SetMat4("cam_projection", p);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)0, 64, ref p);


            // View:
            var lookat = Matrix4.LookAt(transform.position.ToOpenTKVec(), (transform.position + transform.forward).ToOpenTKVec(), transform.up.ToOpenTKVec());
            //program.SetMat4("cam_view", lookat);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)64, 64, ref lookat);


            GL.BindBuffer(BufferTarget.UniformBuffer, 0);


            // Position:
            program.SetVec3("cam_pos", transform.position);

        }
    }
}
