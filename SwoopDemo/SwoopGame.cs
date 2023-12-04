
using MGRawInputLib;

using SwoopLib;
using SwoopLib.UIElements;
using swoop = SwoopLib.Swoop;
using static SwoopLib.UIExterns;

using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.IO;
using System.Runtime.InteropServices;

namespace SwoopDemo {
    public class SwoopGame : Game {
        GraphicsDeviceManager graphics;

        public static double target_fps = 60;
        FPSCounter fps;

        public static XYPair resolution = new XYPair(800, 600);

        bool capture_demo_screenshot_on_exit = true;

        Texture2D logo;

        RenderTarget2D output_rt;

        SwoopLib.UIElementManager UI => swoop.UI;

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
            swoop.Initialize(this, resolution);
            fps = new FPSCounter();
            this.Disposed += SwoopGame_Disposed;
            base.Initialize();
        }

        protected override void LoadContent() {
            output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y);

            swoop.Load(GraphicsDevice, graphics, Content, resolution);

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
                swoop.End();
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
                        UIElementManager.focus_element(UI, "test_button");
                    };

                    UIElementManager.focus_element(sub_elements, "close");
                }));
            };

            swoop.resize_end = (XYPair size) => {
                resolution = size;

                output_rt.Dispose();
                output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y);

                graphics.PreferredBackBufferWidth = resolution.X;
                graphics.PreferredBackBufferHeight = resolution.Y;
                graphics.ApplyChanges();

            };

            UI.add_element(new Label("ri_info_label",
                $"{swoop.input_handler.ri_info()}",
                XYPair.One * 20f + (XYPair.UnitX * 300)));

            UI.add_element(new GJKTestPanel("gjk_panel", (XYPair.One * 20) + (XYPair.UnitY * 100), XYPair.One * 200));

            UI.add_element(new Label("rb_label", "radio buttons", new XYPair(UI.elements["gjk_panel"].right + 10, UI.elements["gjk_panel"].Y)));

            RadioButton radio_button_a1 = new RadioButton("rba1", "a1", new XYPair(UI.elements["gjk_panel"].right + 15, UI.elements["gjk_panel"].Y + 20 ));
            RadioButton radio_button_a2 = new RadioButton("rba2", "a2", new XYPair(UI.elements["gjk_panel"].right + 15, UI.elements["gjk_panel"].Y + 20 + 20));
            RadioButton radio_button_a3 = new RadioButton("rba3", "a3", new XYPair(UI.elements["gjk_panel"].right + 15, UI.elements["gjk_panel"].Y + 20 + 40));

            RadioButtons.link(radio_button_a1, radio_button_a2, radio_button_a3);


            RadioButton radio_button_b1 = new RadioButton("rbb1", "b1", new XYPair(UI.elements["gjk_panel"].right + 60, UI.elements["gjk_panel"].Y + 20 ));
            RadioButton radio_button_b2 = new RadioButton("rbb2", "b2", new XYPair(UI.elements["gjk_panel"].right + 60, UI.elements["gjk_panel"].Y + 20 + 20));
            RadioButton radio_button_b3 = new RadioButton("rbb3", "b3", new XYPair(UI.elements["gjk_panel"].right + 60, UI.elements["gjk_panel"].Y + 20 + 40));
                                                                                                                       
            RadioButtons.link(radio_button_b1, radio_button_b2, radio_button_b3);

            UI.add_elements(radio_button_a1, radio_button_a2, radio_button_a3);
            UI.add_elements(radio_button_b1, radio_button_b2, radio_button_b3);
        }


        protected override void Update(GameTime gameTime) {
            swoop.Update();
            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";
            string focus_info = $"{(UIElementManager.focused_element != null ? UIElementManager.focused_element.name : "")}";            

            ((TitleBar)UI.elements["title_bar"]).left_text = title_text;
            ((TitleBar)UI.elements["title_bar"]).right_text = FPS_text + " | " +  Input.poll_method;

            ((Label)UI.elements["ri_info_label"]).change_text(swoop.input_handler.ri_info());

            if (swoop.input_handler.is_pressed(Keys.Escape)) ((Button)UI.elements["exit_button"]).click_action();

            fps.update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
           if (!swoop.enable_draw) {
                GraphicsDevice.SetRenderTarget(null);
                if (swoop.fill_background) {
                    Drawing.graphics_device.Clear(swoop.UI_background_color);
                } else {
                    Drawing.graphics_device.Clear(Color.Transparent);
                }

                Drawing.graphics_device.Clear(swoop.UI_background_color);

                base.Draw(gameTime);
                return;
            }

            swoop.Draw();

            Drawing.graphics_device.SetRenderTarget(output_rt);

            GraphicsDevice.Clear(swoop.UI_background_color);
            
            Drawing.image(logo, 
                (resolution.ToVector2()) - (logo.Bounds.Size.ToVector2() * 0.5f) - (Vector2.One * 8f), 
                logo.Bounds.Size.ToVector2() * 0.5f,
                SpriteEffects.FlipHorizontally);
            Drawing.image(swoop.render_target_output, XYPair.Zero, resolution);

            Drawing.end();


            GraphicsDevice.SetRenderTarget(null);
            Drawing.image(output_rt, XYPair.Zero, resolution);

            base.Draw(gameTime);
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            swoop.End();
        }
    }
}