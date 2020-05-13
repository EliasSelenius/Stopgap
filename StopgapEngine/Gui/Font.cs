﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nums;
using Glow;

namespace Stopgap.Gui {
    public class Font {

        public static readonly Font Arial = new Font(FontResources.arial, new Texture2D(FontResources.arialAtlas, true));

        public readonly Texture2D Atlas;

        public readonly List<Glyph> glyphs = new List<Glyph>();

        public int paddingTop { get; private set; }
        public int paddingBottom { get; private set; }
        public int paddingLeft { get; private set; }
        public int paddingRight { get; private set; }


        public int lineHeight { get; private set; }

        public class Glyph {
            public readonly int id;
            public readonly vec2 pos;
            public readonly vec2 size;
            public readonly vec2 offset;
            public readonly float advance;

            public Glyph(int id, vec2 pos, vec2 size, vec2 offset, float advance) {
                this.id = id;
                this.pos = pos;
                this.size = size;
                this.offset = offset;
                this.advance = advance;
            }
        }

        public Font(string fontData, Texture2D atlas) {
            Atlas = atlas;
            this.LoadFontData(fontData.Split('\n', '\r'));
        }

        private void LoadFontData(string[] fontdata) {
            var values = new Dictionary<string, string>();

            fontdata = fontdata.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            int _getInt(string key) => int.Parse(values[key]);
            int[] _getArray(string key) => values[key].Split(',').Select(x => int.Parse(x)).ToArray();

            int lineIndex = 0;
            bool _next() {
                values.Clear();

                if (lineIndex >= fontdata.Length)
                    return false;

                var line = fontdata[lineIndex];

                // end if line is empty string:
                if (string.IsNullOrEmpty(line))
                    return false;

                foreach (var item in line.Split(' ')) {
                    var s = item.Split('=');
                    if (s.Length != 2) continue;
                    values.Add(s[0], s[1]);
                }
                lineIndex++;
                return true;
            }


            // load padding
            _next();
            var p = _getArray("padding");
            paddingTop = p[0];
            paddingLeft = p[1];
            paddingBottom = p[2];
            paddingRight = p[3];

            // load line height
            _next();
            lineHeight = _getInt("lineHeight");

            // load characters
            _next(); // skip page
            _next(); // skip chars count

            var atlasSize = new vec2(Atlas.width, Atlas.height);

            while(_next()) {
                glyphs.Add(
                    new Glyph(
                        _getInt("id"),
                        new vec2(_getInt("x"), _getInt("y")) / atlasSize,
                        new vec2(_getInt("width"), _getInt("height")) / atlasSize,
                        new vec2(_getInt("xoffset"), _getInt("yoffset")) / atlasSize,
                        _getInt("xadvance") / atlasSize.x
                    ));
            }
            
        }

        public Glyph GetChar(char c) {
            var code = Encoding.ASCII.GetBytes(new[] { c })[0];

            return (from o in glyphs
                    where o.id == code
                    select o).FirstOrDefault();
        }
    }
}
