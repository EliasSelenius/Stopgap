using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public class GravitationalObject : Component {

        private static readonly Dictionary<Scene, List<GravitationalObject>> otherObjects = new Dictionary<Scene, List<GravitationalObject>>();

        private Rigidbody rb;

        public override void Start() {
            rb = gameObject.GetComponent<Rigidbody>();
        }

        protected override void OnEnter() {
            if (!otherObjects.ContainsKey(gameObject.scene))
                otherObjects[gameObject.scene] = new List<GravitationalObject>();

            otherObjects[gameObject.scene].Add(this);
        }

        protected override void OnLeave() {
            otherObjects[gameObject.scene].Remove(this);
        }

        protected override void Update() {
            var objs = otherObjects[gameObject.scene];
            for (int i = 0; i < objs.Count; i++) {
                var other = objs[i];
                if (other == this) continue;

                var dir = other.transform.position - transform.position;
                rb.AddForce(dir.normalized() * 100 * rb.Mass * other.rb.Mass / dir.sqlength * Game.deltaTime);
            }
        }

        

    }
}
