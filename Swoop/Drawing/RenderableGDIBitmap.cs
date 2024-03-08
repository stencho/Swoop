using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SwoopLib {
    internal class RenderableGDIBitmap {
        System.Drawing.Graphics graphics;
        
        public System.Drawing.Bitmap bitmap;
        public Texture2D texture;

        bool texture_behind_bitmap = true;

        Action<System.Drawing.Graphics> draw_action;

        XYPair size;

        void setup_graphics() {
            graphics = System.Drawing.Graphics.FromImage(bitmap);

            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

        }

        public RenderableGDIBitmap(int width, int height) {
            size = new XYPair(width, height);
            bitmap = new System.Drawing.Bitmap(width, height);
            texture = new Texture2D(Drawing.graphics_device, size.X, size.Y, false, SurfaceFormat.Color);

            setup_graphics();
        }
        public RenderableGDIBitmap(int width, int height, Action<System.Drawing.Graphics> draw_action) {
            size = new XYPair(width, height);
            bitmap = new System.Drawing.Bitmap(width, height);
            texture = new Texture2D(Drawing.graphics_device, size.X, size.Y, false, SurfaceFormat.Color);

            setup_graphics();

            this.draw_action = draw_action;
        }

        public RenderableGDIBitmap(System.Drawing.Bitmap bitmap) {
            size = new XYPair(bitmap.Width, bitmap.Height);
            graphics = System.Drawing.Graphics.FromImage(bitmap);
            texture = new Texture2D(Drawing.graphics_device, size.X, size.Y, false, SurfaceFormat.Color);

            setup_graphics();
        }

        ~RenderableGDIBitmap() {
            graphics.Dispose();
        }

        public void clear_canvas() => graphics.FillRectangle(System.Drawing.Brushes.Transparent, 0, 0, bitmap.Width, bitmap.Height);
        public void clear_canvas(System.Drawing.Brush color) => graphics.FillRectangle(color, 0, 0, bitmap.Width, bitmap.Height);
        public void clear_canvas(System.Drawing.Color color) => graphics.FillRectangle(new System.Drawing.SolidBrush(color), 0, 0, bitmap.Width, bitmap.Height);

        public void update_canvas(bool copy_to_texture = false) {
            draw_action?.Invoke(graphics);
            if (texture_behind_bitmap) copy_bitmap_to_texture();
        }

        public void render_texture(SpriteBatch sb, int x, int y, int width, int height, Color color) {
            sb.Draw(texture, new Rectangle(x,y,width,height), Color.White);
        }
        //public void render_bitmap(SpriteBatch sb, int x, int y, int width, int height, Color color) {
            //Drawing.begin();
        //}

        public void copy_bitmap_to_texture() {
            uint trgb;

            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, size.X, size.Y), 
                System.Drawing.Imaging.ImageLockMode.ReadWrite, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Color[] cdata = new Color[size.X * size.Y];

            Debug.WriteLine("BITMAP");

            int i = 0;
            byte a, r, g, b;

            unsafe {
                uint* ptr = (uint*)data.Scan0;

                for (i = 0; i < size.X * size.Y; i++) {
                    cdata[i].A = (byte)(*(ptr + (i)) >> 24);
                    cdata[i].R = (byte)(*(ptr + (i)) >> 16);
                    cdata[i].G = (byte)(*(ptr + (i)) >> 8);
                    cdata[i].B = (byte)(*(ptr + (i)));
                }

                lock(texture) texture.SetData(cdata);

                bitmap.UnlockBits(data);

                cdata = null;
                ptr = null;
                GC.Collect();
            }

            texture_behind_bitmap = false;
        }        
    }    
}
