using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SwoopLib.Effects;
using System.Runtime.CompilerServices;

namespace SwoopLib {
    public class AutoRenderTarget {
        public static class Manager {
            static List<AutoRenderTarget> targets = new List<AutoRenderTarget>();

            public static int target_count => targets.Count;

            public static void add(AutoRenderTarget target) {
                if (!targets.Contains(target)) targets.Add(target);
            }
            public static void remove(AutoRenderTarget target) {
                if (targets.Contains(target)) targets.Remove(target);
            }

            public static void register_background_draw(AutoRenderTarget target) {
                target.draw_to_screen_early = true;
            }
            public static void register_foreground_draw(AutoRenderTarget target) {
                target.draw_to_screen_late = true;
            }

            public static void unregister_background_draw(AutoRenderTarget target) {
                target.draw_to_screen_early = false;
            }
            public static void unregister_foreground_draw(AutoRenderTarget target) {
                target.draw_to_screen_late = false;
            }

            public static void draw_rts() {
                if (targets.Count == 0) return;

                foreach (AutoRenderTarget target in targets) {
                    if (target.needs_resize) target.resize_render_targets();
                }
                foreach (AutoRenderTarget target in targets) {
                    if (!target.screen_aware) continue;
                    if (target.needs_new_pos_map) target.draw_screen_pos_map();
                }

                foreach (AutoRenderTarget target in targets) {
                    Drawing.end();
                    Drawing.graphics_device.SetRenderTarget(target.render_target);
                    Drawing.graphics_device.Clear(Color.Transparent);

                    if (target.draw != null && target.render_target != null) {
                        target.draw.invoke_all();
                    }
                }
            }

            public static void refresh_all() {
                foreach (AutoRenderTarget target in targets) {
                    if (target != null) target.needs_resize = true;
                }
            }
            public static void draw_rts_to_target_background() {
                Drawing.end();

                if (targets.Count == 0) return;
                foreach (AutoRenderTarget target in targets) {
                    if (target != null && target.draw != null && target.render_target != null && target.draw_to_screen_early) {
                        //target.draw_screen_pos_map();
                        Drawing.image(target.render_target, target.position, target.size);                        
                    }
                }
            }
            public static void draw_rts_to_target_foreground() {
                Drawing.end();

                if (targets.Count == 0) return;
                foreach (AutoRenderTarget target in targets.Reverse<AutoRenderTarget>()) {
                    if (target.draw != null && target.draw_to_screen_late) {
                        //target.draw_screen_pos_map();
                        Drawing.image(target.render_target, target.position, target.size);
                    }
                }
            }

        }

        static Effect screen_pos_effect;

        /// <summary>
        /// called each frame, automatically, by the Manager class
        /// draw = (XYPair position, XYPair size) => { }
        /// </summary>
        public MultiAction draw = new MultiAction();

        public RenderTarget2D render_target;
        public RenderTarget2D screen_pos_rt;

        XYPair _position = XYPair.Zero;
        public XYPair position {
            get { return _position; }
            set { 
                _position = value;
                request_new_pos_map();
            }
        }
        
        XYPair _size = -XYPair.One;
        public XYPair size {
            get {
                if (_size == -XYPair.One) return Swoop.resolution;
                else return _size;                
            } set { _size = value; request_rt_resize(); } 
        }
        
        internal bool draw_to_screen_early = false;
        internal bool draw_to_screen_late = false;

        internal bool screen_aware = true;

        bool needs_resize = true;
        void request_rt_resize() {
            needs_resize = true;
        }

        void resize_render_targets() {
            render_target = new RenderTarget2D(Drawing.graphics_device, size.X, size.Y, false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            
            if (screen_aware) {  
                screen_pos_rt = new RenderTarget2D(Drawing.graphics_device, size.X, size.Y, false, SurfaceFormat.Vector2, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                draw_screen_pos_map();
            }

            needs_resize = false;
        }

        public AutoRenderTarget(XYPair position, XYPair size, bool screen_aware = true) {
            _position = position; _size = size;
            this.screen_aware = screen_aware;
            init();
        }

        public AutoRenderTarget() {
            screen_aware = false;
            init();
        }
        public AutoRenderTarget(XYPair size, bool screen_aware = false) {
            _position = XYPair.Zero; _size = size;
            this.screen_aware = screen_aware;
            init();
        }


        void init() {
            Manager.add(this);

            if (screen_pos_effect == null) screen_pos_effect = Swoop.content.Load<Effect>("effects/render_target_screen_pos");

            draw_screen_pos_map();
        }

        ~AutoRenderTarget() {
            Manager.remove(this);
        }

        public void update() { }

        bool needs_new_pos_map = false;
        void request_new_pos_map() {
            needs_new_pos_map = true;
        }

        public void draw_screen_pos_map() {
            Drawing.end();

            Drawing.graphics_device.SetRenderTarget(screen_pos_rt);

            Drawing.begin(screen_pos_effect);

            screen_pos_effect.Parameters["resolution"].SetValue(Swoop.resolution.ToVector2());
            screen_pos_effect.Parameters["position"].SetValue(position.ToVector2());
            screen_pos_effect.Parameters["size"].SetValue(size.ToVector2());
            
            Drawing.fill_rect(XYPair.Zero, size, Color.White);

            Drawing.end();
            needs_new_pos_map = false;
        }

    }
}
