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
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SwoopLib.UIExterns;

namespace SwoopDemo {
    public class SwoopGame : Game {
        GraphicsDeviceManager graphics;

        public static double target_fps = 60;

        FPSCounter fps;

        public static XYPair resolution = new XYPair(800, 600);

        bool capture_demo_screenshot_on_exit = true;

        Texture2D logo;

        RenderTarget2D output_rt;

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

            Window.AllowUserResizing = true;
        }

        protected override void Initialize() {
            Swoop.Initialize(this, resolution);

            fps = new FPSCounter();
            this.Disposed += SwoopGame_Disposed;
            base.Initialize();
        }

        protected override void LoadContent() {
            output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y);

            Swoop.Load(GraphicsDevice, graphics, Content, resolution);

            logo = Content.Load<Texture2D>("swoop_logo");
             
            build_UI();
        }

        [DllImport("user32.dll", SetLastError = true)] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        protected void build_UI() {
            
            ((Button)UI.elements["exit_button"]).click_action = () => {
                if (capture_demo_screenshot_on_exit) {
                    output_rt.SaveAsPng(new FileStream("..\\..\\..\\..\\current.png", FileMode.OpenOrCreate), resolution.X, resolution.Y);
                    capture_demo_screenshot_on_exit = false;
                }
                Swoop.End();
                this.Exit();
            };


            UI.add_element(new Label("toggle_label", "a toggle button:", (XYPair.UnitY * 20) + (XYPair.UnitX * 15)));

            UI.add_element(new ToggleButton("toggle_button", "Toggled On", "Toggled Off", (XYPair.UnitY * 26) + (XYPair.UnitX * 155)));
            UI.elements["toggle_button"].anchor = UIElement.anchor_point.CENTER;


            UI.add_element(new Button("test_button",
                "this button should display a test dialog window",
                (XYPair.UnitY * 40) + (XYPair.UnitX * 15)));

            UI.add_element(new Checkbox("test_checkbox", "a checkbox",
                (XYPair.One * 20) + (XYPair.UnitY * 53)));

            UI.add_element(new Checkbox("test_checkbox_ml", "a multi-line-\ntext testing\ncheckbox",
                (XYPair.One * 20) + (XYPair.UnitY * 40) + (XYPair.UnitX * 89)));


            UI.add_element(new Button("behind_button",
                @"this button should be behind the test panel",
                (resolution.Y_only - (XYPair.UnitY * 94)) + (XYPair.UnitX * 5)));

            UI.add_element(new Panel("test_panel", (resolution.Y_only - (XYPair.UnitY * 150)) + (Vector2.UnitX * 30), new XYPair(400, 100),
                (Panel panel, UIElementManager sub_elements) => {
                    sub_elements.add_element(new Button("long_button",
                        "this is a really really really really really really long test button that should be far longer than the width of the panel\nunless you click this a bunch",
                        XYPair.One * 5f + (XYPair.UnitY * 17)));

                    ((Button)sub_elements.elements["long_button"]).click_action = () => {
                        UI.elements["test_panel"].size += XYPair.One * 20f;
                    };

                    sub_elements.add_element(new Label("label",
                        $"this label is a sub element of a Panel called \'{panel.name}\'",
                        XYPair.One * 5f));

                    sub_elements.add_element(new Label("behind_label",
                        "<- that button should always be behind this panel\n     (and only the visible portion should be clickable)",
                        XYPair.One * 2f + (XYPair.UnitY * 55)));
                })
            );


            var dialog_size = new XYPair(340, 160);
            ((Button)UI.elements["test_button"]).click_action = () => {
                UI.add_element(new Dialog("test_dialog",
                (resolution / 2) - (dialog_size * 0.5f),
                dialog_size,
                "test dialog title text",
                (Dialog td, UIElementManager sub_elements) => {
                    sub_elements.add_element(new Label("test_label", "this is a test label for testing\nall sorts of different text\n\nthis is a bit of extra text for testing\nabcdefghijklmnopqrstuvwxyz",
                        td.size / 2, Label.anchor_point.CENTER));

                    ((Label)sub_elements.elements["test_label"]).text_justification = Label.alignment.CENTER;

                    sub_elements.add_element(new Label("test_label_2", $"this is another test label with an outline\nalso this dialog is called {td.name}", XYPair.One * 10));
                    ((Label)sub_elements.elements["test_label_2"]).draw_outline = true;

                    sub_elements.add_element(new Button("close", "close", new XYPair(td.size.X / 2, td.size.Y - 25)));
                    sub_elements.elements["close"].anchor = UIElement.anchor_point.CENTER;
                    ((Button)sub_elements.elements["close"]).click_action = () => {
                        UI.remove_element(td.name);
                        UIElementManager.focused_element = UI.elements["test_button"];
                    };

                    UIElementManager.focused_element = sub_elements.elements["close"];
                }));
            };

            Swoop.resize_end = (XYPair size) => {
                resolution = size;

                output_rt.Dispose();
                output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y);

                graphics.PreferredBackBufferWidth = resolution.X;
                graphics.PreferredBackBufferHeight = resolution.Y;
                graphics.ApplyChanges();

            };

            UI.add_element(new Label("ri_info_label",
                $"{Swoop.input_handler.ri_info()}",
                XYPair.One * 20f + (XYPair.UnitX * 300)));

        }


        protected override void Update(GameTime gameTime) {
            Swoop.Update();
            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";
            string focus_info = $"{(UIElementManager.focused_element != null ? UIElementManager.focused_element.name : "")}";            

            ((TitleBar)UI.elements["title_bar"]).left_text = title_text;
            ((TitleBar)UI.elements["title_bar"]).right_text = FPS_text + " | " +  Input.poll_method;

            ((Label)UI.elements["ri_info_label"]).change_text(Swoop.input_handler.ri_info());

            fps.update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
           if (!Swoop.enable_draw) {
                GraphicsDevice.SetRenderTarget(null);
                if (Swoop.fill_background) {
                    Drawing.graphics_device.Clear(Swoop.UI_background_color);
                } else {
                    Drawing.graphics_device.Clear(Color.Transparent);
                }

                Drawing.graphics_device.Clear(Swoop.UI_background_color);

                base.Draw(gameTime);
                return;
            }
           
            Swoop.Draw();

            Drawing.graphics_device.SetRenderTarget(output_rt);

            GraphicsDevice.Clear(Swoop.UI_background_color);
            
            Drawing.image(logo, 
                (resolution.ToVector2()) - (logo.Bounds.Size.ToVector2() * 0.5f) - (Vector2.One * 8f), 
                logo.Bounds.Size.ToVector2() * 0.5f,
                SpriteEffects.FlipHorizontally);
            Drawing.image(Swoop.render_target_output, XYPair.Zero, resolution);

            Drawing.end();


            GraphicsDevice.SetRenderTarget(null);
            Drawing.image(output_rt, XYPair.Zero, resolution);

            base.Draw(gameTime);
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            Swoop.End();
        }
    }
}