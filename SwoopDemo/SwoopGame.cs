using System;
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
using System.ComponentModel;
using System.Linq;
using System.IO;

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

        bool capture_demo_screenshot_on_startup = true;

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
            Swoop.Load(GraphicsDevice, graphics, Content, resolution);
            build_UI();
        }

        protected void build_UI() {
            var text_length = Drawing.measure_string_profont("x") ;

            UI.add_element(new Button("exit_button", "x",
                resolution.ToVector2().X_only() - text_length.X_only() - (Vector2.UnitX * 10f)));
            UI.elements["exit_button"].ignore_dialog = true;
            UI.elements["exit_button"].can_be_focused = false;

            UI.add_element(new Button("minimize_button", "_", 
                    resolution.ToVector2().X_only() - ((text_length.X_only() + (Vector2.UnitX * 9f)) * 2), 
                    UI.elements["exit_button"].size));
            UI.elements["minimize_button"].ignore_dialog = true;
            UI.elements["minimize_button"].can_be_focused = false;

            ((Button)UI.elements["exit_button"]).click_action = () => {
                if (capture_demo_screenshot_on_startup) {
                    Swoop.render_target_output.SaveAsPng(new FileStream("..\\..\\..\\..\\current.png", FileMode.OpenOrCreate), resolution.X, resolution.Y);
                    capture_demo_screenshot_on_startup = false;
                }
                Swoop.End();
                this.Exit();
            };

            ((Button)UI.elements["minimize_button"]).click_action = () => {
                UIExterns.minimize_window();
            };

            UI.add_element(new TitleBar("title_bar", 
                Vector2.Zero, (int)(resolution.X - (UI.elements["exit_button"].width*2)) + 3));
            UI.elements["title_bar"].ignore_dialog = true;

            UI.add_element(new Button("big_ol_test_button", 
                @"this is a really long test string to put on the button to test a thing
It's also gonna be a couple of lines long just to make sure everything works",
                Vector2.One * 20 + (Vector2.UnitY * 300)));

            var dialog_size = new Vector2(340, 200);
            UI.add_element(new Dialog("test_dialog",
                (resolution.ToVector2() / 2) - (dialog_size * 0.5f),
                dialog_size,
                "test dialog title text",
                (Dialog td, UIElementManager sub_elements) => {
                    sub_elements.add_element(new Label("test_label", "this is a test label for testing\nall sorts of different text\n\nthis is a bit of extra text for testing\nabcdefghijklmnopqrstuvwxyz",
                        td.size / 2, Label.anchor_point.CENTER));

                    ((Label)sub_elements.elements["test_label"]).text_justification = Label.alignment.CENTER;
                    ((Label)sub_elements.elements["test_label"]).draw_outline = true;

                    sub_elements.add_element(new Label("test_label_2", $"this is another test label with an outline\nalso this dialog is called {td.name}", Vector2.One * 10));
                    ((Label)sub_elements.elements["test_label_2"]).draw_outline = true;

                    sub_elements.add_element(new Button("close", "close", new Vector2(td.size.X / 2, td.size.Y - 40)));
                    ((Button)sub_elements.elements["close"]).click_action = () => {
                        UI.remove_element(td.name);
                    };

                    UIElementManager.focused_element = sub_elements.elements["close"];
                }));
            UI.dialog_element = "test_dialog";

            UI.add_element(new Panel("test_panel", Vector2.One * 60, Vector2.One * 500, 
                (Panel panel, UIElementManager sub_elements) => {
                    sub_elements.add_element(new Button("long_button", 
                        "this is a really really really really really really long test button that should be far longer than the width of the panel", 
                        Vector2.One * 5f + (Vector2.UnitY * 25)));

                    ((Button)sub_elements.elements["long_button"]).click_action = () => {
                        UI.add_element(new Dialog("test_dialog",
                        (resolution.ToVector2() / 2) - (dialog_size * 0.5f),
                        dialog_size,
                        "test dialog title text",
                        (Dialog td, UIElementManager sub_elements) => {
                            sub_elements.add_element(new Label("test_label", "this is a test label for testing\nall sorts of different text\n\nthis is a bit of extra text for testing\nabcdefghijklmnopqrstuvwxyz",
                                td.size / 2, Label.anchor_point.CENTER));

                            ((Label)sub_elements.elements["test_label"]).text_justification = Label.alignment.CENTER;

                            sub_elements.add_element(new Label("test_label_2", $"this is another test label with an outline\nalso this dialog is called {td.name}", Vector2.One * 10));
                            ((Label)sub_elements.elements["test_label_2"]).draw_outline = true;

                            sub_elements.add_element(new Button("close", "close", new Vector2(td.size.X / 2, td.size.Y - 40)));
                            ((Button)sub_elements.elements["close"]).click_action = () => {
                                UI.remove_element(td.name);
                                UIElementManager.focused_element = UI.elements["test_panel"].sub_elements.elements["long_button"];
                            };

                            UIElementManager.focused_element = sub_elements.elements["close"];
                        }));
                    };

                    sub_elements.add_element(new Label("label",
                        $"this label is a sub_element of a Panel called \'{panel.name}\'", 
                        Vector2.One * 5f));
                })
            );

            UI.add_element(new ToggleButton("toggle_button", (Vector2.UnitY * 26) + (Vector2.UnitX * 50)));
            UI.elements["toggle_button"].anchor = UIElement.anchor_point.CENTER;
        }

        protected override void Update(GameTime gameTime) {
            Swoop.Update();
            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";
            string focus_info = $"{(UIElementManager.focused_element != null ? UIElementManager.focused_element.name : "")}";            

            ((TitleBar)UI.elements["title_bar"]).left_text = title_text;
            ((TitleBar)UI.elements["title_bar"]).right_text = FPS_text + " " + focus_info + " " +  Input.poll_method;

            fps.update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Transparent);

            Swoop.Draw();

            Drawing.graphics_device.SetRenderTarget(null);
            Drawing.image(Swoop.render_target_output, Vector2.Zero, resolution);

            Drawing.end();

            base.Draw(gameTime);
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            Swoop.End();
        }
    }
}