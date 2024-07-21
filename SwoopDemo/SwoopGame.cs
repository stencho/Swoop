
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
using System.Diagnostics;
using System.Collections.Generic;
namespace SwoopDemo {
    public class SwoopGame : Game {
        GraphicsDeviceManager graphics;

        public static double target_fps = 60;
        FPSCounter fps;

        public static XYPair resolution = new XYPair(1050, 830);

        bool capture_demo_screenshot_on_exit = true;

        RenderTarget2D output_rt;
        
        UIElementManager UI => Swoop.UI;

        AutoRenderTarget render_target_bg;
        AutoRenderTarget render_target_fg;

        ShadedQuadWVP draw_shader;
        ShadedQuadWVP tint_effect;

        ShadedQuad test_quad;
        FontManager font_manager_badaboom;
        FontManager font_manager_impact;
        FontManager font_manager_profont;
        FontManager font_manager_emoji;
        FontManager font_manager_assets;
        FontManager font_manager_print;
        FontManager font_manager_bebop;

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
            Swoop.Initialize(this, graphics, Window, resolution, true, false, true);

            fps = new FPSCounter();
            this.Disposed += SwoopGame_Disposed;
            base.Initialize();
        }

        void print_test(string input, int number) {
            Debug.Print(input + " " + number);
        }

        protected override void LoadContent() {
            output_rt = new RenderTarget2D(GraphicsDevice, resolution.X, resolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            Swoop.Load(GraphicsDevice, graphics, Content, Window);

            font_manager_badaboom = new FontManager("BadaBoom BB", 18f, -0.5f);
            font_manager_badaboom.alter_glyph_width("P", -2);
            font_manager_badaboom.alter_glyph_width("p", -2);
            font_manager_badaboom.alter_glyph_width("i", -1);
            font_manager_badaboom.alter_glyph_width("h", -1);
            font_manager_badaboom.alter_glyph_width("B", -2);
            font_manager_badaboom.alter_glyph_width("g", -1);
            font_manager_badaboom.alter_glyph_width("o", -1);

            font_manager_impact = new FontManager("Impact", 27f, 1f);
            font_manager_profont = new FontManager("ProFontWindows", 9f, 1f);
            font_manager_emoji = new FontManager("Segoe UI Emoji", 16f, System.Drawing.FontStyle.Regular, 3f, false);
            font_manager_assets = new FontManager("Segoe MDL2 Assets", 27f, 1.8f, false);
            font_manager_print = new FontManager("Segoe Print", 27f, 1.8f, false);
            font_manager_bebop = new FontManager("Nueva Std", 27f, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic, -0.2f);

            font_manager_bebop.alter_glyph_width("P", -4);
            font_manager_bebop.alter_glyph_width("W", -2);
            font_manager_bebop.alter_glyph_width("O", 1);
            font_manager_bebop.alter_glyph_width("Y", -3);
            font_manager_bebop.alter_glyph_width(".", 3);

            build_UI();
        }

        float spinny = 0;
        void draw_gdi_canvas(System.Drawing.Graphics e_g) {
            e_g.Clear(Swoop.UI_background_color.ToGDIColor());

            e_g.DrawString("BAZINGA!", new System.Drawing.Font("BadaBoom BB", 24), new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 255, 0, 0)), Vector2.Zero.ToPointF());
            e_g.DrawString("🦶🦶👀👅", new System.Drawing.Font("Segoe UI Emoji", 24), System.Drawing.Brushes.White, (Vector2.UnitY * 25).ToPointF());

            spinny += (float)(90f * Swoop.game_time.ElapsedGameTime.TotalSeconds);
            if (spinny > 360f) spinny -= 360f;

