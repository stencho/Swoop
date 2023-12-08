using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography;

namespace SwoopLib.Effects {

    public class ManagedEffect {
        static BasicEffect basic_effect;

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

        internal Effect shader;
        public Effect effect => shader;

        int selected_technique = 0;
        int selected_pass = -1;

        public virtual void update() { }

        internal void load_shader_file(ContentManager content, string effect_name) {
            if (shader == null) shader = content.Load<Effect>(effect_name);
        }        

        internal bool shader_has_param(string param) {
            if (shader == null) return false;

            foreach (EffectParameter parameter in shader.Parameters) 
                if (parameter.Name == param) return true;
            
            return false;
        }

        bool throw_error_on_bad_param = false;

        public void set_param<T>(string param, T value) {
            if (value == null || shader == null || !shader_has_param(param)) {
                if (throw_error_on_bad_param) throw new Exception("Bad shader param: " + param);
                else return;
            }
            var t = typeof(T); 
            var obj = (object)value;

            if (t == typeof(bool)) shader.Parameters[param].SetValue((bool)obj);
            else if (t == typeof(int)) shader.Parameters[param].SetValue((int)obj);
            else if (t == typeof(XYPair)) shader.Parameters[param].SetValue(((XYPair)obj).ToVector2());
            else if (t == typeof(float)) shader.Parameters[param].SetValue((float)obj);
            else if (t == typeof(Vector2)) shader.Parameters[param].SetValue((Vector2)obj);
            else if (t == typeof(Vector3)) shader.Parameters[param].SetValue((Vector3)obj);
            else if (t == typeof(Vector4)) shader.Parameters[param].SetValue((Vector4)obj);
            else if (t == typeof(Color)) {
                shader.Parameters[param].SetValue(((Color)obj).ToVector4());
            } else if (t == typeof(Texture2D)) shader.Parameters[param].SetValue((Texture2D)obj);
            else if (t == typeof(TextureCube)) shader.Parameters[param].SetValue((TextureCube)obj);
            else if (t == typeof(RenderTarget2D)) shader.Parameters[param].SetValue((RenderTarget2D)obj);
        }

        public virtual void begin_spritebatch(SpriteBatch sb) {
            if (Drawing.sb_drawing) sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
            Drawing.sb_drawing = true;
        }

        
        public ManagedEffect() {
            basic_effect = new BasicEffect(Drawing.graphics_device);
            
            basic_effect.World = Matrix.Identity;
            basic_effect.View = Matrix.Identity;
            basic_effect.Projection = Matrix.Identity;

            basic_effect.TextureEnabled = true;
        }
        
        public virtual void draw_buffers_basic_effect_first_pass(VertexBuffer vb, IndexBuffer ib, Matrix world, Matrix view, Matrix projection) {
            if (shader == null) return;
            if (selected_technique < 0 || selected_technique >= shader.Techniques.Count) return;
            if (selected_pass < -1 || selected_pass >= shader.Techniques[selected_technique].Passes.Count) return;

            basic_effect.World = world;
            basic_effect.View = view;
            basic_effect.Projection = projection;
            

            Drawing.graphics_device.SetVertexBuffer(vb);
            Drawing.graphics_device.Indices = ib;

            basic_effect.CurrentTechnique.Passes[0].Apply();
            
            if (selected_pass == -1) {
                for (int i = 0; i < shader.Techniques[selected_technique].Passes.Count; i++) {
                    shader.Techniques[selected_technique].Passes[i].Apply();
                }
            } else {
                shader.Techniques[selected_technique].Passes[selected_pass].Apply();
            }

            //shader.CurrentTechnique.Passes[0].Apply();
            Drawing.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

    }
}
