using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glow;
using Nums;

namespace Stopgap {
    public class CamFlyController : Component {

        public float smooth = 0;

        public override void Start() {
            //Input.FixedMouse(true);
        }

        float x_angle, y_angle;

        protected override void Update() {

            if (Input.IsKeyPressed(OpenTK.Input.Key.T)) {
                var o = Assets.getPrefab("test-prefab").createInstance();
                o.EnterScene(scene);
            }


            if (Input.MouseRightButtonDown) {
                Input.FixedMouse(true);
                Input.hideMouse(true);
                var zrotinput = -Input.KeyAxis(OpenTK.Input.Key.Q, OpenTK.Input.Key.E) / 10f;
                //transform.Rotate(vec3.unity, -Input.MouseDelta.x / 100f);
                //transform.Rotate(transform.right, Input.MouseDelta.y / 100f);
                x_angle = math.clamp(x_angle + Input.MouseDelta.y / 100f, -math.half_pi, math.half_pi);
                y_angle += -Input.MouseDelta.x / 100f;
                transform.setRotation(vec3.unity, y_angle);
                transform.Rotate(transform.right, x_angle);

                float speed = .4f;
                if (Input.IsKeyDown(OpenTK.Input.Key.LShift)) {
                    speed = 1.6f;
                }

                var translation = vec3.zero;
                var wasd = Input.Wasd;
                translation += transform.forward * wasd.y;
                translation += transform.left * wasd.x;
                translation += transform.up * Input.KeyAxis(OpenTK.Input.Key.Space, OpenTK.Input.Key.LControl);
                transform.position += translation * speed;

                //transform.Rotate(transform.up, Input.MouseDelta.X / 100);
                //transform.Rotate(transform.right, Input.MouseDelta.Y / 100);
                //transform.Rotate(transform.forward, Input.KeyAxis(OpenTK.Input.Key.E, OpenTK.Input.Key.Q) / 15);

                //Console.WriteLine(Input.MouseDelta);

            } else {
                Input.FixedMouse(false);
                Input.hideMouse(false);
            }

        }
    }
}
