using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;

namespace Stopgap.Gui {
    public class Element {

        public Canvas canvas { get; private set; }
        public Element parent { get; private set; }
        public readonly List<Element> children = new List<Element>();
        public bool IsLeafNode => children.Count == 0;
        public bool IsRootNode => parent == null;
        public bool HasParent => parent != null;
        public bool IsInFocus => canvas.focusedElement == this;

        public vec2 size = vec2.one;
        public vec2 pos = vec2.zero;

        // ndc is normalized device coordinates
        public vec2 size_ndc {
            get => new vec2(size.x * canvas.aspectRatio, size.y);
            set => size = new vec2(value.x / canvas.aspectRatio, value.y);
        }
        public vec2 pos_ndc {
            get => new vec2(pos.x * canvas.aspectRatio, pos.y);
            set => pos = new vec2(value.x / canvas.aspectRatio, value.y);
        }

        public float aspect => size.y / size.x;


        public vec4 background_color = (.7f, .7f, .7f, 1);
        public bool Visible = true;
        public bool Active = true;


        protected virtual void ConnectedToParent() { }
        protected virtual void OnHover() {
            //background_color = (0, 1, 0, 1);
        }
        protected virtual void OnClick() {
            Console.WriteLine("click");
            //background_color = (0, 0, 1, 1);
        }
        protected virtual void OnUpdate() { }
        protected virtual void OnRender() { }
        protected virtual void OnFocus() { }
        protected virtual void OnLostFocus() { }

        internal static T Create<T>(Canvas c) where T : Element, new() {
            T t = new T();
            t.canvas = c;
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
            Canvas.guiShader.SetVec4("rectTransform", p.x, p.y, s.x, s.y);
            Canvas.guiShader.SetVec4("color", background_color);
        }

        internal void Render() {

            if (!Visible) return;

            ApplyUniforms();
            Graphics.RenderRect(); // for debug
            OnRender();

            for (int i = 0; i < children.Count; i++) {
                children[i].Render();
            }
        }

        public void Focus() {
            if (canvas.focusedElement != this) {
                canvas.focusedElement?.OnLostFocus();
                canvas.focusedElement = this;
                OnFocus();
            }
        }

        public void Unfocus() {
            if (IsInFocus) {
                canvas.focusedElement = null;
                OnLostFocus();
            }
        }

        internal void UpdateEvents() {

            if (!Active) return;

            var mp = Input.MousePos_ndc;
            var hs = size_ndc * .5f;
            if (MyMath.InsideBounds(mp, pos_ndc - hs, pos_ndc + hs)) {
                this.OnHover();
                if (Input.LeftMousePressed) {
                    this.OnClick();
                    this.Focus();       
                } 
            } else {
                if (Input.LeftMousePressed) {
                    this.Unfocus();
                }
            }

            OnUpdate();

            for (int i = 0; i < children.Count; i++) {
                children[i].UpdateEvents();
            }
        }
    }
}
