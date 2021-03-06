﻿using Nums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public class SphereCollider : Collider {

        public float radius = 1;


        internal override bool intersectsAABB(AABB other) {
            throw new NotImplementedException();
        }

        internal override bool intersectsRay(vec3 pos, vec3 direction) {
            var hypv = getWorldPos() - pos;
            var dot = direction.dot(hypv);
            return math.sqrt(hypv.sqlength - dot * dot) - radius < 0;
        }

        internal override bool intersectsSphere(SphereCollider other) {
            return (other.getWorldPos() - this.getWorldPos()).length - radius - other.radius < 0;
        }
    }
}
