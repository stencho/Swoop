using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SwoopLib.Effects {

    public class ManagedEffect {
        internal Effect shader;
        public Effect effect => shader;

        internal void load_shader_file(ContentManager content, string effect_name) {
            if (shader == null) shader = content.Load<Effect>(effect_name);
        }        

        internal bool shader_has_param(string param) {
            if (shader == null) return false;

            foreach (EffectParameter parameter in shader.Parameters) 
                if (parameter.Name == param) return true;
            
            return false;
        }

        internal void set_param<T>(string param, T value) {
            if (value == null || shader == null || !shader_has_param(param)) return;

            var t = typeof(T); 
            var obj = (object)value;

            if (t == typeof(bool)) shader.Parameters[param].SetValue((bool)obj);
            else if (t == typeof(int)) shader.Parameters[param].SetValue((int)obj);
            else if (t == typeof(XYPair)) shader.Parameters[param].SetValue(((XYPair)obj).ToVector2());
            else if (t == typeof(float)) shader.Parameters[param].SetValue((float)obj);
            else if (t == typeof(Vector2)) shader.Parameters[param].SetValue((Vector2)obj);
            else if (t == typeof(Vector3)) shader.Parameters[param].SetValue((Vector3)obj);
            else if (t == typeof(Vector4)) shader.Parameters[param].SetValue((Vector4)obj);
            else if (t == typeof(Color)) shader.Parameters[param].SetValue(((Color)obj).ToVector4());
            else if (t == typeof(Texture2D)) shader.Parameters[param].SetValue((Texture2D)obj);
            else if (t == typeof(TextureCube)) shader.Parameters[param].SetValue((TextureCube)obj);
        }

        public virtual void begin_spritebatch(SpriteBatch sb) {
            if (Drawing.sb_drawing) sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, null);
            Drawing.sb_drawing = true;
        }

    }
}
