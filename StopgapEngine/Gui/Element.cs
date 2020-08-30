using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nums;

namespace Stopgap.Gui {

    public enum Anchor {
        top_left,
        top_right,
        bottom_left,
        bottom_right,
        center,
        top_center,
        bottom_center,
        left_center,
        right_center
    }

    public class Element {

        public Canvas canvas { get; private set; }
        public Element parent { get; private set; }
        public readonly List<Element> children = new List<Element>();
        public bool is_leafnode => children.Count == 0;
        public bool is_rootnode => parent == null;
        public bool has_parent => parent != null;
        public bool focused => canvas.focusedElement == this;

        public readonly Transform transform = new Transform();
        public Anchor anchor = Anchor.center;
        public bool draw_background = true;
        public vec4 background_color = (.7f, .7f, .7f, 1);
        public bool visible = true;
        public bool active = true;


        protected virtual void OnConnected() { }
        protected virtual void ConnectedToParent() { }

        protected virtual void Update() { }
        protected virtual void Draw() { }

        public event Action<Element> OnHover;
        public event Action<Element> OnClick;
        public event Action<Element> OnFocus;
        public event Action<Element> OnUnfocus;
        public event Action<Element> OnUpdate;
        public event Action<Element> OnRender;

        internal static T Create<T>(Canvas c) where T : Element, new() {
            T t = new T();
            t.canvas = c;
            t.OnConnected();
            return t;
        }

        public T AddChild<T>() where T : Element, new() {
            T elm = new T();
            children.Add(elm);

            elm.canvas = this.canvas;
            elm.parent = this;
            
            elm.ConnectedToParent();

            return elm;
        }

        public virtual void loadXml(XmlElement xml) {

        }

        private void ApplyUniforms() {
            /*
            if (HasParent) {
                p /= Parent.size;
                p += Parent.pos;
            }*/

            //Canvas.rectShader.set_vec4("rectTransform", transform.position.x, transform.position.y, transform.scale.x, transform.scale.y);
            Canvas.rectShader.set_mat4("model", transform.matrix);
            Canvas.rectShader.set_vec4("color", background_color);
        }

        internal void Render() {

            if (!visible) return;

            if (draw_background) {
                Canvas.rectShader.use();
                ApplyUniforms();
                Graphics.RenderRect();
            }

            Draw();
            OnRender?.Invoke(this);

            for (int i = 0; i < children.Count; i++) {
                children[i].Render();
            }
        }

        public void Focus() {
            if (canvas.focusedElement != this) {
                canvas.focusedElement?.OnUnfocus(canvas.focusedElement);
                canvas.focusedElement = this;
                OnFocus?.Invoke(this);
            }
        }

        public void Unfocus() {
            if (focused) {
                canvas.focusedElement = null;
                OnUnfocus?.Invoke(this);
            }
        }

        internal void UpdateEvents() {

            if (!active) return;

            var mp = Input.MousePos_ndc;
            var hs = transform.scale.xy * .5f;
            if (MyMath.InsideBounds(mp, transform.position.xy - hs, transform.position.xy + hs)) {
                this.OnHover?.Invoke(this);
                if (Input.LeftMousePressed) {
                    this.OnClick?.Invoke(this);
                    this.Focus();       
                } 
            } else {
                if (Input.LeftMousePressed || Input.RightMousePressed) {
                    this.Unfocus();
                }
            }

            Update();
            OnUpdate?.Invoke(this);

            for (int i = 0; i < children.Count; i++) {
                children[i].UpdateEvents();
            }
        }
    }
}
