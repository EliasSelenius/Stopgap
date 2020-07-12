using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public static class Objectpool {
        public static void recycle<T>(T item) where T : class, new() => Objectpool<T>.recycle(item);
    }

    public static class Objectpool<T> where T : class, new() {

        private static readonly Stack<T> pool = new Stack<T>();

        public static T get() {
            if (pool.Any()) return pool.Pop();
            return new T();
        }

        public static void recycle(T item) {
            pool.Push(item);
        }

        public static void alloc(int count) {
            for (int i = 0; i < count; i++) {
                pool.Push(new T());
            }
        }
    }
}
