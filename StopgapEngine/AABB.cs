using Nums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public class AABB : Collider {
        internal override bool intersectsAABB(AABB other) {
            throw new NotImplementedException();
        }

        internal override bool intersectsRay(vec3 pos, vec3 direction) {
            throw new NotImplementedException();
        }

        internal override bool intersectsSphere(SphereCollider other) {
            throw new NotImplementedException();
        }
    }
}
