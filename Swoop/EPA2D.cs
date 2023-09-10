using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static SwoopLib.Collision.GJK2D;
using static SwoopLib.Collision.GJK2D.gjk_simplex;

namespace SwoopLib.Collision {
    public class EPA2D {
        public struct epa_vert {
            public Vector2 P;
            public Vector2 support_A, support_B;

            public epa_vert(Vector2 support_A, Vector2 support_B) {
                this.support_A = support_A;
                this.support_B = support_B;
                P = support_A - support_B;
            }
            public epa_vert(Vector2 P, Vector2 support_A, Vector2 support_B) {
                this.support_A = support_A;
                this.support_B = support_B;
                this.P = P;
            }
        }
        public struct index_tri {
            public int A;
            public int B;
            public int C;

            public index_tri(int a, int b, int c, bool remove) {
                A = a;
                B = b;
                C = c;
            }
        }

        public class polytope {
            public List<epa_vert> points = new List<epa_vert>();

            public polytope(gjk_simplex simplex) {
                points.Add(new epa_vert(simplex.A_support.P, simplex.A_support.A, simplex.A_support.B));
                points.Add(new epa_vert(simplex.B_support.P, simplex.B_support.A, simplex.B_support.B));
                points.Add(new epa_vert(simplex.C_support.P, simplex.C_support.A, simplex.C_support.B));
            }
            
            void add_edge(epa_vert A, epa_vert B) { 

            }

            void try_add() {

            }
        }

        static (int index, epa_vert A, epa_vert B, Vector2 normal, float distance) closest_edge(ref polytope p) {
            float min = float.MaxValue;
            (int index, epa_vert A, epa_vert B, Vector2 normal, float distance) output = new();

            for (int i = 0; i < p.points.Count; i++) {
                var v = p.points[i];
                var v1 = p.points[(i+1) % p.points.Count];

                var d = v1.P - v.P;
                var n = Vector2.Normalize(Collision2D.triple_product(d, v.P, d));

                var dist = Vector2.Dot(n, v1.P);

                if (dist < min) { 
                    min = dist;
                    output.index = i;
                    output.A = v;
                    output.B = v1;
                    output.normal = n;
                    output.distance = dist;
                }
            }
            return output;
        }

        public static void expand_polytope(Shape2D shape_A, Shape2D shape_B, ref gjk_simplex simplex, ref gjk_result result) {
            var poly = new polytope(simplex);
            int iterations = 0;
            while (iterations < 10) {
                var c = closest_edge(ref poly);
                var sa = shape_A.support(c.normal);
                var sb = shape_B.support(-c.normal);
                var p = sa - sb;

                if ((float)Math.Abs(Vector2.Dot(c.normal, p) - c.distance) < 0.1f) {
                    result.polytope = poly;
                    result.penetration = c.distance;
                    return;
                }

                poly.points.Insert(c.index, new epa_vert(p, sa, sb));

                iterations++;
            }
        }
    }
}
