
using MGRawInputLib;

using SwoopLib;
using SwoopLib.UIElements;
using SwoopLib.Effects;

using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using ManagedBass;
using ManagedBass.Asio;

namespace NestEdit {
    public class NestEditGame : Game {
        GraphicsDeviceManager graphics;

        public static double target_fps = 60;
        FPSCounter fps;

        public static XYPair resolution = new XYPair(1000, 600);

        bool capture_demo_screenshot_on_exit = true;

        RenderTarget2D output_rt;
        
        UIElementManager UI => Swoop.UI;

        float[] _buffer;

        public NestEditGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            this.IsFixedTimeStep = true;
            this.InactiveSleepTime = System.TimeSpan.Zero;
            this.TargetElapsedTime = System.TimeSpan.FromMilliseconds(1000.0 / target_fps);            
        }

        protected override void Initialize() {
            Swoop.Initialize(this, graphics, Window, resolution, false);
            Window.Title = "NestEdit";

            Swoop.show_logo = false;
            Swoop.draw_UI_border = false;

            fps = new FPSCounter();
            this.Disposed += SwoopGame_Disposed;
            base.Initialize();
        }

        protected override void LoadContent() {
            output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            Swoop.Load(GraphicsDevice, graphics, Content, Window, false);
             
            build_UI();
        }

        protected void build_UI() {
            
            Swoop.resize_end = (XYPair size) => {
                resolution = Swoop.resolution;

                output_rt.Dispose();
                output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                graphics.PreferredBackBufferWidth = resolution.X;
                graphics.PreferredBackBufferHeight = resolution.Y;
                graphics.ApplyChanges();

                AutoRenderTarget.Manager.refresh_all();
            };

        }

        protected override void Update(GameTime gameTime) {
            float clock = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Swoop.Update(gameTime);

            string title_text = $"{UIExterns.get_window_title()}";

            if (Swoop.input_handler.is_pressed(Keys.Escape)) {
                if (capture_demo_screenshot_on_exit) {
                    output_rt.SaveAsPng(new FileStream("..\\..\\..\\current_nedit.png", FileMode.OpenOrCreate), resolution.X, resolution.Y);
                    capture_demo_screenshot_on_exit = false;
                }
                Swoop.End();
                this.Exit();
            }

            fps.update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            if (!Swoop.enable_draw ) {
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Swoop.UI_background_color);

                //swoop.DrawBackground();
                base.Draw(gameTime);
                return;
            }

            //draw the UI
            Swoop.Draw();

            GraphicsDevice.SetRenderTarget(output_rt);
            GraphicsDevice.Clear(Color.Transparent);
            //Draw the UI to an output RT
            //this is not required, but it makes screenshots easier
            Swoop.DrawBackground();
            AutoRenderTarget.Manager.draw_rts_to_target_background();
            Drawing.image(Swoop.render_target_output, XYPair.Zero, resolution);
            AutoRenderTarget.Manager.draw_rts_to_target_foreground();

            GraphicsDevice.SetRenderTarget(null);
            Drawing.image(output_rt, XYPair.Zero, resolution);

            //test.set_param("main_texture", Drawing.OnePXWhite);
            //test.draw_plane(Swoop.resolution);

            base.Draw(gameTime);
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            Swoop.End();
        }
    }
}