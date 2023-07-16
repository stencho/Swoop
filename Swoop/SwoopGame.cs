using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MGRawInputLib;
using Swoop.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Swoop {
    static class Extensions {
        public static Vector2 X_only(this Vector2 v) => new Vector2(v.X, 0);
        public static Vector2 Y_only(this Vector2 v) => new Vector2(0, v.Y);
    }

    public class SwoopGame : Game {
        GraphicsDeviceManager graphics;
        public static double target_fps = 60;
        static float title_bar_height = 16f;

        FPSCounter fps;

        public static Vector2 resolution = new Vector2(850, 600);

        UIElementManager ui;

        public SwoopGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            this.IsFixedTimeStep = true;
            this.InactiveSleepTime = System.TimeSpan.Zero;
            this.TargetElapsedTime = System.TimeSpan.FromMilliseconds(1000.0 / target_fps);

            Window.IsBorderless = true;
            Window.Title = "Swoop";

            graphics.PreferMultiSampling = false;
            graphics.SynchronizeWithVerticalRetrace = true;
            
            graphics.PreferredBackBufferWidth = (int)resolution.X;
            graphics.PreferredBackBufferHeight = (int)resolution.Y;

            graphics.ApplyChanges();
        }

        protected override void Initialize() {
            Input.initialize(this);

            ui = new UIElementManager(Vector2.Zero,resolution);
            
            fps = new FPSCounter();
            this.Disposed += SwoopGame_Disposed;
            base.Initialize();
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            Input.end();
        }

        protected override void LoadContent() {
            Drawing.load(GraphicsDevice, graphics, Content);
            build_UI();
            SDF.load(Content);
        }

        protected void build_UI() {
            var s = new Vector2(340, 200);

            var tl = Drawing.measure_string_profont("x") ;
            ui.add_element("exit_button", new Button("x", resolution.X_only() - tl.X_only() - (Vector2.UnitX * 10f)));
            ui.elements["exit_button"].ignore_dialog = true;
            ui.add_element("minimize_button", new Button("_", resolution.X_only() - ((tl.X_only() + (Vector2.UnitX * 10f)) * 2), ui.elements["exit_button"].size));
            ui.elements["minimize_button"].ignore_dialog = true;

            ((Button)ui.elements["exit_button"]).click_action = () => {
                Input.end();
                this.Exit();
            };
            ((Button)ui.elements["minimize_button"]).click_action = () => {
                UIExterns.minimize_window();
            };

            ui.add_element("title_bar", new TitleBar(Vector2.Zero, new Vector2(resolution.X - ((ui.elements["exit_button"].width*2)), title_bar_height)));
            ui.elements["title_bar"].ignore_dialog = true;

            ui.add_element("big_ol_test_button", new Button("this is a really long test string to put on the button to test a thing for a moment", Vector2.One * 20 + (Vector2.UnitY * 300)));

            ui.add_element("test_dialog", new Dialog((resolution * 0.5f) - (s * 0.5f), s, "test dialog title text"));
            ui.dialog_element = "test_dialog";
        }

        protected override void Update(GameTime gameTime) {
            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";

            ((TitleBar)ui.elements["title_bar"]).left_text = title_text;
            ((TitleBar)ui.elements["title_bar"]).right_text = FPS_text;

            ui.update();

            fps.update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            ui.draw();

            Drawing.rect(Vector2.UnitX, (Vector2.UnitY * title_bar_height) + (Vector2.UnitX * resolution.X), Color.White, 1f);
            Drawing.rect(Vector2.Zero, resolution, Color.White, 2f);

            //Drawing.text($"[{Input.RAWINPUT_DEBUG_STRING}]", Vector2.UnitX * 200 + (Vector2.One * 3f), Color.White);

            Drawing.end();
            base.Draw(gameTime);
        }


    }
}