            e_g.FillPie(System.Drawing.Brushes.LightGray, new System.Drawing.Rectangle(5, 75, 20, 20), -90f, 0f + spinny);
        }

        XYPair rt_pos;

        protected void build_UI() {
            render_target_bg = new AutoRenderTarget(
                resolution.X_only - (resolution.X_only / 4.5f) + ((UI.elements["title_bar"].height + 28) * XYPair.UnitY),
                (XYPair.UnitX * 200) + (XYPair.UnitY * 100), true);

            rt_pos = resolution.X_only - (resolution.X_only / 4.5f) + ((render_target_bg.position.Y + render_target_bg.size.Y + 20) * XYPair.UnitY);
            render_target_fg = new AutoRenderTarget(
                rt_pos,
                (XYPair.UnitX * 200) + (XYPair.UnitY * 100), true);

            UI.add_element(new Label(
                "test_3d_label_2", "shader > rt > foreground",
                (resolution.X_only - (resolution.X_only / 4.5f)) + ((render_target_fg.position.Y - 11) * XYPair.UnitY)));
            UI.add_element(new Label(
                "test_3d_label_3", "this text is behind the top\nrt layer and being passed\nthrough via a shader which is\naware of each pixel's screen\nposition, so it can pull data\nfrom the main screen render\ntarget and tint it for example\nwheeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee\n\n    hold Ctrl+Z to move this tint box around",
                (resolution.X_only - (resolution.X_only / 4.5f)) + ((render_target_fg.position.Y + 5) * XYPair.UnitY) + (XYPair.UnitX * -20f)));

            UI.add_element(new Panel("trans_panel",
                resolution.X_only - (resolution.X_only / 4.5f) + ((render_target_fg.position.Y + render_target_fg.size.Y + 40) * XYPair.UnitY),
                (XYPair.UnitX * 200) + (XYPair.UnitY * 100),
                null, (Panel p) => { 
                    Drawing.end(); 
                    Drawing.begin(BlendState.Opaque); 
                    Drawing.fill_rect(XYPair.One, p.size - (XYPair.One * 1), Color.Transparent);
                    Drawing.end();
                } ));

            UI.add_element(new Label("hole",
                "hole",
                UI.elements["trans_panel"].position - (XYPair.UnitY * 13) + (XYPair.UnitX * 20)
                ));

            AutoRenderTarget.Manager.register_background_draw(render_target_bg);
            AutoRenderTarget.Manager.register_foreground_draw(render_target_fg);


            UI.add_element(new Label("test_3d_label", "AutoRenderTarget tests\n3D plane > rt > background\nthis text is in front and\npart of the above label", (resolution.X_only - (resolution.X_only / 4.5f)) + ((UI.elements["title_bar"].height + 5) * XYPair.UnitY)));

            UI.add_element(new Button("test_custom_draw_button", (UIElement parent) => {
                Drawing.fill_rect(XYPair.Zero, parent.size, parent.mouse_over && parent.mouse_down ? Swoop.get_color(parent) : Swoop.UI_background_color);
                Drawing.image(Drawing.Logo, Vector2.Zero, Vector2.One * 28, SpriteEffects.None);
                Drawing.rect(XYPair.One, parent.size, Swoop.get_color(parent), 1f);
            }, UI.elements["test_3d_label"].position - (XYPair.UnitX * 30), XYPair.One * 28));
            UI.elements["test_custom_draw_button"].can_be_focused = false;

            draw_shader = new ShadedQuadWVP(Swoop.content, "draw_2d");
            tint_effect = new ShadedQuadWVP(Swoop.content, "test_tint");

            draw_shader.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f), render_target_bg.size.aspect_ratio, 0.01f, 5f);
            draw_shader.view = Matrix.CreateLookAt(Vector3.Backward * 1f, Vector3.Forward, Vector3.Up);
            draw_shader.world = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(45f)) * Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(25));
            
            draw_shader.set_param("main_texture", Drawing.Logo);

            draw_shader.set_param("screen_pos_texture", render_target_fg.screen_pos_rt);
            draw_shader.set_param("screen_texture", Swoop.render_target_output);

            render_target_bg.draw.register_action("test_draw_plane", () => {
                Drawing.image(render_target_bg.screen_pos_rt, XYPair.Zero, render_target_bg.size);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullNone;

                draw_shader.set_param("main_texture", Drawing.Logo);
                draw_shader.draw_plane();

                Drawing.end();
                Drawing.rect(XYPair.One, render_target_bg.size, Swoop.UI_color, 1f);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                Drawing.graphics_device.DepthStencilState = DepthStencilState.Default;
            });

            render_target_fg.draw.register_action("test_tint", () => {

                Drawing.end();

                tint_effect.set_param("tint", Swoop.UI_highlight_color.ToVector4());
                tint_effect.set_param("bg", Swoop.UI_background_color.ToVector4());
                tint_effect.set_param("screen_texture", output_rt);
                tint_effect.set_param("screen_pos_texture", render_target_fg.screen_pos_rt);

                tint_effect.draw_plane();

                Drawing.rect(XYPair.One, render_target_fg.size, Swoop.UI_color, 1f);

                Drawing.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                Drawing.graphics_device.DepthStencilState = DepthStencilState.Default;
            });

            ((Button)UI.elements["exit_button"]).click_action = () => {
                if (capture_demo_screenshot_on_exit) {
                    output_rt.SaveAsPng(new FileStream("..\\..\\..\\current.png", FileMode.OpenOrCreate), resolution.X, resolution.Y);
                    capture_demo_screenshot_on_exit = false;
                }
                Swoop.End();
            };


            UI.add_element(new Label("toggle_label", "a toggle button:", (XYPair.UnitY * 25) + (XYPair.UnitX * 15)));

            UI.add_element(new ToggleButton("toggle_button", "Toggled On", "Toggled Off", (XYPair.UnitY * 31) + (XYPair.UnitX * 155)));
            UI.elements["toggle_button"].anchor_local = UIElement.AnchorPoint.CENTER;


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
            UI.register_tooltip("behind_button", "behind hehe");
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

                    sub_elements.add_element(new Label("anchor_label", "this label should be anchored to the bottom right", panel.size, UIElement.AnchorPoint.BOTTOM_RIGHT));
                    sub_elements.anchor_to_side("anchor_label", Anchor.AnchorTo.Right | Anchor.AnchorTo.Bottom);
                    
                })                
            );

            ((Panel)UI.elements["test_panel"]).draw_action = (Panel panel) => {
                font_manager_assets.draw_string("", (XYPair.UnitX * 200) + (XYPair.UnitY * 280), Color.White, 1f);
            };

            UI.add_element(new Label("panel_label", "panel", UI.elements["test_panel"].position + XYPair.Up * 15));

            var dialog_size = new XYPair(340, 160);
            ((Button)UI.elements["test_button"]).click_action = () => {
                UI.add_element(new Dialog("test_dialog",
                (resolution / 2) - (dialog_size * 0.5f),
                dialog_size,
                "test dialog title text",
                (Dialog td, UIElementManager sub_elements) => {
                    sub_elements.add_element(new Label("test_label", "this is a test label for testing\nall sorts of different text\n\nthis is a bit of extra text for testing\nabcdefghijklmnopqrstuvwxyz",
                        td.size / 2, Label.AnchorPoint.CENTER));

                    ((Label)sub_elements.elements["test_label"]).text_justification = Label.alignment.CENTER;

                    sub_elements.add_element(new Label("test_label_2", $"this is another test label with an outline\nalso this dialog is called {td.name}", XYPair.One * 10));
                    ((Label)sub_elements.elements["test_label_2"]).draw_outline = true;

                    sub_elements.add_element(new Button("close", "close", new XYPair(td.size.X / 2, td.size.Y - 25)));
                    sub_elements.elements["close"].anchor_local = UIElement.AnchorPoint.CENTER;
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
            test_listbox.add_item(new ListBoxItem(50, () => {
                Drawing.fill_rect_dither(XYPair.Zero, resolution, Swoop.UI_background_color, Swoop.UI_color);
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
            UI.register_tooltip("rng_add_button", new Tooltip("Add a new item to the listbox with\na randomized hexadecimal name"));


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
                "\nI don't really know why I thought that this was a good idea but it did let me implement a fairly robust system for editing text, the TextBox above uses the same system" +
                "\n" +
                "\ntodo:" +
                "\nmouse support at all" +
                "\nscroll bars" +
                "\nselection is still broken" +
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
                UI.elements["text_editor"].bottom_xy + (XYPair.UnitY * 8f), 
                UI.elements["text_editor"].size.X_only, 
                "Option Slider", 
                "Low", "Medium", "High", "Ultra", "Yes"
                ));
            //UI.anchor_local("option_slider", UIElement.AnchorPoint.BOTTOM_RIGHT);
            //UI.anchor_to_side("option_slider", Anchor.AnchorTo.Right | Anchor.AnchorTo.Bottom);
            UI.register_tooltip("option_slider", new Tooltip("A TOOLTIP TITLE", "test of a tooltip lmoa\nhaha"));

            UI.add_elements(new DropDown("dropdown",
                UI.elements["option_slider"].bottom_xy + (XYPair.UnitY * 8f),
                UI.elements["option_slider"].size.X,
                "fart lol hahahahahahahhahahdahsjgdfjsdfgkhjsdfhgjksdfgkljsfdhngsdnfh g",
                "anotehr fart"
                ));



            Binds.add("test", Keys.Back);
            Binds.add("test_two", MouseButtons.Right);
            Binds.add("test_tint_rect", Keys.Z);
        }

        static float progress_bar_test_value = 0.5f;

        protected override void Update(GameTime gameTime) {
            Swoop.update_input();

            float clock = (float)gameTime.ElapsedGameTime.TotalSeconds;
            progress_bar_test_value += 0.25f * clock;
            if (progress_bar_test_value > 1f)
                progress_bar_test_value -= 1f;

            ((ProgressBar)UI.elements["progress_bar"]).value = progress_bar_test_value;
            ((ProgressBar)UI.elements["progress_bar_inverted"]).value = progress_bar_test_value;

            ((ProgressBar)UI.elements["progress_bar_vertical"]).value = progress_bar_test_value;
            ((ProgressBar)UI.elements["progress_bar_vertical_inverted"]).value = progress_bar_test_value;

            if (Swoop.input_handler.is_pressed(Keys.LeftControl) && Binds.is_pressed("test_tint_rect", Swoop.input_handler)) {
                Binds.handle("test_tint_rect", Swoop.input_handler);
                render_target_fg.position = Input.cursor_pos.ToXYPair() - (render_target_fg.size / 2f);

            } else if (Binds.just_released("test_tint_rect", Swoop.input_handler)) {
                render_target_fg.position = rt_pos;
            }


            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";
            string focus_info = $"{(UIElementManager.focused_element != null ? UIElementManager.focused_element.name : "")}";            

            ((TitleBar)UI.elements["title_bar"]).left_text = title_text;
            ((TitleBar)UI.elements["title_bar"]).right_text = 
                (Swoop.mouse_over_element != null ? $"M{UIElementManager.manager_under_mouse_index} -> {Swoop.mouse_over_element.name} | " : "WORLD | ") +                 
                FPS_text + " | " +  Input.poll_method;

            ((Label)UI.elements["ri_info_label"]).change_text(Swoop.input_handler.ri_info());

            if (Swoop.input_handler.is_pressed(Keys.Escape)) ((Button)UI.elements["exit_button"]).click_action();


            if (Swoop.input_handler.just_held(Keys.H)) {
                Debug.Print("fart");
            }

            Swoop.Update(gameTime);
            fps.update(gameTime);
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime) {
            if (test_quad == null) test_quad = new ShadedQuad(Content, "draw_2d", XYPair.One * 100, XYPair.One * 100);

            if (!Swoop.enable_draw) {
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Swoop.UI_background_color);
                //swoop.DrawBackground();
                base.Draw(gameTime);
                return;
            }

            //draw the UI
            Swoop.Draw();

            //Draw the UI to an output RT
            //this is not required, but it makes screenshots easier
            GraphicsDevice.SetRenderTarget(output_rt);
            GraphicsDevice.Clear(Color.Transparent);
            Swoop.DrawBackground();
            AutoRenderTarget.Manager.draw_rts_to_target_background();

            font_manager_impact.draw_string("SPRITE FONT RENDERER", (XYPair.UnitX * 10) + (XYPair.UnitY * 440), Swoop.UI_highlight_color);
            font_manager_badaboom.draw_map_debug_layer((XYPair.UnitX * 10) + (XYPair.UnitY * 480), font_manager_badaboom.char_map_size, Content);

            font_manager_profont.draw_string("FontManager vs SpriteFont\n[ProFontWindows]", (XYPair.UnitX * 10) + (XYPair.UnitY * 595), Swoop.UI_color);
            Drawing.begin();
            Drawing.sb.DrawString(Drawing.fnt_profont,"[ProFontWindows]", ((XYPair.UnitX * 10) + (XYPair.UnitY * 621)).ToVector2(), Swoop.UI_color);
            font_manager_emoji.draw_string("🤔 hmmm...\nI like the top one 👍", (XYPair.UnitX * 110) + (XYPair.UnitY * 605), Swoop.UI_color);
            font_manager_badaboom.draw_string_shadow("Bazinga!", (XYPair.UnitX * 10) + (XYPair.UnitY * 480) + (XYPair.UnitY * 65), Color.Red, Color.Black, XYPair.One * 2, 2f);
            font_manager_assets.draw_string("      ", (XYPair.UnitX * 200) + (XYPair.UnitY * 480) + (XYPair.UnitY * 65), Color.White, 1f);            
            font_manager_print.draw_string_rainbow("wet socks...", (XYPair.UnitX * 260) + (XYPair.UnitY * 480) + (XYPair.UnitY * 95), 1f, XYPair.One * 2, Swoop.UI_highlight_color, Color.Magenta, Color.LawnGreen, Color.Blue);
            font_manager_bebop.draw_string("SEE YOU SPACE COWBOY...", resolution - (font_manager_bebop.measure_string("SEE YOU SPACE COWBOY...") + (XYPair.UnitX * 20)), Swoop.UI_color, 1f);


            Drawing.begin(BlendState.Opaque);
            Drawing.fill_rect(
                Swoop.UI.elements["trans_panel"].position,
                Swoop.UI.elements["trans_panel"].position + (XYPair.UnitX * 200) + (XYPair.UnitY * 100),
                Color.Transparent);
            Drawing.end();

            //draw background RTs, then the main RT output, then the foreground RTs
            Drawing.image(Swoop.render_target_output, XYPair.Zero, resolution);

            GraphicsDevice.SetRenderTarget(null);
            Drawing.image(output_rt, XYPair.Zero, resolution);
            AutoRenderTarget.Manager.draw_rts_to_target_foreground();


            //FontManager.glyph_draw_shader.begin_spritebatch(Drawing.sb, SamplerState.AnisotropicWrap);
            //Drawing.image(font_manager_small.char_map_texture, XYPair.Zero, Drawing.font_manager_profont.char_map_size);
            //font_manager_emoji.draw_map_debug_layer((XYPair.UnitX * 10) + (XYPair.UnitY * 480), font_manager_emoji.char_map_size, Content);

            base.Draw(gameTime);
        }

        private void SwoopGame_Disposed(object sender, System.EventArgs e) {
            Swoop.End();
        }
    }
}