
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
using Microsoft.VisualBasic.Devices;

namespace SwoopDemo {
    public class SwoopGame : Game {
        GraphicsDeviceManager graphics;

        public static double target_fps = 250;
        FPSCounter fps;

        public static XYPair resolution = new XYPair(1000, 600);

        bool capture_demo_screenshot_on_exit = true;

        RenderTarget2D output_rt;
        
        UIElementManager UI => Swoop.UI;

        AutoRenderTarget render_target_bg;
        AutoRenderTarget render_target_fg;

        ShadedQuadWVP draw_shader;
        ShadedQuadWVP tint_effect;
        public SwoopGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferMultiSampling = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            this.IsFixedTimeStep = true;
            this.InactiveSleepTime = System.TimeSpan.Zero;
            this.TargetElapsedTime = System.TimeSpan.FromMilliseconds(1000.0 / target_fps);
        }

        protected override void Initialize() {
            Swoop.Initialize(this, graphics, Window, resolution);

            fps = new FPSCounter();
            this.Disposed += SwoopGame_Disposed;
            base.Initialize();
        }

        protected override void LoadContent() {
            output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            Swoop.Load(GraphicsDevice, graphics, Content, Window);
             
            build_UI();
        }

        float spinny = 0;
        void draw_gdi_canvas(System.Drawing.Graphics e_g) {
            e_g.Clear(Swoop.UI_background_color.ToGDIColor());

            e_g.DrawString("BAZINGA!", new System.Drawing.Font("BadaBoom BB", 24), new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 255, 0, 0)), Vector2.Zero.ToPointF());
            e_g.DrawString("❤🦶🦶👀👅", new System.Drawing.Font("Segoe UI Emoji", 24), System.Drawing.Brushes.White, (Vector2.UnitY * 25).ToPointF());

            spinny += (float)(90f * Swoop.game_time.ElapsedGameTime.TotalSeconds);
            if (spinny > 360f) spinny -= 360f;

            e_g.FillPie(System.Drawing.Brushes.LightGray, new System.Drawing.Rectangle(5, 75, 20, 20), -90f, 0f + spinny);
        }

        protected void build_UI() {
            render_target_bg = new AutoRenderTarget(
                resolution.X_only - (resolution.X_only / 4.5f) + ((UI.elements["title_bar"].height + 30) * XYPair.UnitY),
                resolution / 5, true);
            render_target_fg = new AutoRenderTarget(
                resolution.X_only - (resolution.X_only / 4.5f) + ((UI.elements["title_bar"].height + 167) * XYPair.UnitY),
                resolution / 5, true);

            UI.add_element(new GDICanvas("gdi_canvas", 
                resolution.X_only - (resolution.X_only / 4.5f) + ((UI.elements["title_bar"].height + 304) * XYPair.UnitY),
                resolution / 5, 
                draw_gdi_canvas));

            UI.add_element(new Label("gdi_label",
                "GDI+ drawing canvas, for some reason",
                UI.elements["gdi_canvas"].position - (XYPair.UnitY * 13)
                )); ;

            AutoRenderTarget.Manager.register_background_draw(render_target_bg);
            AutoRenderTarget.Manager.register_foreground_draw(render_target_fg);

            draw_shader = new ShadedQuadWVP(Swoop.content, "draw_2d");
            tint_effect = new ShadedQuadWVP(Swoop.content, "test_tint");

            UI.add_element(new Label("test_3d_label", "AutoRenderTarget tests\n3D plane > rt > background\nthis text is in front and\npart of the above label", (resolution.X_only - (resolution.X_only / 4.5f)) + ((UI.elements["title_bar"].height + 5) * XYPair.UnitY)));

            UI.add_element(new Button("test_custom_draw_button", (UIElement parent) => {
                Drawing.fill_rect(XYPair.Zero, parent.size, parent.mouse_over && parent.mouse_down ? Swoop.get_color(parent) : Swoop.UI_background_color);
                Drawing.image(Drawing.Logo, Vector2.Zero, Vector2.One * 28, SpriteEffects.None);
                Drawing.rect(XYPair.One, parent.size, Swoop.get_color(parent), 1f);
            }, UI.elements["test_3d_label"].position - (XYPair.UnitX * 30), XYPair.One * 28));
            UI.elements["test_custom_draw_button"].can_be_focused = false;

            UI.add_element(new Label(
                "test_3d_label_2", "shader > rt > foreground", 
                (resolution.X_only - (resolution.X_only / 4.5f)) + ((UI.elements["title_bar"].height + 155) * XYPair.UnitY)));
            UI.add_element(new Label(
                "test_3d_label_3", "this text is behind the top\nrt layer and being passed\nthrough via a shader which is\naware of each pixel's screen\nposition, so it can pull data\nfrom the main screen render\ntarget and tint it for example\nwheeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", 
                (resolution.X_only - (resolution.X_only / 4.5f)) + ((UI.elements["title_bar"].height + 175) * XYPair.UnitY) + (XYPair.UnitX * -20f)));
            
            //draw_shader.projection = Matrix.CreateOrthographic(2f, 2f, 0f, 5f);
            //draw_shader.projection = Matrix.CreateOrthographic(1f,1f, 0f, 5f);
            draw_shader.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f), render_target_bg.size.aspect_ratio, 0.01f, 5f);
            draw_shader.view = Matrix.CreateLookAt(Vector3.Backward * 1f, Vector3.Forward, Vector3.Up);

            draw_shader.world = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(45f)) * Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(25));
            //draw_shader.world = Matrix.CreateScale(trt.size.X, trt.size.Y, 1f);

            draw_shader.set_param("main_texture", Drawing.Logo);

            draw_shader.set_param("screen_pos_texture", render_target_fg.screen_pos_rt);
            draw_shader.set_param("screen_texture", Swoop.render_target_output);

            render_target_bg.draw = (XYPair position, XYPair size) => {
                Drawing.image(render_target_bg.screen_pos_rt, XYPair.Zero, render_target_bg.size);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullNone;

                draw_shader.set_param("main_texture", Drawing.Logo);
                draw_shader.draw_plane();

                Drawing.end();
                Drawing.rect(XYPair.One, render_target_bg.size, Swoop.UI_color, 1f);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                Drawing.graphics_device.DepthStencilState = DepthStencilState.Default;
            };

            render_target_fg.draw = (XYPair position, XYPair size) => {

                Drawing.end();

                tint_effect.set_param("tint", Swoop.UI_highlight_color.ToVector4());
                tint_effect.set_param("bg", Swoop.UI_background_color.ToVector4());
                tint_effect.set_param("screen_texture", Swoop.render_target_output);
                tint_effect.set_param("screen_pos_texture", render_target_fg.screen_pos_rt);

                tint_effect.draw_plane();

                Drawing.rect(XYPair.One, render_target_fg.size, Swoop.UI_color, 1f);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                Drawing.graphics_device.DepthStencilState = DepthStencilState.Default;
            };

            ((Button)UI.elements["exit_button"]).click_action = () => {
                if (capture_demo_screenshot_on_exit) {
                    output_rt.SaveAsPng(new FileStream("..\\..\\..\\current.png", FileMode.OpenOrCreate), resolution.X, resolution.Y);
                    capture_demo_screenshot_on_exit = false;
                }
                Swoop.End();
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

            Swoop.resize_end = (XYPair size) => {
                resolution = size;

                output_rt.Dispose();
                output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

                graphics.PreferredBackBufferWidth = resolution.X;
                graphics.PreferredBackBufferHeight = resolution.Y;
                graphics.ApplyChanges();

                AutoRenderTarget.Manager.refresh_all();
            };

            //current input handler label
            UI.add_element(new Label("ri_info_label",
                $"{Swoop.input_handler.ri_info()}",
                XYPair.One * 20f + (XYPair.UnitX * 300)));

            //gjk test panel
            UI.add_element(new GJKTestPanel("gjk_panel", (XYPair.One * 20) + (XYPair.UnitY * 100), XYPair.One * 200));

            var gjk_panel = UI.elements["gjk_panel"];

            UI.add_element(new Label("gjk_label", "GJK", gjk_panel.position + XYPair.Up * 15));


            //radio buttons
            UI.add_element(new Label("rb_label", "Radio Buttons", new XYPair(gjk_panel.right - 3, gjk_panel.Y - 100)));

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
            UI.add_element(new Label("pb_label", "Progress Bars",                   gjk_panel.bottom_xy + (XYPair.Down * 3f)));


            UI.add_element(new ProgressBar("progress_bar", 0.5f,                    gjk_panel.bottom_xy + (XYPair.Right * 100f) + (XYPair.Down * 30f), new XYPair(100, 10)));
            ((ProgressBar)UI.elements["progress_bar"]).text = "normal";


            UI.add_element(new ProgressBar("progress_bar_inverted", 0.5f,           gjk_panel.bottom_xy + (XYPair.Right * 100f) + (XYPair.Down * 53f), new XYPair(100, 10)));
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
            ((ProgressBar)UI.elements["progress_bar_clickable"]).value_changed = (float value) => {
                ((ProgressBar)UI.elements["progress_bar_clickable"]).text = $"clickable -> {value}";
            };

            ((ProgressBar)UI.elements["progress_bar_clickable"]).clickable = true;
            ((ProgressBar)UI.elements["progress_bar_clickable"]).text = "clickable";

            UI.add_element(new ProgressBar("progress_bar_clickable_inverted", 0.5f,
                ((ProgressBar)UI.elements["progress_bar_clickable"]).bottom_xy + (XYPair.Down * 12f),
                new XYPair(100, 10)));

            ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).invert = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).clickable = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).text = "inverted";
            ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).value_changed = (float value) => {
                ((ProgressBar)UI.elements["progress_bar_clickable_inverted"]).text = $"inverted -> {value}";
            };



            UI.add_element(new ProgressBar("progress_bar_clickable_vertical", 0.5f, UI.elements["progress_bar_vertical_inverted"].right_xy + (XYPair.UnitX * 15), new XYPair(10, 90)));
            ((ProgressBar)UI.elements["progress_bar_clickable_vertical"]).vertical = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_vertical"]).clickable = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_vertical"]).text = "clickable";

            UI.add_element(new ProgressBar("progress_bar_clickable_vertical_inverted", 0.5f, UI.elements["progress_bar_clickable_vertical"].right_xy + (XYPair.UnitX * 15), new XYPair(10, 90)));
            ((ProgressBar)UI.elements["progress_bar_clickable_vertical_inverted"]).vertical = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_vertical_inverted"]).invert = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_vertical_inverted"]).clickable = true;
            ((ProgressBar)UI.elements["progress_bar_clickable_vertical_inverted"]).text = "clickable inv";

            //ListBox
            UI.add_element(new Label("listbox_label", "ListBox", gjk_panel.right_xy + (XYPair.UnitX * 10) - (XYPair.UnitY * 15)));

            ListBox test_listbox = new ListBox("test_listbox", gjk_panel.right_xy + (XYPair.UnitX * 10) + (XYPair.UnitY * 1), new XYPair(200, 320));
            test_listbox.add_item(new ListBoxItem("test"));
            test_listbox.add_item(new ListBoxItem("a second test"));
            test_listbox.add_item(new ListBoxItem("a third test\nthis time it's multiline\nlol"));
            test_listbox.add_item(new ListBoxItem(50, (XYPair position, XYPair size) => {
                Drawing.fill_rect_dither(XYPair.Zero, size, Swoop.UI_background_color, Swoop.UI_color);
                Drawing.fill_rect(Vector2.One * 18, Vector2.One * 22 + Drawing.measure_string_profont_xy("BIG HEFTY CUSTOM DRAW"), Swoop.UI_background_color);
                Drawing.text("BIG HEFTY CUSTOM DRAW", Vector2.One * 20, Color.Red);
            }));

            Button test_listbox_add_button = new Button("rng_add_button", "Add Item", test_listbox.position + (XYPair.Up * 17) + (XYPair.Right * 75));
            test_listbox_add_button.click_action = () => {
                var lbi = new ListBoxItem(RandomNumberGenerator.GetHexString(RandomNumberGenerator.GetInt32(10, 55)));

                string t = lbi.text;

                int nl = RandomNumberGenerator.GetInt32(0, 3);

                for (int n = 0; n < nl; n++) {
                    int nl_pos = RandomNumberGenerator.GetInt32(t.Length - 2);
                    t = t.Insert(nl_pos, "\n");
                }
                lbi.text = t;
                
                test_listbox.add_item(lbi);
            };

            test_listbox.add_item(new ListBoxItem("here comes some random spam!"));
            UI.add_element(test_listbox);
            UI.add_element(test_listbox_add_button);

            UI.add_element(new TextBox("text_box", "this is a test textbox",
                UI.elements["test_listbox"].position + UI.elements["test_listbox"].size.X_only + (XYPair.Right * 8),
                ((UI.elements["test_listbox"].size * 0.75f) * (XYPair.UnitX * 2)) + (XYPair.UnitY * 19)
                ));

            //text editor
            UI.add_element(new TextEditor("text_editor", 
                "this is a literal actual text editor" +
                "\n" +
                "\nand this line is extra long for testing scrolling and word wrap" +
                "\nthis line is similarly excellently long for the testing of the word wrap and scrolling and it needs to be at least three lines maybe even four lines long" +
                "\n" +
                "\nI don't really know why I thought that this" +
                "\nwas a good idea but it did let me implement" +
                "\na fairly robust system for editing text" +        
                "\n" +
                "\nthe TextBox above actually uses the same system" +
                "\n" +
                "\nit is still missing a ton of features though, " +
                "\nword wrap still needs to be implemented because this is a nuisance" +
                "\n" +
                "\nthat said, you could probably still edit a small file with this" +
                "\n" +
                "\n" +
                "\n- CTRL + L to toggle line count" +
                "\n" +
                "\ntodo:" +
                "\nmouse support at all" +
                "\nscroll bars" +
                "\nword wrap" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\n" +
                "\nsecret" +
                "",                

                UI.elements["test_listbox"].position + UI.elements["test_listbox"].size.X_only + (XYPair.Right * 8) + (XYPair.Down * 25),
                UI.elements["test_listbox"].size * 0.75f * (XYPair.One + XYPair.Right)));

            UI.add_element(new OptionSlider("option_slider", 
                UI.elements["text_editor"].bottom_xy + (XYPair.UnitY * 10) + (UI.elements["text_editor"].size.X_only / 6), 
                UI.elements["text_editor"].size.X_only * 0.75f, 
                "Option Slider", 
                "No", "Low", "Medium", "High", "Ultra", "Yes"
                ));

            

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

            Swoop.Update(gameTime);
            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";
            string focus_info = $"{(UIElementManager.focused_element != null ? UIElementManager.focused_element.name : "")}";            

            ((TitleBar)UI.elements["title_bar"]).left_text = title_text;
            ((TitleBar)UI.elements["title_bar"]).right_text = FPS_text + " | " +  Input.poll_method;

            ((Label)UI.elements["ri_info_label"]).change_text(Swoop.input_handler.ri_info());

            if (Swoop.input_handler.is_pressed(Keys.Escape)) ((Button)UI.elements["exit_button"]).click_action();

            fps.update(gameTime);
            base.Update(gameTime);
        }

        ShadedQuad test;

        protected override void Draw(GameTime gameTime) {
            if (test == null) test = new ShadedQuad(Content, "draw_2d", XYPair.One * 100, XYPair.One * 100);
            

            if (!Swoop.enable_draw) {
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Swoop.UI_background_color);
                //swoop.DrawBackground();
                base.Draw(gameTime);
                return;
            }

            //update any ManagedEffects which are registered for updates
            ManagedEffect.Manager.do_updates();

            //draw the UI
            Swoop.Draw();

            //draw each of the bg/fg AutoRenderTargets
            AutoRenderTarget.Manager.draw_rts();

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