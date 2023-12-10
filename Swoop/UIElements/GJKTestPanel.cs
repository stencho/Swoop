using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SwoopLib.Collision;
using SwoopLib.Shapes;

namespace SwoopLib.UIElements {
    public class GJKTestPanel : Panel {
        Circle cursor = new Circle(Vector2.Zero, 5f);

        List<Shape2D> shapes = new List<Shape2D>();
        List<GJK2D.gjk_result> results = new List<GJK2D.gjk_result>();

        //LateDrawRenderTarget sfp;
        public GJKTestPanel(string name, XYPair position, XYPair size) 
            : base(name, position, size, null) {

            shapes.Add(new Circle(Vector2.One * 50f, 15f));
            shapes.Add(new Square(Vector2.One * 50f + Vector2.UnitX * 100f, Vector2.One * 25f));
            shapes.Add(new Polygon(Vector2.One * 150f, 25));

            //sfp = new LateDrawRenderTarget();
        }

        int selected_simplex = 0;
        int selected_shape = 1;

        internal override void draw_rt() {
            if (!visible) return;
            base.draw_rt();

            foreach (Shape2D s in shapes) {                
                s.draw();
            }

            cursor.draw();

            if (selected_simplex > results[selected_shape].simplices.Count - 1) selected_simplex = results[selected_shape].simplices.Count - 1;

            if (GJK2D.SAVE_SIMPLICES) {
                results[selected_shape].simplices[selected_simplex].draw(size.ToVector2() / 2f);
                var selected = results[selected_shape].simplices[selected_simplex];
                (float u, float v, float w) b = (0f, 0f, 0f);
                
                if (selected.stage == GJK2D.gjk_simplex.simplex_stage.triangle)
                    b = Collision2D.triangle_barycentric(Vector2.Zero, selected.A, selected.B, selected.C);

                Drawing.text(
    $"[{selected_simplex}] {selected.iteration} {selected.stage.ToString()} {results[selected_shape].hit}\n{b.u}x{b.v}x{b.w}\n{results
    [selected_shape].distance}\n{results[selected_shape].penetration}", Vector2.One * 3f, Color.White);
            }
            foreach (GJK2D.gjk_result result in results) {
                result.draw();
            }
        }

        internal override void draw() {
            if (!visible) return;
            base.draw();
        }

        internal override void update() {
            if (!visible) return;

            if (GJK2D.SAVE_SIMPLICES) {
                if (Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.A)) {
                    selected_shape--;
                    if (selected_shape < 0) selected_shape = 0;
                }
                if (Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.D)) {
                    selected_shape++;
                    if (selected_shape > shapes.Count - 1) selected_shape = shapes.Count - 1;
                }

                if (Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.Q)) {
                    selected_simplex--;

                    if (selected_simplex < 0) selected_simplex = 0;
                }
                if (Swoop.input_handler.just_pressed(Microsoft.Xna.Framework.Input.Keys.E)) {
                    selected_simplex++;

                    if (selected_simplex > results[selected_shape].simplices.Count - 1) selected_simplex = results[selected_shape].simplices.Count - 1;
                }
            }

            if (mouse_over) {
                var sd = Swoop.input_handler.rawinput_mouse_state.ScrollDelta;

                if (Swoop.input_handler.is_pressed(MouseButtons.ScrollUp)) {
                    if (cursor.radius < 30)
                        cursor.radius += sd / 120;
                    else cursor.radius = 30;
                } else if (Swoop.input_handler.is_pressed(MouseButtons.ScrollDown)) {
                    if (cursor.radius > 5)
                        cursor.radius += sd / 120;
                    else cursor.radius = 5;
                }
                cursor.position = Input.cursor_pos.ToVector2() - position;
            }

            var p = Vector2.Zero;
            if (Swoop.input_handler.is_pressed(Keys.Up)) {
                p += -Vector2.UnitY;
            }
            if (Swoop.input_handler.is_pressed(Keys.Down)) {
                p += Vector2.UnitY;
            }
            if (Swoop.input_handler.is_pressed(Keys.Left)) {
                p += -Vector2.UnitX;
            }
            if (Swoop.input_handler.is_pressed(Keys.Right)) {
                p += Vector2.UnitX;
            }

            cursor.position += p;

            results.Clear();
            bool hit = false;
            for (int i = 0; i < shapes.Count; i++) {
                GJK2D.gjk_result res;
                if (GJK2D.intersects(shapes[i], cursor, out res))
                    hit = true;
                results.Add(res);
            }

            if (hit) cursor.debug_color = Color.Red;
            else cursor.debug_color = Color.White;
            base.update();
        }
    }
}
