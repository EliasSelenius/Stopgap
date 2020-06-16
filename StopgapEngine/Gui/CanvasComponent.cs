using Glow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap.Gui {
    public class CanvasComponent : Component, IRenderable {

        public readonly Canvas canvas;


        public CanvasComponent(Canvas c) {
            canvas = c;
        }

        protected override void OnEnter() {
            Game.renderer.SetObject(gameObject.scene, Canvas.rectShader, this);
        }

        protected override void OnLeave() {
            Game.renderer.RemoveObject(gameObject.scene, Canvas.rectShader, this);
        }

        public void Render(ShaderProgram shader) {
            canvas.Render();
        }
    }
}
