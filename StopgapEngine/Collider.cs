using Nums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public abstract class Collider : Component {

        public vec3 offset = vec3.zero;

        protected vec3 getWorldPos() => transform.position + transform.right * offset.x + transform.up * offset.y + transform.forward * offset.z;

        internal bool intersects(Collider other) {
            if (other is SphereCollider sc) return intersectsSphere(sc);
            if (other is AABB aabb) return intersectsAABB(aabb);

            return false;
        }

        internal abstract bool intersectsSphere(SphereCollider other);
        internal abstract bool intersectsAABB(AABB other);
        internal abstract bool intersectsRay(vec3 pos, vec3 direction);
    }
}
