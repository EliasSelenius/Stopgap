using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public class ObjectPool<T> where T : class, new() {

        private readonly List<T> unused = new List<T>();

        public void pre_alloc(int count) {
            for (int i = 0; i < count; i++) {
                return_object(@new());
            }
        }

        private T @new() => new T();

        public T get_object() {
            T res = null;
            if (unused.Any()) {
                res = unused[0];
                unused.Remove(res);
            }
            return res ?? @new();
        }

        public void return_object(T t) {
            unused.Add(t);
        }

    }
}
