﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SwoopLib.Effects;
using static System.Formats.Asn1.AsnWriter;

namespace SwoopLib
{

    public static class Drawing {
        public static SpriteBatch sb;

        private static bool _sb_drawing = false;
        public static bool sb_drawing {
            get { return _sb_drawing; }
            set { _sb_drawing = value; }
        }

        public static GraphicsDevice graphics_device;
        public static GraphicsDeviceManager graphics;

        internal static RenderTarget2D main_render_target;

        public static SpriteFont fnt_profont;

        public static Texture2D OnePXWhite;
        public static Texture2D Logo;

        public static Texture2D sdf_circle;
        private static int sdf_circle_res = 256;

        static Effects.Dither dither_effect;
        static Effects.TwoColorFlip color_flip_effect;

        public static void load(GraphicsDevice gd, GraphicsDeviceManager gdm, ContentManager content, XYPair resolution) {
            sb = new SpriteBatch(gd);

            graphics_device = gd;
            graphics = gdm;

            main_render_target = new RenderTarget2D(graphics_device, resolution.X, resolution.Y,
                false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            //create a 1x1 white texture
            OnePXWhite = new Texture2D(gd, 1, 1);
            OnePXWhite.SetData<Color>(new Color[1] { Color.White });

            Logo = content.Load<Texture2D>("swoop_logo");

            //create an SDF of a circle
            Color[] sdf_data = new Color[sdf_circle_res * sdf_circle_res];

            for (var i = 0; i < sdf_circle_res; i++) {

                float px, py;

                for (var x = 0; x < sdf_circle_res; x++) {

                    px = x / (float)sdf_circle_res;
                    py = i / (float)sdf_circle_res;

                    float t = Vector2.Distance(Vector2.One * 0.5f, new Vector2(px, py))*2;

                    int o = (int)((t) * 255);
                    sdf_data[(i * sdf_circle_res) + x] = Color.FromNonPremultiplied(255 - o, 255 - o, 255 - o, 255);
                }
            }

            sdf_circle = new Texture2D(gd, sdf_circle_res, sdf_circle_res);
            sdf_circle.SetData<Color>(sdf_data);

            fnt_profont = content.Load<SpriteFont>("profont");
        }

        public static void begin() {
            if (!sb_drawing) {
                //sb.Begin();
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, null);
                sb_drawing = true;
            }
        }
        public static void begin_dither(Vector2 top_left, Vector2 bottom_right, Color col_a, Color col_b) {
            if (dither_effect == null) dither_effect = new Dither(Swoop.content);

            dither_effect.configure_shader(top_left, bottom_right, col_a, col_b);
            dither_effect.begin_spritebatch(sb);
        }

        public static void begin_two_color_flip(Vector2 top_left, Vector2 bottom_right, Color col_a, Color col_b) {
            if (color_flip_effect == null) color_flip_effect = new TwoColorFlip(Swoop.content);

            color_flip_effect.configure_shader(top_left, bottom_right, col_a, col_b);
            color_flip_effect.begin_spritebatch(sb);
        }

        public static void text_inverting(string text, Vector2 top_left, Vector2 bottom_right, Color col_a, Color col_b) {
            begin_two_color_flip(top_left, bottom_right, col_a, col_b);

            Drawing.text(text, top_left, Color.White);
            end();
        }

        public static void begin(Effect effect) {
            if (!sb_drawing) {
                //sb.Begin();
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
                sb_drawing = true;
            }
        }

        public static void end() {
            if (sb_drawing) {
                sb.End();
                sb_drawing = false;
            }
        }

        public static void pixel(Vector2 position, Color color) {
            begin();
            sb.Draw(OnePXWhite, position, color);
        }
        public static void pixel(XYPair position, Color color) {
            begin();
            sb.Draw(OnePXWhite, position.ToVector2(), color);
        }

        public static void line(Vector2 A, Vector2 B, Color color, float thickness) {
            begin();

            var tan = B - A;
            var rot = (float)Math.Atan2(tan.Y, tan.X);

            var middlePoint = new Vector2(0, 0.5f);
            var scale = new Vector2(tan.Length(), thickness);

            sb.Draw(OnePXWhite, A, null, color, rot, middlePoint, scale, SpriteEffects.None, 0f);
        }


