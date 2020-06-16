using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;

namespace Stopgap.Gui {
    public class Canvas {
        internal static ShaderProgram rectShader;
        internal static ShaderProgram textShader;

        static Canvas() {
            rectShader = ShaderProgram.create(Shaders.ShaderResources.rectElementFragment, Shaders.ShaderResources.rectElementVertex);
            textShader = ShaderProgram.create(Shaders.ShaderResources.textFragment, Shaders.ShaderResources.textVertex);
        }



        public int width { get; private set; }
        public int height { get; private set; }
        public void resize(int w, int h) {
            width = w; height = h;
        }

        public Element focusedElement { get; internal set; } = null;
        public float aspectRatio => (float)height / width;

        private readonly List<Element> rootElements = new List<Element>();
        
        public Canvas(int w, int h) {
            width = w;
            height = h;
        }

        public T Create<T>() where T : Element, new() {
            var e = Element.Create<T>(this);
            rootElements.Add(e);
            return e;
        }

        internal void Render() {
            for (int i = 0; i < rootElements.Count; i++) {
                rootElements[i].Render();
            }
        }

        internal void Update() {
            for (int i = 0; i < rootElements.Count; i++) {
                rootElements[i].UpdateEvents();
            }
        }

    }
}
