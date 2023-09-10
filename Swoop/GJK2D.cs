using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static SwoopLib.Collision.GJK2D.gjk_simplex;
using static SwoopLib.Collision.Collision2D;
using System.Net.Http.Headers;
using System.Xml.XPath;
using System.ComponentModel;
using static SwoopLib.Collision.EPA2D;

namespace SwoopLib.Collision {
    public interface Shape2D {
        public Vector2 position { get; set; }        
        public Color debug_color { get; set; }

        Vector2 support(Vector2 direction);
        void draw();
    }

    public static class GJK2D {
        public const bool SAVE_SIMPLICES = true;
        
        const float EPSILON = 0.00001f;

        const int MAX_ITERATIONS = 20;

        public struct gjk_result {
            public Shape2D? shape_A, shape_B;

            public Vector2 closest_A;
            public Vector2 closest_B;
            public Vector2 C;

            public bool hit = false;

            public float distance = float.MaxValue;
            public float penetration = 0;

            public polytope polytope;

            public gjk_simplex simplex;
            public List<gjk_simplex> simplices = new List<gjk_simplex>();

            public gjk_result() { }

            public void draw() {
                Drawing.line(closest_A, closest_B, Color.White, 1f);
                Drawing.fill_circle(closest_A, 2f, Color.HotPink);
                Drawing.fill_circle(closest_B, 2f, Color.HotPink);
            }
        }

        public struct gjk_support {
            public Vector2 A;
            public Vector2 B;

            public Vector2 P;

            public gjk_support() {
                A = Vector2.Zero;
                B = Vector2.Zero;
                P = Vector2.Zero;
            }

            public gjk_support(Vector2 a, Vector2 b, Vector2 p) {
                A = a;
                B = b;
                P = p;
            }
        }

        public struct gjk_simplex {
            public enum spoint { A=0, B=1, C=2 }
            public enum simplex_stage {
                empty = -1,
                point=0,
                line=1,
                triangle=2
            }
            public simplex_stage stage = simplex_stage.empty;

            public gjk_support[] supports = new gjk_support[3];

            internal int A_index => (int)stage;
            internal int B_index => (int)stage - 1;
            internal int C_index => (int)stage - 2;

            public Vector2 A => supports[A_index].P;
            public Vector2 B => supports[B_index].P;
            public Vector2 C => supports[C_index].P;

            public gjk_support A_support => supports[A_index];
            public gjk_support B_support => supports[B_index];
            public gjk_support C_support => supports[C_index];

            public Vector2 AO => -A;
            public Vector2 BO => -B;
            public Vector2 CO => -C;

            public Vector2 AB => B - A;
            public Vector2 AC => C - A;
            public Vector2 BC => C - B;
            public Vector2 CA => A - C;

            public Vector2 direction = Vector2.Zero;

            public int iteration = 0;

            public Vector2 closest_A;
            public Vector2 closest_B;

            public gjk_simplex() { }

            public gjk_simplex copy() {
                return new gjk_simplex() {
                    stage = stage,
                    supports = supports,
                    direction = direction,
                    iteration = iteration
                };
            }

            public void draw(Vector2 offset) {
                Drawing.fill_circle(offset, 3, Color.White);

                if (stage == simplex_stage.point) {
                    Drawing.line(A + offset, A + offset + (Vector2.Normalize(direction) * 5), Color.HotPink, 2f);

                } else if (stage == simplex_stage.line) {
                    var c = (A + B) / 2;
                    Drawing.poly(Color.White, 2f, true, A + offset, B + offset);

                    Drawing.fill_circle(offset + A, 2, Color.Red);
                    Drawing.fill_circle(offset + B, 2, Color.Green);

                    Drawing.line(c + offset, c + offset + (Vector2.Normalize(direction) * 5), Color.HotPink, 2f);

                } else if (stage == simplex_stage.triangle) {
                    var a = (A + B) / 2;
                    var b = (B + C) / 2;
                    var c = (C + A) / 2;

                    Drawing.poly(Color.White, 2f, true, A + offset, B + offset, C + offset);

                    Drawing.fill_circle(A + offset, 2, Color.Red);
                    Drawing.fill_circle(B + offset, 2, Color.Green);
                    Drawing.fill_circle(C + offset, 2, Color.Blue);

                    var tb = triangle_barycentric(Vector2.Zero, A,B,C);

                    var acc = offset + (A + C) / 2;
                    var ac_dir = Vector2.Normalize(triple_product(AB, BC, AC));

                    var abc = offset + (A + B) / 2;
                    var ab_dir = Vector2.Normalize(triple_product(BC, AC, AB));

                    if (Vector2.Dot(ab_dir, AO) > Vector2.Dot(ac_dir, AO)) {
                        Drawing.line(abc, abc + (ab_dir * 10f), Color.Red, 2f);
                    } else {
                        Drawing.line(acc, acc + (ac_dir * 10f), Color.Green, 2f);
                    }
                }
            }