        public static void line(XYPair A, XYPair B, Color color, float thickness) {
            begin();

            var tan = B - A;
            var rot = (float)Math.Atan2(tan.Y, tan.X);

            var middlePoint = new Vector2(0, 0.5f);
            var scale = new Vector2(tan.Length(), thickness);

            sb.Draw(OnePXWhite, A.ToVector2(), null, color, rot, middlePoint, scale, SpriteEffects.None, 0f);
        }
        public static void line_rounded_ends(Vector2 A, Vector2 B, Color color, float thickness) {
            line(A, B, color, thickness);

            fill_circle(A, thickness + 1f, color);
            fill_circle(B, thickness + 1f, color);
        }

        public static void cross(Vector2 center, float size, Color color) {
            center.Round();
            line(center - (Vector2.UnitX * (size + 1)), center + (Vector2.UnitX * size), color, 1f);
            line(center - (Vector2.UnitY * (size + 1)), center + (Vector2.UnitY * size), color, 1f);
        }

        public static void poly(Color color, float thickness, bool close_polygon, params Vector2[] points) {
            if (points.Length < 2) return;
            begin();

            for (var i = 0; i < points.Length - 1; i++) {
                var p = points[i];
                var pP = points[i + 1];
                line(p, pP, color, thickness);
            }

            if (points.Length > 2 && close_polygon)
                line(points.First(), points.Last(), color, thickness);


            for (var i = 0; i < points.Length; i++) {
                //fill_circle(points[i], thickness, color);
            }
        }

        public static void tri(Vector2 A, Vector2 B, Vector2 C, Color color, float thickness) {
            begin();
            poly(color, thickness, true, A, B, C);
        }
        public static void tri(XYPair A, XYPair B, XYPair C, Color color, float thickness) {
            begin();
            poly(color, thickness, true, A.ToVector2(), B.ToVector2(), C.ToVector2());
        }

        public static void rect(Vector2 min, float size_x, float size_y, Color color, float thickness) {
            rect(min, min + new Vector2(size_x, size_y), color, thickness);
        }
        public static void rect(XYPair min, float size_x, float size_y, Color color, float thickness) {
            rect(min.ToVector2(), min.ToVector2() + new Vector2(size_x, size_y), color, thickness);
        }
        public static void rect(XYPair min, XYPair max, Color color, float thickness) {
            rect(min.ToVector2(), max.ToVector2(), color, thickness);
        }
        public static void rect(Vector2 min, Vector2 max, Color color, float thickness) {
            min.Floor(); max.Ceiling();
            begin();

            var w = Vector2.UnitX * (max.X - min.X);
            var h = Vector2.UnitY * (max.Y - min.Y);

            var half_line_x = Vector2.UnitX * (thickness/2f);

            //draw sides
            //top
            line(min - half_line_x,
                (min + half_line_x) + w,
                color, thickness);
            //bottom
            line((min - half_line_x) + h,
                (min + half_line_x) + h + w,
                color, thickness);
            //left
            line(min, min + h, color, thickness);
            //right
            line(min + w, min + h + w, color, thickness);
        }

        public static void fill_rect(Vector2 min, float size_x, float size_y, Color color) {
            fill_rect(min, min + new Vector2(size_x, size_y), color);
        }
        public static void fill_rect(XYPair min, float size_x, float size_y, Color color) {
            fill_rect(min.ToVector2(), min.ToVector2() + new Vector2(size_x, size_y), color);
        }


        public static void fill_rect(Vector2 min, Vector2 max, Color color) {
            min.Floor(); max.Ceiling();
            begin();
            sb.Draw(OnePXWhite, min, null, color, 0f, Vector2.Zero, max - min, SpriteEffects.None, 0f);
        }

        public static void fill_rect(XYPair min, XYPair max, Color color) {
            begin();
            sb.Draw(OnePXWhite, min.ToVector2(), null, color, 0f, Vector2.Zero, (max - min).ToVector2(), SpriteEffects.None, 0f);
        }
        public static void fill_rect_dither(XYPair min, XYPair max, Color color_a, Color color_b) {
            begin_dither(min.ToVector2(), max.ToVector2(), color_a, color_b);
            sb.Draw(OnePXWhite, min.ToVector2(), null, Color.White, 0f, Vector2.Zero, (max - min).ToVector2(), SpriteEffects.None, 0f);
            end();
        }


        public static void fill_rect_outline(Vector2 min, Vector2 max, Color color, Color outline, float outline_thickness) {
            min.Floor(); max.Ceiling();
            fill_rect(min, max, color);
            rect(min, max, outline, outline_thickness);
        }
        public static void fill_rect_outline(XYPair min, XYPair max, Color color, Color outline, float outline_thickness) {
            fill_rect(min, max, color);
            rect(min, max, outline, outline_thickness);
        }


