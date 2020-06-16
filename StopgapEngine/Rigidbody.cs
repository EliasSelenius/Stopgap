using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using Nums;

namespace Stopgap {
    public class Rigidbody : Component {

        public float Mass = 1f;
        public vec3 CenterOfMass = vec3.zero;

        public vec3 Velocity;
        public vec3 AngularVelocity;

        public vec3 Momentum => Velocity * Mass;

        protected override void Update() {
            transform.position += Velocity * Game.deltaTime;
            transform.Rotate(AngularVelocity * Game.deltaTime);
            
            // air drag:
            //Velocity += -Momentum * .01f;

        }

        public void AddForce(vec3 force) {
            Velocity += force / Mass;
        }

        public void AddForce(vec3 force, vec3 offset) {
            offset -= CenterOfMass;
            AddForce(force);
            AddTorque(offset.cross(force));
        }

        public void AddTorque(vec3 torque) {
            AngularVelocity += torque / Mass; // TODO: moment of inertia?
        }

        public override void OnCollision(Collider other) {
            Console.WriteLine("COLLISION");

            AddForce(other.transform.position - transform.position);

        }

    }
}
