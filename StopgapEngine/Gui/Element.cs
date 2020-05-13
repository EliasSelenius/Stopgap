using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;

namespace Stopgap.Gui {

    public enum Anchor {
        top_left,
        top_right,
        bottom_left,
        bottom_right,
        center
    }

    public class Element {

        public Canvas canvas { get; private set; }
        public Element parent { get; private set; }
        public readonly List<Element> children = new List<Element>();
        public bool IsLeafNode => children.Count == 0;
        public bool IsRootNode => parent == null;
        public bool HasParent => parent != null;
        public bool IsInFocus => canvas.focusedElement == this;

        public Anchor anchor = Anchor.center;
        public unit2 size = new unit2(UnitType.viewHeight, .5f, .5f);
        public unit2 pos = new unit2(UnitType.ndc, 0, 0);
        public vec2 size_ndc => size.get_ndc(this);
        public vec2 pos_ndc => anchor switch {
            Anchor.top_left => pos.get_ndc(this) + size_ndc * new vec2(.5f, -.5f),
            Anchor.top_right => pos.get_ndc(this) + size_ndc * new vec2(-.5f, -.5f),
            Anchor.bottom_left => pos.get_ndc(this) + size_ndc * .5f,
            Anchor.bottom_right => pos.get_ndc(this) + size_ndc * new vec2(-.5f, .5f),
            Anchor.center => pos.get_ndc(this)
        };
            


        // ndc is normalized device coordinates
        /*
        public vec2 size_ndc {
            get => new vec2(size.x * canvas.aspectRatio, size.y);
            set => size = new vec2(value.x / canvas.aspectRatio, value.y);
        }
        public vec2 pos_ndc {
            get => new vec2(pos.x * canvas.aspectRatio, pos.y);
            set => pos = new vec2(value.x / canvas.aspectRatio, value.y);
        }*/

        //public float aspect => size.y / size.x;

        public bool draw_background = true;
        public vec4 background_color = (.7f, .7f, .7f, 1);
        public bool Visible = true;
        public bool Active = true;


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

        private void ApplyUniforms() {
            /*
            if (HasParent) {
                p /= Parent.size;
                p += Parent.pos;
            }*/

            vec2 p = pos_ndc, s = size_ndc;
            Canvas.rectShader.set_vec4("rectTransform", p.x, p.y, s.x, s.y);
            Canvas.rectShader.set_vec4("color", background_color);
        }

        internal void Render() {

            if (!Visible) return;

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
            if (IsInFocus) {
                canvas.focusedElement = null;
                OnUnfocus?.Invoke(this);
            }
        }

        internal void UpdateEvents() {

            if (!Active) return;

            var mp = Input.MousePos_ndc;
            var hs = size_ndc * .5f;
            if (MyMath.InsideBounds(mp, pos_ndc - hs, pos_ndc + hs)) {
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
