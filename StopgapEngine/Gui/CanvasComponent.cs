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
            scene.renderables.Add(this);
            //Game.renderer.SetObject(gameObject.scene, Canvas.rectShader, this);
        }

        protected override void OnLeave() {
            scene.renderables.Remove(this);
            //Game.renderer.RemoveObject(gameObject.scene, Canvas.rectShader, this);
        }

        public void render() {
            canvas.Render();
        }
    }
}
