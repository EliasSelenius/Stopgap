using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;

namespace Stopgap.Gui {
    public class Canvas {
        public static ShaderProgram guiShader;

        static Canvas() {
            guiShader = ShaderProgram.CreateProgram(Shaders.ShaderResources.guiFragment, Shaders.ShaderResources.guiVertex);
            Assets.Shaders["gui"] = guiShader;
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

            guiShader.Use();

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
