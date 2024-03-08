using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib.Effects {
    public class DrawGlyph : ManagedEffect {
        public DrawGlyph(ContentManager content) {
            load_shader_file(content, "effects/glyph");
        }
        public void configure_shader(FontManager glyph_manager, GlyphInfo glyph) {}
    }
}
