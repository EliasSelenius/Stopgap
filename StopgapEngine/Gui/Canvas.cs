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


        public Element focusedElement { get; internal set; } = null;
        public float aspectRatio => (float)Game.window.Height / Game.window.Width;

        private readonly List<Element> rootElements = new List<Element>();

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
