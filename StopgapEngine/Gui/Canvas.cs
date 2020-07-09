using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;

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

        public Canvas(XmlElement canvas) {


            string get_attrib(XmlElement el, string name, string defvalue) {
                var a = el.GetAttribute(name);
                return a == string.Empty ? defvalue : a;
            }
            int get_int(XmlElement el, string name, string defvalue) => int.Parse(get_attrib(el, name, defvalue));

            width = get_int(canvas, "width", Game.window.Width.ToString());
            height = get_int(canvas, "height", Game.window.Height.ToString());

            var element_types = new Dictionary<string, Func<XmlElement, Element>>() {
                { "element", xml => {
                    return new Element();
                }},
                { "textbox", xml => {
                    var el = new Textbox();

                    return el;
                }},
            };
            
            void process_element(XmlElement xml_el) {

                var el = element_types[xml_el.Name](xml_el);

                // process attributes

                // process children
            }


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