            public void add_new_point(Vector2 A_sup, Vector2 B_sup) {
                if ((int)stage < (int)simplex_stage.triangle)
                    stage = (simplex_stage)((int)stage + 1);
                else
                    return;

                var P = A_sup - B_sup;
                supports[(int)stage] = new gjk_support(A_sup, B_sup, P);

            }

            int spoint_index(spoint p) {
                switch (p) {
                    case spoint.A: return A_index;
                    case spoint.B: return B_index;
                    case spoint.C: return C_index;
                }
                return -1;
            }

            public void move_to_stage(spoint A) {
                var s = new gjk_support[4];
                s[0] = supports[spoint_index(A)];
                supports = s;
                stage = simplex_stage.point;
            }

            public void move_to_stage(spoint A, spoint B) {
                var s = new gjk_support[4];
                s[1] = supports[spoint_index(A)];
                s[0] = supports[spoint_index(B)];

                supports = s;
                stage = simplex_stage.line;
            }

            public void move_to_stage(spoint A, spoint B, spoint C) {
                var s = new gjk_support[4];
                s[2] = supports[spoint_index(A)];
                s[1] = supports[spoint_index(B)];
                s[0] = supports[spoint_index(C)];

                supports = s;
                stage = simplex_stage.triangle;
            }

            public bool same_dir_as_AO(Vector2 v) => (Vector2.Dot(v, AO) >= 0);
        }