        public static void circle(Vector2 P, float radius, float thickness, Color color) {
            SDF.draw_circle(P, radius, thickness, color);
        }
        public static void fill_circle(Vector2 P, float radius, Color color) {
            SDF.fill_circle(P, radius, color);
        }

        public static void image(Texture2D image, Vector2 position, Vector2 size) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
        }
        public static void image(RenderTarget2D image, Vector2 position, Vector2 size) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
        }

        public static void image(Texture2D image, Vector2 position, Vector2 size, SpriteEffects flip_mode) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), null, Color.White, 0f, Vector2.Zero, flip_mode, 1f);
        }
        public static void image(RenderTarget2D image, Vector2 position, Vector2 size, SpriteEffects flip_mode) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), null, Color.White, 0f, Vector2.Zero, flip_mode, 1f);
        }

        public static void image(Texture2D image, Vector2 position, Point size) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size), Color.White);
        }
        public static void image(RenderTarget2D image, Vector2 position, Point size) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size), Color.White);
        }
        public static void image(Texture2D image, Point position, Point size) {
            begin();
            sb.Draw(image, new Rectangle(position, size), Color.White);
        }
        public static void image(RenderTarget2D image, Point position, Point size) {
            begin();
            sb.Draw(image, new Rectangle(position, size), Color.White);
        }
        public static void image(Texture2D image, XYPair position, XYPair size) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
        }
        public static void image(RenderTarget2D image, XYPair position, XYPair size) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
        }
        public static void image(Texture2D image, Vector2 position, Vector2 size, Color tint) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), tint);
        }
        public static void image(RenderTarget2D image, Vector2 position, Vector2 size, Color tint) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()), tint);
        }
        public static void image(Texture2D image, Vector2 position, Vector2 size, float rotation) {
            begin();
            sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, Color.White, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
        }
        public static void image(RenderTarget2D image, Vector2 position, Vector2 size, float rotation) {
            begin();
            sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, Color.White, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
        }
        public static void image(Texture2D image, Vector2 position, Vector2 size, Color tint, float rotation) {
            begin();
            sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, tint, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
        }
        public static void image(RenderTarget2D image, Vector2 position, Vector2 size, Color tint, float rotation) {
            begin();
            sb.Draw(image, new Rectangle((position + (image.Bounds.Size.ToVector2() / 2f)).ToPoint(), size.ToPoint()), null, tint, MathHelper.ToRadians(rotation), image.Bounds.Size.ToVector2() / 2f, SpriteEffects.None, 0f);
        }


        public static void image(Texture2D image, Vector2 position, Vector2 size, int source_rect_x, int source_rect_y, int source_rect_w, int source_rect_h) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()),
                new Rectangle(source_rect_x, source_rect_y, source_rect_w, source_rect_h),
                Color.White);
        }

        public static void image(Texture2D image, Vector2 position, Vector2 size, Vector2 fractional_source_rect_pos, Vector2 fractional_source_rect_size) {
            begin();
            sb.Draw(image, new Rectangle(position.ToPoint(), size.ToPoint()),
                new Rectangle(
                    (int)(image.Width * fractional_source_rect_pos.X), (int)(image.Height * fractional_source_rect_pos.Y),
                    (int)(image.Width * fractional_source_rect_size.X), (int)(image.Height * fractional_source_rect_size.Y)),
                Color.White);
        }

        public static void text(string text, XYPair position, Color color) {
            Drawing.text(text, position.ToVector2(), color);
        }

        public static void text(string text, Vector2 position, Color color) {
            begin();
            position.Ceiling(); //this prevents half-pixel positioning which helps keep text crisp and artifact-free
            sb.DrawString(fnt_profont, text, position, color);
        }
        public static void text_vertical(string text, Vector2 position, Color color) {
            begin();
            position.Ceiling(); //this prevents half-pixel positioning which helps keep text crisp and artifact-free
            sb.DrawString(fnt_profont, text, position, color, MathHelper.ToRadians(90f), Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }

        public static void text_shadow(string text, Vector2 position, Color color) {
            Drawing.text(text, position + (Vector2.One), Swoop.UI_background_color);
            Drawing.text(text, position, color);
        }
        public static Vector2 measure_string_profont(string text) {
            return fnt_profont.MeasureString(text);
        }
        public static Point measure_string_profont_pt(string text) {
            return fnt_profont.MeasureString(text).ToPoint();
        }
        public static XYPair measure_string_profont_xy(string text) {
            return fnt_profont.MeasureString(text).ToXYPair();
        }
    }
}