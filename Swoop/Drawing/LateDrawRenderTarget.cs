using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SwoopLib.Effects;
using System.Runtime.CompilerServices;

namespace SwoopLib {
    public class LateDrawRenderTarget {
        public static class Manager {
            static List<LateDrawRenderTarget> targets = new List<LateDrawRenderTarget>();

            public static void add(LateDrawRenderTarget plane) {
                if (!targets.Contains(plane)) targets.Add(plane);
            }
            public static void remove(LateDrawRenderTarget plane) {
                if (targets.Contains(plane)) targets.Remove(plane);
            }

            public static void draw_rts() {
                foreach(LateDrawRenderTarget target in targets) {
                    Drawing.graphics_device.SetRenderTarget(target.render_target);
                    if (target.draw != null) {
                        target.draw();
                    }
                }
            }
            public static void draw_rts_to_target() {
                foreach (LateDrawRenderTarget target in targets) {
                    if (target.draw != null) {
                        Drawing.image(target.render_target, target.position, target.size);
                    }
                }
            }

        }

        static Effect screen_pos_effect;

        public RenderTarget2D render_target;
        public RenderTarget2D screen_pos_rt;

        XYPair _position = XYPair.Zero;
        public XYPair position {
            get { return _position; }
            set { 
                _position = value;
                draw_screen_pos_map();
            }
        }
        
        XYPair _size = -XYPair.One;
        public XYPair size {
            get {
                if (_size == -XYPair.One) return Swoop.resolution;
                else return _size;                
            } set { _size = value; resize_render_targets(); } }

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        DynamicVertexBuffer quad_vb;
        DynamicIndexBuffer quad_ib;

        void resize_render_targets () {
            render_target = new RenderTarget2D(Drawing.graphics_device, size.X, size.Y);
            screen_pos_rt = new RenderTarget2D(Drawing.graphics_device, size.X, size.Y);

            draw_screen_pos_map();
        }

        static BasicEffect test_effect;

        public LateDrawRenderTarget() {
            Manager.add(this);

            if (screen_pos_effect == null) screen_pos_effect = Swoop.content.Load<Effect>("effects/render_target_screen_pos");
            if (test_effect == null) test_effect = new BasicEffect(Drawing.graphics_device);

            render_target = new RenderTarget2D(Drawing.graphics_device, size.X, size.Y);
            screen_pos_rt = new RenderTarget2D(Drawing.graphics_device, size.X, size.Y);

            draw_screen_pos_map();

            world = Matrix.CreateTranslation(Vector3.Backward * 0.01f);

            projection = Matrix.Identity;// Matrix.CreateOrthographic(1f, 1f, 0.01f, 1f);

            quad_vb = new DynamicVertexBuffer(Drawing.graphics_device, VertexPositionColorTexture.VertexDeclaration, 4, BufferUsage.None);
            quad_ib = new DynamicIndexBuffer(Drawing.graphics_device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);

            quad_vb.SetData(new VertexPositionColorTexture[4] {
                new VertexPositionColorTexture(Vector3.Up + Vector3.Left,                  Color.Red, Vector2.Zero),
                new VertexPositionColorTexture(Vector3.Up + Vector3.Right,                 Color.Red, Vector2.UnitX),
                new VertexPositionColorTexture(Vector3.Down + Vector3.Right, Color.Red, Vector2.One),
                new VertexPositionColorTexture(Vector3.Down + Vector3.Left,                 Color.Red, Vector2.UnitY)
            });

            quad_ib.SetData(new int[6] { 0, 1, 2, 2, 3, 0 });

            draw = () => {
                Drawing.graphics_device.Clear(Color.White);
                Drawing.fill_rect_dither(XYPair.Zero, XYPair.One * 50, Color.Red, Color.Transparent);
                Drawing.image(screen_pos_rt, XYPair.Zero, size);

                Drawing.fill_rect_dither(XYPair.Zero, XYPair.One * 50, Color.Red, Color.Transparent);
                /*
                test_effect.World = world;
                test_effect.View = view;
                test_effect.Projection = projection;
                test_effect.DiffuseColor = Color.White.ToVector3();

                Drawing.graphics_device.RasterizerState = RasterizerState.CullNone;
                Drawing.graphics_device.DepthStencilState = DepthStencilState.None;

                Drawing.graphics_device.SetVertexBuffer(quad_vb);
                Drawing.graphics_device.Indices = quad_ib;

                Drawing.graphics_device.SetRenderTarget(render_target);
                */
                //test_effect.CurrentTechnique.Passes[0].Apply();
                //Drawing.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            };
        }

        ~LateDrawRenderTarget() {
            Manager.remove(this);
        }

        public void update() { }        

        public void draw_screen_pos_map() {
            Drawing.graphics_device.SetRenderTarget(screen_pos_rt);

            screen_pos_effect.Parameters["resolution"].SetValue(Swoop.resolution.ToVector2());
            screen_pos_effect.Parameters["position"].SetValue(position.ToVector2());
            screen_pos_effect.Parameters["size"].SetValue(size.ToVector2());

            Drawing.end();
            Drawing.begin(screen_pos_effect);

            Drawing.fill_rect(XYPair.Zero, XYPair.One * size, Color.White);

            Drawing.end();
        }

        Action draw;
    }
}
