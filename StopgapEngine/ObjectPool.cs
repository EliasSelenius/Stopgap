using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap {
    public class ObjectPool<T> where T : class, new() {

        private readonly List<T> inuse = new List<T>();
        private readonly List<T> unused = new List<T>();

        public T GetNewObject() {
            T res = null;
            if (unused.Any()) {
                res = unused[0];
                unused.Remove(res);
            } else {
                res = new T();
            }
            inuse.Add(res);
            return res;
        }

        public void Unuse(T t) {
            inuse.Remove(t);
            unused.Add(t);
        }

    }
}