        public static bool intersects(Shape2D shape_A, Shape2D shape_B, out gjk_result result) {
            result = new gjk_result();

            if (shape_A == null || shape_B == null) { return false; }
            result.shape_A = shape_A; result.shape_B = shape_B;

            gjk_simplex simplex = new gjk_simplex();

            simplex.direction = Vector2.UnitX;
            simplex.add_new_point(shape_A.support(simplex.direction), shape_B.support(-simplex.direction));

            closest_point(ref simplex, ref result);

            if (SAVE_SIMPLICES)
                result.simplices.Add(simplex.copy());

            simplex.direction = simplex.AO;

            int iterations = 0;
            float l_dist = float.MaxValue;
            int dist_count = 0;

            while (iterations < MAX_ITERATIONS) {
                if (SAVE_SIMPLICES)
                    result.simplices.Add(simplex.copy());

                iterations++;

                simplex.add_new_point(shape_A.support(simplex.direction), shape_B.support(-simplex.direction));
                simplex.iteration = iterations;

                if (SAVE_SIMPLICES)
                    result.simplices.Add(simplex.copy());

                if (simplex.stage == gjk_simplex.simplex_stage.line) {
                    //exit if A and B are touching
                    if (Vector2.Distance(simplex.A, simplex.B) <= EPSILON) {
                        simplex.move_to_stage(spoint.B);
                        break;
                    }

                    //early exit if origin is on the A->B line
                    if (closest_point_on_line(simplex.A, simplex.B, Vector2.Zero).Length() <= EPSILON) {
                        result.hit = true;
                        break;
                    }

                    //origin can either be between A and B or back the way we came
                    if (simplex.same_dir_as_AO(simplex.AB)) {
                        if (simplex.same_dir_as_AO(perpendicular(simplex.AB))) {
                            simplex.direction = perpendicular(simplex.AB);
                        } else {
                            simplex.direction = perpendicular_inverse(simplex.AB);
                        }
                    } else {
                        simplex.direction = simplex.AO;
                    }

                } else if (simplex.stage == gjk_simplex.simplex_stage.triangle) {
                    //exit if A and B or A and C are basically touching
                    if (Vector2.Distance(simplex.A, simplex.B) <= EPSILON
                        || Vector2.Distance(simplex.A, simplex.C) <= EPSILON) {
                        simplex.move_to_stage(spoint.B, spoint.C);
                        break;
                    }

                    //exit if A is on BC
                    if (closest_point_on_line(simplex.C, simplex.B, simplex.A).Length() <= EPSILON) {
                        simplex.move_to_stage(spoint.B, spoint.C);
                        break;
                    }

                    var tb = triangle_barycentric(Vector2.Zero, simplex.A,simplex.B,simplex.C);

                    //just in case
                    if (float.IsInfinity(tb.u) || float.IsInfinity(tb.v) || float.IsInfinity(tb.w)) break;

                    //hit if origin is within triangle                    
                    if ((tb.u > 0 && tb.v > 0 && tb.w > 0) || (tb.u < 0 && tb.v < 0 && tb.w < 0)) {
                        result.hit = true;
                        break;

                    //otherwise find a new direction, either AB or AC, as A is always the newest, closest point  
                    } else {

                        var ac_dir = Vector2.Normalize(triple_product(simplex.AB, simplex.BC, simplex.AC));
                        var ab_dir = Vector2.Normalize(triple_product(simplex.BC, simplex.AC, simplex.AB));

                        //also just in case
                        if (ac_dir.contains_nan() || ab_dir.contains_nan()) {
                            break;
                        }

                        //move to AB or AC depending on which has the higher dot
                        if (Vector2.Dot(ab_dir, simplex.AO) >= Vector2.Dot(ac_dir, simplex.AO)) {
                            simplex.direction = ab_dir;
                            simplex.move_to_stage(spoint.A, spoint.B);
                        } else {
                            simplex.direction = ac_dir;
                            simplex.move_to_stage(spoint.A, spoint.C);
                        }
                    }
                }

                //do a closest point calc and see if the new point added this iteration has even moved us any closer
                closest_point(ref simplex, ref result);
                if (result.distance < l_dist) {
                    l_dist = result.distance;
                    dist_count = 0;

                //if the closest point hasn't changed, increase dist_count
                } else if (Math.Abs(result.distance - l_dist) <= EPSILON) {
                    dist_count++;
                }

                //been at the same distance for 2 steps in a row, so exit                        
                if (dist_count == 2) {
                    break;
                }
            }

            if (!result.hit) { 
                closest_point(ref simplex, ref result);
            } else {
                result.distance = 0;
                
                if (simplex.stage == simplex_stage.triangle)
                    EPA2D.expand_polytope(shape_A, shape_B, ref simplex, ref result);
            }

            if (SAVE_SIMPLICES)
                result.simplices.Add(simplex.copy());

            result.simplex = simplex;
            return result.hit;
        }

        static void closest_point(ref gjk_simplex simplex, ref gjk_result result) {
            Vector2 closest_A = Vector2.Zero, closest_B = Vector2.Zero;
            
            float d = float.MaxValue;

            switch (simplex.stage) {
                case simplex_stage.point:
                    closest_A = simplex.A_support.A;
                    closest_B = simplex.A_support.B;
                    break;

                case simplex_stage.line:
                    lines_closest_points(simplex.A_support.A, simplex.B_support.A, simplex.A_support.B, simplex.B_support.B,
                        out _, out _, out closest_A, out closest_B);
                    break;

                case simplex_stage.triangle:
                    var cp = closest_points_two_triangles(
                        (simplex.A_support.A, simplex.B_support.A, simplex.C_support.A),
                        (simplex.A_support.B, simplex.B_support.B, simplex.C_support.B)
                        );

                    closest_A = cp.A; closest_B = cp.B;
                    break;
            }

            d = Vector2.Distance(closest_A, closest_B);

            simplex.closest_A = closest_A;
            simplex.closest_B = closest_B;

            if (d < result.distance && d > 0) {
                result.distance = d;

                result.closest_A = closest_A;
                result.closest_B = closest_B;
            }
        }

    }
}
