using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Markup;

namespace SwoopLib.Effects {
    internal class Draw3D : ManagedEffect {
        Matrix _world = Matrix.Identity;
        Matrix _view = Matrix.Identity;
        Matrix _projection = Matrix.Identity;

        Matrix world {
            get { return _world; }
            set { _world = value; set_param("world", value); }
        }
        Matrix view {
            get { return _view; }
            set { _view = value; set_param("view", value); }
        }
        Matrix projection {
            get { return _projection; }
            set { _projection = value; set_param("projection", value); }
        }

        Action update_action;

        public override void update() { 
            if (update_action != null) update_action();
            base.update();
        }

        public Draw3D(ContentManager content) {
            load_shader_file(content, "effects/draw_3d");
            default_matrices();

            Manager.register_for_update(this);
        }

        void default_matrices() {
            set_param("world", Matrix.Identity);
            set_param("view", Matrix.Identity);
            set_param("projection", Matrix.Identity);
        }
    }


    public class ShadedQuad : ManagedEffect {
        Matrix _world = Matrix.Identity;

        XYPair _position;
        XYPair _size;

        public XYPair position {
            get { return _position; } 
            set { _position = value; }
        }
        public XYPair size {
            get { return _size; }
            set { _size = value; }
        }

        internal static VertexBuffer quad_vb;
        internal static IndexBuffer quad_ib;

        internal static VertexPositionTexture[] vb_data = {
            new VertexPositionTexture(Vector3.Up + Vector3.Left,      Vector2.Zero),
            new VertexPositionTexture(Vector3.Up + Vector3.Right,     Vector2.UnitX),
            new VertexPositionTexture(Vector3.Down + Vector3.Left,    Vector2.UnitY),
            new VertexPositionTexture(Vector3.Down + Vector3.Right,   Vector2.One)
        };
        internal static int[] ib_data = { 0, 1, 2, 1, 3, 2 };

        public ShadedQuad(ContentManager content, string effect_name, XYPair position, XYPair size) {
            if (quad_vb == null) {
                quad_vb = new VertexBuffer(Drawing.graphics_device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);

                quad_vb.SetData(vb_data);
            }
            if (quad_ib == null) {
                quad_ib = new IndexBuffer(Drawing.graphics_device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
                quad_ib.SetData(ib_data);
            }

            _position = position;
            _size = size;

            load_shader_file(content, $"effects/{effect_name}");
            Manager.register_for_update(this);
        }

        public void draw_plane(XYPair parent_resolution) {

            Vector2 pr = _position.ToVector2() / parent_resolution.ToVector2();
            Vector2 sr = _size.ToVector2() / parent_resolution.ToVector2();

            _world = 
                Matrix.CreateTranslation(1f, -1f, 0) 
                * Matrix.CreateScale(sr.X, sr.Y, 0) 
                * Matrix.CreateTranslation(-1f, 1f, 0) 
                * Matrix.CreateTranslation(pr.X, -pr.Y, 0);

            base.draw_buffers_basic_effect_first_pass(quad_vb, quad_ib, _world, Matrix.Identity, Matrix.Identity);
        }
    }

    public class ShadedQuadWVP : ManagedEffect {
    
        Matrix _world = Matrix.Identity;
        Matrix _view = Matrix.Identity;
        Matrix _projection = Matrix.Identity;

        Action update_action;

        Texture2D _texture;
        Texture2D texture { 
            get {
                return _texture;
            } set {
                _texture = value;
                set_param("tx", value);
            }
        }

        public Matrix world {
            get { return _world; }
            set { _world = value; set_param("world", value); }
        }
        public Matrix view {
            get { return _view; }
            set { _view = value; set_param("view", value); }
        }
        public Matrix projection {
            get { return _projection; }
            set { _projection = value; set_param("projection", value); }
        }

        void default_params() {
        }

        public override void update() {
            if (update_action != null) update_action();
            base.update();
        }

        public ShadedQuadWVP(ContentManager content, string effect_name) {
            if (ShadedQuad.quad_vb == null) {
                ShadedQuad.quad_vb = new VertexBuffer(Drawing.graphics_device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);

                ShadedQuad.quad_vb.SetData(ShadedQuad.vb_data);
            }
            if (ShadedQuad.quad_ib == null) { 
                ShadedQuad.quad_ib = new IndexBuffer(Drawing.graphics_device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
                ShadedQuad.quad_ib.SetData(ShadedQuad.ib_data);
            }

            load_shader_file(content, $"effects/{effect_name}");
            default_params();

            Manager.register_for_update(this);
        }

        public void draw_plane() {
            base.draw_buffers_basic_effect_first_pass(ShadedQuad.quad_vb, ShadedQuad.quad_ib, _world, _view, _projection);
        }
    }


}
