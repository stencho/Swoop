using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib.Effects {

    public abstract class ManagedEffect {
        public static class Manager {
            static List<ManagedEffect> registered_effects_update = new List<ManagedEffect>();

            public static void register_for_update(ManagedEffect effect) => registered_effects_update.Add(effect);            
            public static void unregister_for_update(ManagedEffect effect) => registered_effects_update.Remove(effect);

            public static void do_updates() {
                foreach (var effect in registered_effects_update) {
                    effect.update();
                }
            }
        }
        static BasicEffect basic_effect;

        public Effect effect => _effect;
        internal Effect _effect;

        public int selected_technique { get; set; } = 0;
        public int selected_pass { get; set; } = -1;
        public bool throw_error_on_bad_param { get; set; } = false;

        public Matrix basic_effect_world { get { return basic_effect.World; } set { basic_effect.World = value; } }
        public Matrix basic_effect_view { get { return basic_effect.View; } set { basic_effect.View = value; } }
        public Matrix basic_effect_projection { get { return basic_effect.Projection; } set { basic_effect.Projection = value; } }

        public ManagedEffect() {
            build_basic_effect();
        }
        public ManagedEffect(Effect effect) {
            _effect = effect;
            build_basic_effect();
        }
        public ManagedEffect(ContentManager content, string effect_name) {
            load_shader_file(content, effect_name);
            build_basic_effect();
        }

        void build_basic_effect() {
            if (basic_effect == null) {
                basic_effect = new BasicEffect(Drawing.graphics_device);

                basic_effect.TextureEnabled = true;
                basic_effect.Texture = Drawing.OnePXWhite;
            }
        }

        /// <summary>
        /// Used by the Manager class in its update loop
        /// </summary>
        internal virtual void update() { }

        internal void load_shader_file(ContentManager content, string effect_name) {
             _effect = content.Load<Effect>(effect_name);
        }        

        internal bool shader_has_param(string param) {
            if (_effect == null) return false;

            foreach (EffectParameter parameter in _effect.Parameters) 
                if (parameter.Name == param) return true;
            
            return false;
        }

        public void set_param<T>(string param, T value) {
            if (value == null || _effect == null || !shader_has_param(param)) {
                if (throw_error_on_bad_param) throw new Exception("Bad shader param: " + param);
                else return;
            }

            var t = typeof(T); var obj = (object)value;

            if (t == typeof(bool)) _effect.Parameters[param].SetValue((bool)obj);
            else if (t == typeof(int)) _effect.Parameters[param].SetValue((int)obj);
            else if (t == typeof(XYPair)) _effect.Parameters[param].SetValue(((XYPair)obj).ToVector2());
            else if (t == typeof(float)) _effect.Parameters[param].SetValue((float)obj);
            else if (t == typeof(Vector2)) _effect.Parameters[param].SetValue((Vector2)obj);
            else if (t == typeof(Vector3)) _effect.Parameters[param].SetValue((Vector3)obj);
            else if (t == typeof(Vector4)) _effect.Parameters[param].SetValue((Vector4)obj);
            else if (t == typeof(Color)) _effect.Parameters[param].SetValue(((Color)obj).ToVector4());            
            else if (t == typeof(Texture2D)) _effect.Parameters[param].SetValue((Texture2D)obj);
            else if (t == typeof(TextureCube)) _effect.Parameters[param].SetValue((TextureCube)obj);
            else if (t == typeof(RenderTarget2D)) _effect.Parameters[param].SetValue((RenderTarget2D)obj);
            else { throw new Exception("Bad shader object type"); }
        }

        public virtual void begin_spritebatch(SpriteBatch sb) {
            if (Drawing.sb_drawing) sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
            Drawing.sb_drawing = true;
        }
        public virtual void begin_spritebatch(SpriteBatch sb, BlendState blend_state) {
            if (Drawing.sb_drawing) sb.End();
            sb.Begin(SpriteSortMode.Immediate, blend_state, SamplerState.PointWrap, null, null, effect, null);
            Drawing.sb_drawing = true;
        }
        public virtual void begin_spritebatch(SpriteBatch sb, SamplerState sampler_state) {
            if (Drawing.sb_drawing) sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler_state, null, null, effect, null);
            Drawing.sb_drawing = true;
        }
        public virtual void begin_spritebatch(SpriteBatch sb, BlendState blend_state, SamplerState sampler_state) {
            if (Drawing.sb_drawing) sb.End();
            sb.Begin(SpriteSortMode.Immediate, blend_state, sampler_state, null, null, effect, null);
            Drawing.sb_drawing = true;
        }

        public virtual void draw(XYPair position, XYPair size) {
            begin_spritebatch(Drawing.sb);
            Drawing.sb.Draw(Drawing.OnePXWhite, new Rectangle(position.ToPoint(), size.ToPoint()), Color.Transparent);
            Drawing.end();
        }
        public virtual void draw_texture(Texture2D texture, XYPair position, XYPair size) {
            begin_spritebatch(Drawing.sb);
            Drawing.sb.Draw(texture, new Rectangle(position.ToPoint(), size.ToPoint()), Color.White);
            Drawing.end();
        }
        public virtual void draw_texture(Texture2D texture, XYPair position, XYPair size, XYPair crop_position, XYPair crop_size) {
            begin_spritebatch(Drawing.sb);
            Drawing.sb.Draw(texture, 
                new Rectangle(position.ToPoint(), size.ToPoint()),
                new Rectangle(crop_position.X, crop_position.Y, crop_size.X, crop_size.Y),
                Color.White);
            Drawing.end();
        }

        public void apply_passes() {
            for (int i = 0; i < _effect.Techniques[selected_technique].Passes.Count; i++) {
                _effect.Techniques[selected_technique].Passes[i].Apply();
            }
        }
        public void apply_passes(int technique) {
            for (int i = 0; i < _effect.Techniques[technique].Passes.Count; i++) {
                _effect.Techniques[technique].Passes[i].Apply();
            }
        }
        public void apply_pass(int pass) {
            _effect.Techniques[selected_technique].Passes[pass].Apply();
        }
        public void apply_pass(int technique, int pass) {
            _effect.Techniques[technique].Passes[pass].Apply();
        }


        /// <summary>
        /// <para>Uses the BasicEffect to set up the vertices and paint a base white texture,
        /// then uses the selected effect's pixel shader to draw its texture</para>
        /// <para></para>
        /// <para>Useful for drawing a 3D mesh with an appearance entirely determined by a pixel shader</para>
        /// </summary>
        /// <param name="vb">The vertex buffer of the mesh</param>
        /// <param name="ib">The index buffer of the mesh</param>
        /// <param name="world">World matrix</param>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        public virtual void draw_buffers_basic_effect_first_pass(VertexBuffer vb, IndexBuffer ib, Matrix world, Matrix view, Matrix projection) {
            if (_effect == null) return;
            if (selected_technique < 0 || selected_technique >= _effect.Techniques.Count) return;
            if (selected_pass < -1 || selected_pass >= _effect.Techniques[selected_technique].Passes.Count) return;

            basic_effect.World = world;
            basic_effect.View = view;
            basic_effect.Projection = projection;
            basic_effect.Texture = Drawing.OnePXWhite;

            Drawing.graphics_device.SetVertexBuffer(vb);
            Drawing.graphics_device.Indices = ib;

            basic_effect.CurrentTechnique.Passes[0].Apply();

            if (selected_pass == -1) apply_passes();
            else apply_pass(selected_pass);

            //shader.CurrentTechnique.Passes[0].Apply();
            Drawing.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        /// <summary>
        /// <para>Uses the BasicEffect to set up the vertices and paint a base white texture, 
        /// then uses the selected effect's pixel shader to draw its texture</para>
        /// <para></para>
        /// <para>Useful for drawing a 3D mesh with an appearance entirely determined by a pixel shader</para>
        /// <para>This call assumes you have already set up the vertex and index buffers</para>
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        public virtual void draw_buffers_basic_effect_first_pass(Matrix world, Matrix view, Matrix projection) {
            if (_effect == null) return;
            if (selected_technique < 0 || selected_technique >= _effect.Techniques.Count) return;
            if (selected_pass < -1 || selected_pass >= _effect.Techniques[selected_technique].Passes.Count) return;

            basic_effect.World = world;
            basic_effect.View = view;
            basic_effect.Projection = projection;

            basic_effect.CurrentTechnique.Passes[0].Apply();

            if (selected_pass == -1) apply_passes();
            else apply_pass(selected_pass);            

            //shader.CurrentTechnique.Passes[0].Apply();
            Drawing.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        /// <summary>
        /// <para>Uses the BasicEffect to set up the vertices and paint a base white texture, 
        /// then uses the selected effect's pixel shader to draw its texture</para>
        /// <para></para>
        /// <para>Useful for drawing a 3D mesh with an appearance entirely determined by a pixel shader</para>
        /// <para>This call assumes you have already set up the vertex and index buffers using GraphicsDevice,
        /// as well as the BasicEffect's WVP using the basic_effect_world/view/projection properties </para>
        /// </summary>
        public virtual void draw_buffers_basic_effect_first_pass() {
            if (_effect == null) return;
            if (selected_technique < 0 || selected_technique >= _effect.Techniques.Count) return;
            if (selected_pass < -1 || selected_pass >= _effect.Techniques[selected_technique].Passes.Count) return;

            basic_effect.CurrentTechnique.Passes[0].Apply();

            if (selected_pass == -1) apply_passes();
            else apply_pass(selected_pass);            

            //shader.CurrentTechnique.Passes[0].Apply();
            Drawing.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }
}
