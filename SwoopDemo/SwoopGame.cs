﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SwoopLib;
using SwoopLib.UIElements;
using MGRawInputLib;
using static SwoopLib.Swoop;

namespace SwoopDemo {
    static class Extensions {
        public static Vector2 X_only(this Vector2 v) => new Vector2(v.X, 0);
        public static Vector2 Y_only(this Vector2 v) => new Vector2(0, v.Y);
        public static Point X_only(this Point p) => new Point(p.X, 0);
        public static Point Y_only(this Point p) => new Point(0, p.Y);
    }

    public class SwoopGame : Game {
        GraphicsDeviceManager graphics;

        public static double target_fps = 60;
        static float title_bar_height = 16f;

        FPSCounter fps;

        public static Point resolution = new Point(850, 600);

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
            
            graphics.PreferredBackBufferWidth = resolution.X;
            graphics.PreferredBackBufferHeight = resolution.Y;

            graphics.ApplyChanges();
        }

        protected override void Initialize() {
            Swoop.Initialize(this, resolution);

            fps = new FPSCounter();
            this.Disposed += SwoopGame_Disposed;
            base.Initialize();
        }

        protected override void LoadContent() {
            Swoop.Load(GraphicsDevice, graphics, Content);
            build_UI();
        }

        protected void build_UI() {
            var text_length = Drawing.measure_string_profont("x") ;

            UI.add_element("exit_button", 
                new Button("x", resolution.ToVector2().X_only() - text_length.X_only() - (Vector2.UnitX * 10f)));
            UI.elements["exit_button"].ignore_dialog = true;

            UI.add_element("minimize_button", 
                new Button("_", 
                    resolution.ToVector2().X_only() - ((text_length.X_only() + (Vector2.UnitX * 10f)) * 2), 
                    UI.elements["exit_button"].size));
            UI.elements["minimize_button"].ignore_dialog = true;

            ((Button)UI.elements["exit_button"]).click_action = () => {
                Swoop.End();
                this.Exit();
            };

            ((Button)UI.elements["minimize_button"]).click_action = () => {
                UIExterns.minimize_window();
            };

            UI.add_element("title_bar", 
                new TitleBar(Vector2.Zero, (int)(resolution.X - (UI.elements["exit_button"].width*2))));
            UI.elements["title_bar"].ignore_dialog = true;

            UI.add_element("big_ol_test_button", 
                new Button("this is a really long test string to put on the button to test a thing",
                Vector2.One * 20 + (Vector2.UnitY * 300)));

            var dialog_size = new Vector2(340, 200);
            UI.add_element("test_dialog",
                new Dialog((resolution.ToVector2() / 2) - (dialog_size * 0.5f), 
                dialog_size, 
                "test dialog title text",
                (Dialog td, UIElementManager sub_elements) => {
                    sub_elements.add_element("test_label", new Label("this is a test label for testing\nall sorts of different text\n\nthis is a bit of extra text for testing\nabcdefghijklmnopqrstuvwxyz",
                        td.size / 2, Label.anchor_point.CENTER));

                    ((Label)sub_elements.elements["test_label"]).draw_outline = true;
                    ((Label)sub_elements.elements["test_label"]).text_justification = Label.alignment.CENTER;

                    sub_elements.add_element("test_label_2", new Label("this another test", Vector2.One * 10));
                    ((Label)sub_elements.elements["test_label_2"]).draw_outline = true;

                    sub_elements.add_element("close", new Button("close", new Vector2(td.size.X / 2, td.size.Y - 40)));
                    ((Button)sub_elements.elements["close"]).click_action = () => {
                        UI.remove_element(td.name);
                    };
                }));
            UI.dialog_element = "test_dialog";
        }

        protected override void Update(GameTime gameTime) {
            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";

            ((TitleBar)UI.elements["title_bar"]).left_text = title_text;
            ((TitleBar)UI.elements["title_bar"]).right_text = FPS_text;

            Swoop.Update();

            fps.update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            Swoop.Draw();

            Drawing.rect(Vector2.UnitX, (Vector2.UnitY * title_bar_height) + (Vector2.UnitX * resolution.X), Color.White, 1f);
            Drawing.rect(Vector2.Zero, resolution.ToVector2(), Color.White, 2f);

            Drawing.end();
            base.Draw(gameTime);
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            Swoop.End();
        }
    }
}