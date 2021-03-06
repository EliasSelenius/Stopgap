﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;


using Nums;
using System.Net;

namespace Stopgap {
    public static class MyMath {

        public static bool InsideBounds(vec2 v, vec2 a, vec2 b) => InsideBounds(v.x, a.x, b.x) && InsideBounds(v.y, a.y, b.y);
        public static bool InsideBounds(float v, float a, float b) => (v < a) == (v > b);

        public static vec3 Sum(params vec3[] vecs) => vecs.Aggregate((x, y) => x + y);

        public static vec3 AvgVec(params vec3[] vecs) {
            return Sum(vecs) / (float)vecs.Length;
        }

        public static Nums.vec3 ToNumsVec(this Vector3 v) => new Nums.vec3(v.X, v.Y, v.Z);
        public static Vector3 ToOpenTKVec(this Nums.vec3 v) => new Vector3(v.x, v.y, v.z);
        public static Nums.vec4 ToNumsVec(this Vector4 v) => new Nums.vec4(v.X, v.Y, v.Z, v.W);
        public static Vector4 ToOpenTKVec(this Nums.vec4 v) => new Vector4(v.x, v.y, v.z, v.w);


        public static vec3 Transform(this OpenTK.Matrix4 m, vec3 a) {
            return (new Vector4(a.x, a.y, a.z, 1f) * m).Xyz.ToNumsVec();
        }

        private static int seed = int.MinValue;

        public static vec3 RandomDirection() => RandomDirection(seed++);
        public static vec3 RandomDirection(int seed) => new vec3(math.rand(seed), math.rand(seed + 1), math.rand(seed + 2)).normalized();
        public static float RandomRange(float minmag, float maxmag) => minmag + (math.rand(seed++) * 0.5f + 1f) * (maxmag - minmag);
        public static vec3 RandomVector(float minmag, float maxmag) => RandomDirection() * RandomRange(minmag, maxmag);
        public static float Random() => math.rand(seed++);
    }
}
