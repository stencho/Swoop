
using MGRawInputLib;

using SwoopLib;
using SwoopLib.UIElements;
using swoop = SwoopLib.Swoop;
using static SwoopLib.UIExterns;
using SwoopLib.Effects;

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

        public static double target_fps = 240;
        FPSCounter fps;

        public static XYPair resolution = new XYPair(800, 600);

        bool capture_demo_screenshot_on_exit = true;


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
             
            build_UI();
        }

        [DllImport("user32.dll", SetLastError = true)] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        AutoRenderTarget render_target_bg;
        AutoRenderTarget render_target_fg;
        DrawShaded3DPlane draw_shader;
        DrawShaded3DPlane draw_shader_passthrough;
        protected void build_UI() {
            render_target_bg = new AutoRenderTarget(resolution.X_only - (resolution.X_only / 4.5f) + ((UI.elements["title_bar"].height + 30) * XYPair.UnitY), resolution / 5);
            render_target_fg = new AutoRenderTarget(resolution.X_only - (resolution.X_only / 4.5f) + ((UI.elements["title_bar"].height + 167) * XYPair.UnitY), resolution / 5);

            AutoRenderTarget.Manager.register_background_draw(render_target_bg);
            AutoRenderTarget.Manager.register_foreground_draw(render_target_fg);

            draw_shader = new DrawShaded3DPlane(Swoop.content, "draw_2d");
            draw_shader_passthrough = new DrawShaded3DPlane(Swoop.content, "test_tint");

            UI.add_element(new Label("test_3d_label", "auto render target tests\n3D plane > rt > background\n this text is in front and\n part of the above label", (resolution.X_only - (resolution.X_only / 4.5f)) + ((UI.elements["title_bar"].height + 5) * XYPair.UnitY)));

            UI.add_element(new Label("test_3d_label_2", "shader > rt > foreground", (resolution.X_only - (resolution.X_only / 4.5f)) + ((UI.elements["title_bar"].height + 155) * XYPair.UnitY)));
            UI.add_element(new Label("test_3d_label_3", "this text is behind the top\nrt layer and being passed\nthrough via a shader which is\naware of each pixel's screen\nposition, so it can pull data\nfrom the main screen render\ntarget and tint it for example\nwheeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", 
                (resolution.X_only - (resolution.X_only / 4.5f)) + ((UI.elements["title_bar"].height + 175) * XYPair.UnitY) + (XYPair.UnitX * -20f)));

            //draw_shader.projection = Matrix.CreateOrthographic(2f, 2f, 0f, 5f);
            //draw_shader.projection = Matrix.CreateOrthographic(1f,1f, 0f, 5f);
            draw_shader.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f), render_target_bg.size.aspect_ratio, 0.01f, 5f);
            draw_shader.view = Matrix.CreateLookAt(Vector3.Backward * 1f, Vector3.Forward, Vector3.Up);

            draw_shader.world = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(45f)) * Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(25));
            //draw_shader.world = Matrix.CreateScale(trt.size.X, trt.size.Y, 1f);

            draw_shader.set_param("main_texture", Drawing.Logo);

            draw_shader.set_param("screen_pos_texture", render_target_fg.screen_pos_rt);
            draw_shader.set_param("screen_texture", swoop.render_target_output);

            render_target_bg.draw = () => {
                Drawing.graphics_device.Clear(Color.Transparent);

                Drawing.image(render_target_bg.screen_pos_rt, XYPair.Zero, render_target_bg.size);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullNone;

                draw_shader.draw_plane();

                Drawing.end();
                Drawing.rect(XYPair.One, render_target_bg.size, Swoop.UI_color, 1f);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                Drawing.graphics_device.DepthStencilState = DepthStencilState.Default;
            };

            render_target_fg.draw = () => {
                Drawing.graphics_device.Clear(Swoop.UI_background_color);

                //Drawing.image(render_target_fg.screen_pos_rt, XYPair.Zero, render_target_fg.size);

                //Drawing.graphics_device.RasterizerState = RasterizerState.CullNone;

                Drawing.end();
                Drawing.begin(draw_shader.effect);
                draw_shader_passthrough.set_param("tint", Swoop.UI_highlight_color.ToVector3());
                draw_shader_passthrough.set_param("screen_texture", swoop.render_target_output);
                draw_shader_passthrough.set_param("screen_pos_texture", render_target_fg.screen_pos_rt);
                draw_shader_passthrough.draw_plane();
                //Drawing.fill_rect(Vector2.Zero, render_target_fg.size.ToVector2(), Color.White);
                //draw_shader.set_param("texture", trt.screen_pos_rt);
                //draw_shader.draw_plane();

                Drawing.end();
                Drawing.rect(XYPair.One, render_target_fg.size, Swoop.UI_color, 1f);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                Drawing.graphics_device.DepthStencilState = DepthStencilState.Default;
            };

            ((Button)UI.elements["exit_button"]).click_action = () => {
                if (capture_demo_screenshot_on_exit) {
                    output_rt.SaveAsPng(new FileStream("..\\..\\..\\current.png", FileMode.OpenOrCreate), resolution.X, resolution.Y);
                    capture_demo_screenshot_on_exit = false;
                }
                swoop.End();
                this.Exit();
            };


            UI.add_element(new Label("toggle_label", "a toggle button:", (XYPair.UnitY * 25) + (XYPair.UnitX * 15)));

            UI.add_element(new ToggleButton("toggle_button", "Toggled On", "Toggled Off", (XYPair.UnitY * 31) + (XYPair.UnitX * 155)));
            UI.elements["toggle_button"].anchor = UIElement.anchor_point.CENTER;


            UI.add_element(new Button("test_button",
                "display a test dialog window",
                (XYPair.UnitY * 45) + (XYPair.UnitX * 15)));

            UI.add_element(new Checkbox("test_checkbox", "a checkbox",
                (XYPair.One * 20) + (XYPair.UnitY * 63)));

            UI.add_element(new Checkbox("test_checkbox_ml", "a multi-line-\ntext testing\ncheckbox",
                (XYPair.One * 20) + (XYPair.UnitY * 50) + (XYPair.UnitX * 89)));


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

            UI.add_element(new Label("panel_label", "panel", UI.elements["test_panel"].position + XYPair.Up * 15));

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

                AutoRenderTarget.Manager.refresh_all();
            };

            //current input handler label
            UI.add_element(new Label("ri_info_label",
                $"{swoop.input_handler.ri_info()}",
                XYPair.One * 20f + (XYPair.UnitX * 300)));

            //gjk test panel
            UI.add_element(new GJKTestPanel("gjk_panel", (XYPair.One * 20) + (XYPair.UnitY * 100), XYPair.One * 200));

            var gjk_panel = UI.elements["gjk_panel"];

            UI.add_element(new Label("gjk_label", "gjk", gjk_panel.position + XYPair.Up * 15));


            //radio buttons
            UI.add_element(new Label("rb_label", "radio buttons", new XYPair(gjk_panel.right - 3, gjk_panel.Y - 100)));

            RadioButton radio_button_a1 = new RadioButton("rba1", "a1", new XYPair(gjk_panel.right, gjk_panel.Y -80 ));
            RadioButton radio_button_a2 = new RadioButton("rba2", "a2", new XYPair(gjk_panel.right, gjk_panel.Y -80 + 20));
            RadioButton radio_button_a3 = new RadioButton("rba3", "a3", new XYPair(gjk_panel.right, gjk_panel.Y -80 + 40));

            RadioButtons.link(radio_button_a1, radio_button_a2, radio_button_a3);


            RadioButton radio_button_b1 = new RadioButton("rbb1", "b1", new XYPair(gjk_panel.right + 40, gjk_panel.Y - 80 ));
            RadioButton radio_button_b2 = new RadioButton("rbb2", "b2", new XYPair(gjk_panel.right + 40, gjk_panel.Y - 80 + 20));
            RadioButton radio_button_b3 = new RadioButton("rbb3", "b3", new XYPair(gjk_panel.right + 40, gjk_panel.Y - 80 + 40));
                                                                                                                       
            RadioButtons.link(radio_button_b1, radio_button_b2, radio_button_b3);

            UI.add_elements(radio_button_a1, radio_button_a2, radio_button_a3);
            UI.add_elements(radio_button_b1, radio_button_b2, radio_button_b3);


            //progress bars
            UI.add_element(new Label("pb_label", "progress bars",                   gjk_panel.bottom_xy + (XYPair.Down * 3f)));


            UI.add_element(new ProgressBar("progress_bar", 0.5f,                    gjk_panel.bottom_xy + (XYPair.Right * 50f) + (XYPair.Down * 30f), new XYPair(100, 10)));
            ((ProgressBar)UI.elements["progress_bar"]).text = "normal";
            UI.add_element(new ProgressBar("progress_bar_inverted", 0.5f,           gjk_panel.bottom_xy + (XYPair.Right * 50f) + (XYPair.Down * 53f), new XYPair(100, 10)));
            ((ProgressBar)UI.elements["progress_bar_inverted"]).text = "inverted";
            ((ProgressBar)UI.elements["progress_bar_inverted"]).invert = true;

            UI.add_element(new ProgressBar("progress_bar_vertical", 0.5f,           gjk_panel.bottom_xy + (XYPair.Right * -5f) + (XYPair.Down * 23f), new XYPair(10, 90)));
            ((ProgressBar)UI.elements["progress_bar_vertical"]).text = "normal";

            UI.add_element(new ProgressBar("progress_bar_vertical_inverted", 0.5f,  gjk_panel.bottom_xy + (XYPair.Right * 5f) + (XYPair.Down * 23f) + (XYPair.Right * 15f), new XYPair(10, 90)));
            ((ProgressBar)UI.elements["progress_bar_vertical_inverted"]).text = "inverted";

            ((ProgressBar)UI.elements["progress_bar_vertical"]).vertical = true;

            ((ProgressBar)UI.elements["progress_bar_vertical_inverted"]).invert = true;
            ((ProgressBar)UI.elements["progress_bar_vertical_inverted"]).vertical = true;




            UI.add_element(new ProgressBar("progress_bar_clickable", 0.5f,
                ((ProgressBar)UI.elements["progress_bar_inverted"]).bottom_xy + (XYPair.Down * 17f),
                new XYPair(100, 10)));

            ((ProgressBar)UI.elements["progress_bar_clickable"]).clickable = true;
            ((ProgressBar)UI.elements["progress_bar_clickable"]).text = "clickable";

            UI.add_element(new ProgressBar("progress_bar_clickable_inverted", 0.5f,
                ((ProgressBar)UI.elements["progress_bar_clickable"]).bottom_xy + (XYPair.Down * 12f),
                new XYPair(100, 10)));

            ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).invert = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).clickable = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).text = "inverted";
        }

        static float progress_bar_test_value = 0.5f;

        protected override void Update(GameTime gameTime) {
            float clock = (float)gameTime.ElapsedGameTime.TotalSeconds;
            progress_bar_test_value += 0.25f * clock;
            if (progress_bar_test_value > 1f)
                progress_bar_test_value -= 1f;

            ((ProgressBar)UI.elements["progress_bar"]).value = progress_bar_test_value;
            ((ProgressBar)UI.elements["progress_bar_inverted"]).value = progress_bar_test_value;

            ((ProgressBar)UI.elements["progress_bar_vertical"]).value = progress_bar_test_value;
            ((ProgressBar)UI.elements["progress_bar_vertical_inverted"]).value = progress_bar_test_value;

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
                swoop.DrawBackground();
                base.Draw(gameTime);
                return;
            }


            AutoRenderTarget.Manager.draw_rts();

            swoop.DrawBackground();

            Drawing.graphics_device.SetRenderTarget(swoop.render_target_output);

            ManagedEffect.Manager.do_updates();
            AutoRenderTarget.Manager.draw_rts_to_target_early();

            swoop.Draw();

            Drawing.graphics_device.SetRenderTarget(output_rt);


            Drawing.image(swoop.render_target_output, XYPair.Zero, resolution);


            Drawing.end();


            AutoRenderTarget.Manager.draw_rts_to_target_late();
            GraphicsDevice.SetRenderTarget(null);
            Drawing.image(output_rt, XYPair.Zero, resolution);




            base.Draw(gameTime);
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            swoop.End();
        }
    }
}