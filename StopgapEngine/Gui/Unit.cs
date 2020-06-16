using Nums;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stopgap.Gui {

    /*
        different units:
            - ndc [-1..1]
            - px [0..size]
            - vh  [0..1]
            - vw [0..1]

        
    */

    public enum UnitType {
        ndc,
        pixels,
        viewHeight,
        viewWidth
    }

    public struct unit {

        public readonly UnitType type;
        public float value;

        public unit(UnitType type, float value) {
            this.type = type;
            this.value = value;
        }

        public float get_ndc(Element element, bool vertical) {
            return type switch {
                UnitType.ndc => value,
                UnitType.pixels => value / (vertical ? element.canvas.height : element.canvas.width) * 2 - 1,
                UnitType.viewHeight => (value * 2) * (vertical ? 1 : element.canvas.aspectRatio),
                UnitType.viewWidth => (value * 2) / (vertical ? element.canvas.aspectRatio : 1),
                _ => throw new Exception("")
            };
        }

        public static unit parse(string text) {
            UnitType t;
            if (text.EndsWith("ndc")) t = UnitType.ndc;
            else if (text.EndsWith("px")) t = UnitType.pixels;
            else if (text.EndsWith("vh")) t = UnitType.viewHeight;
            else if (text.EndsWith("vw")) t = UnitType.viewWidth;
            else throw new Exception("");

            var i = math.max(text.IndexOf("ndc"), math.max(text.IndexOf("px"), math.max(text.IndexOf("vh"), text.IndexOf("vw"))));

            var num = float.Parse(text.Substring(0, (int)i));
            return new unit(t, num);
        } 

    }

    public struct unit2 {
        public unit x, y;

        public unit2(UnitType type, float x, float y) {
            this.x = new unit(type, x);
            this.y = new unit(type, y);
        }

        public vec2 get_ndc(Element element) => new vec2(x.get_ndc(element, false), y.get_ndc(element, true));

        public static unit2 parse(string text) {
            var u = new unit2();
            var split = text.Split(' ');
            if (split.Length == 1) u.x = u.y = unit.parse(text);
            else {
                u.x = unit.parse(split[0]);
                u.y = unit.parse(split[1]);
            }
            return u;
        }
    }
}
