using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib {
    public static class SDF {
        private static Effect sdf_effect;
        private static Effect sdf_circle_effect;

        internal static bool effect_unloaded => sdf_effect == null;
        internal static void load(ContentManager Content) {
            if (effect_unloaded) sdf_circle_effect = Content.Load<Effect>("effects/sdf_circle");
            if (effect_unloaded) sdf_effect = Content.Load<Effect>("effects/sdf");
        }


        public static void draw_circle(Vector2 position, float radius, float line_width, Color color) {
            Drawing.end();
            Drawing.begin(sdf_circle_effect);
            sdf_circle_effect.Parameters["size"].SetValue(Vector2.One * radius * 2);
            sdf_circle_effect.Parameters["fill"].SetValue(false);
            sdf_circle_effect.Parameters["line_width"].SetValue(line_width);
            Drawing.sb.Draw(Drawing.OnePXWhite, position, null,
                    color, 0f, Vector2.One * 0.5f, Vector2.One * radius * 2,
                SpriteEffects.None, 1f);
            Drawing.end();
        }

        public static void fill_circle(Vector2 position, float radius, Color color) {
            Drawing.end();
            Drawing.begin(sdf_circle_effect);
            sdf_circle_effect.Parameters["size"].SetValue(Vector2.One * radius * 2);
            sdf_circle_effect.Parameters["fill"].SetValue(true);
            Drawing.sb.Draw(Drawing.OnePXWhite, position, null,
                    color, 0f, Vector2.One * 0.5f, Vector2.One * radius * 2,
                SpriteEffects.None, 1f);
            Drawing.end();
        }


        public static void draw(Texture2D sdf, Vector2 position, Vector2 scale, Color color) {
            draw(sdf, position, scale, color, null, 0.99f, 0f, 1f, false);

        }
        public static void draw(Texture2D sdf, Vector2 position, Vector2 scale, Color color, float scissor) {
            draw(sdf, position, scale, color, null, scissor, 0f, 1f, false);
        }

        public static void draw_centered(Texture2D sdf, Vector2 position, Vector2 scale, Color color) {
            draw_centered(sdf, position, scale, color, null, 0.99f, 0f, 1f, false);

        }
        public static void draw_centered(Texture2D sdf, Vector2 position, Vector2 scale, Color color, float scissor) {
            draw_centered(sdf, position, scale, color, null, scissor, 0f, 1f, false);
        }

        private static void configure_shader(Texture2D sdf, Vector2 position, Vector2 scale, Color inner, Color? outline_color, float scissor, float outline_width, float opacity, bool invert) {
            sdf_effect.Parameters["alpha_scissor"].SetValue(scissor);

            sdf_effect.Parameters["invert_map"].SetValue(invert);
            sdf_effect.Parameters["opacity"].SetValue(opacity);
            sdf_effect.Parameters["blend"].SetValue(0.005f);

            sdf_effect.Parameters["inside_color"].SetValue(inner.ToVector4());
            sdf_effect.Parameters["outside_color"].SetValue(Color.Transparent.ToVector4());
                        
            sdf_effect.Parameters["enable_outline"].SetValue(outline_color.HasValue);
            if (outline_color.HasValue) {
                sdf_effect.Parameters["outline_color"].SetValue(outline_color.Value.ToVector4());
                sdf_effect.Parameters["outline_width"].SetValue(outline_width);
            } else {
                sdf_effect.Parameters["outline_color"].SetValue(Color.Transparent.ToVector4());
                sdf_effect.Parameters["outline_width"].SetValue(0f);
            }            
        }

        public static void draw(Texture2D sdf, Vector2 position, Vector2 scale, Color inner, Color? outline_color, float scissor, float outline_width, float opacity, bool invert) {
            configure_shader(sdf, position, scale, inner, outline_color, scissor, outline_width, opacity, invert);
            
            var tex_size = new Vector2(sdf.Bounds.Size.X, sdf.Bounds.Size.Y);

            Drawing.end();
            Drawing.graphics_device.BlendState = BlendState.AlphaBlend;
            Drawing.graphics_device.DepthStencilState = DepthStencilState.Default;            

            Drawing.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, null, sdf_effect, null);
            Drawing.sb.Draw(sdf, new Rectangle((int)position.X, (int)position.Y, (int)scale.X, (int)scale.Y), null, Color.White, 0f,
                Vector2.Zero, SpriteEffects.None, 1f);
            Drawing.sb.End();
        }

        public static void draw_centered(Texture2D sdf, Vector2 position, Vector2 scale, Color inner, Color? outline_color, float scissor, float outline_width, float opacity, bool invert) {
            configure_shader(sdf, position, scale, inner, outline_color, scissor, outline_width, opacity, invert);

            var tex_size = new Vector2(sdf.Bounds.Size.X, sdf.Bounds.Size.Y);

            Drawing.end();
            
            Drawing.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, null, sdf_effect, null);
            Drawing.sb.Draw(sdf, new Rectangle((int)position.X, (int)position.Y, (int)scale.X, (int)scale.Y), null, Color.White, 0f,
                (Vector2.One * 0.5f) * tex_size, SpriteEffects.None, 1f);
            Drawing.sb.End();
        }
    }
}
