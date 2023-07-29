using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SwoopLib.Collision {
    public static class Extensions {
        public static string ToXString(this Vector2 x) => $"{x.X.ToString("F2")}x{x.Y.ToString("F2")}";

        public static Vector2 X_only(this Vector2 v) => new Vector2(v.X, 0);
        public static Vector2 Y_only(this Vector2 v) => new Vector2(0, v.Y);

        public static float NextFloat(this Random r) => (float)r.NextDouble();
        public static float float_neg_one_to_one(this Random r) => (r.NextFloat() - 0.5f) * 2f;

        public static Vector2 NextVector2(this Random r) => new Vector2(r.NextFloat(), r.NextFloat());
        public static Vector2 Vector2_neg_one_to_one(this Random r) => new Vector2(r.float_neg_one_to_one(), r.float_neg_one_to_one());
        
    }

    public static class Collision2D {
        public static (float U, float V) line_barycentric(Vector2 P, Vector2 A, Vector2 B) {
            Vector2 AB = B - A;
            Vector2 AP = P - A;

            float ABAB = AB.X * AB.X + AB.Y * AB.Y;
            float ABAP = AB.X * AP.X + AB.Y * AP.Y;

            float u = ABAP / ABAB;
            float v = 1 - u;
            return (u, v);
        }

        public static (float u, float v, float w) triangle_barycentric(Vector2 P, Vector2 A, Vector2 B, Vector2 C) {
            Vector2 v0 = B - A;
            Vector2 v1 = C - A;
            Vector2 v2 = P - A;

            float f0 = Vector2.Dot(v0, v0);
            float f1 = Vector2.Dot(v0, v1);
            float f2 = Vector2.Dot(v1, v1);
            float f3 = Vector2.Dot(v2, v0);
            float f4 = Vector2.Dot(v2, v1);

            float denom = f0 * f2 - f1 * f1;
            (float u, float v, float w) output = (0f,0f,0f);

            output.v = (f2 * f3 - f1 * f4) / denom;
            output.u = (f0 * f4 - f1 * f3) / denom;
            output.w = 1.0f - output.u - output.v;

            return output;
        }

        public static Vector2 triple_product(Vector2 A, Vector2 B, Vector2 C) {
            var r = A.X * B.Y - A.Y * B.X;
            return new Vector2(-C.Y * r, C.X * r);
        }

        public static Vector3 triangle_closest_point_alternative(Vector3 A, Vector3 B, Vector3 C, Vector3 point) {
            Vector3 ab = B - A;
            Vector3 ac = C - A;
            Vector3 bc = C - B;

            float unom = Vector3.Dot(point - B, bc);
            float sdnom = Vector3.Dot(point - B, A - B);
            if (sdnom <= 0f && unom <= 0f) return B;
            float tdnom = Vector3.Dot(point - C, A - C);
            float udnom = Vector3.Dot(point - C, B - C);
            if (tdnom <= 0f && udnom <= 0f) return C;


            float snom = Vector3.Dot(point - A, ab);
            float tnom = Vector3.Dot(point - A, ac);

            Vector3 n = Vector3.Cross(ab, ac);
            float vc = Vector3.Dot(n, Vector3.Cross(A - point, B - point));
            if (vc <= 0f && snom >= 0f && sdnom >= 0f)
                return A + snom / (snom + sdnom) * ab;

            float va = Vector3.Dot(n, Vector3.Cross(B - point, C - point));

            if (va <= 0f && unom >= 0f && udnom >= 0f)
                return B + unom / (unom + udnom) * bc;

            float vb = Vector3.Dot(n, Vector3.Cross(C - point, A - point));

            if (vb <= 0f && tnom >= 0f && tdnom >= 0f)
                return A + tnom / (tnom + tdnom) * ac;

            float u = va / (va + vb + vc);
            float v = vb / (va + vb + vc);
            float w = 1.0f - u - v; // = vc / (va + vb + vc)

            return u * A + v * B + w * C;
        }

        public static bool v2_intersects_rect(Vector2 P, Vector2 min, Vector2 max)
            => (P.X >= min.X && P.X <= max.X && P.Y >= min.Y && P.Y <= max.Y);
        public static bool point_intersects_rect(Point P, Point min, Point max)
            => (P.X >= min.X && P.X <= max.X && P.Y >= min.Y && P.Y <= max.Y);
        public static bool point_intersects_rect(Point P, XYPair min, XYPair max)
            => (P.X >= min.X && P.X <= max.X && P.Y >= min.Y && P.Y <= max.Y);
        public static bool xypair_intersects_rect(XYPair P, XYPair min, XYPair max)
            => P.within_rect(min, max);

        public static bool point_inside_triangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
            float denom = (B.Y - C.Y) * (A.X - C.X) + (C.X - B.X) * (A.Y - C.Y);
            float a = ((B.Y - C.Y) * (P.X - C.X) + (C.X - B.X) * (A.Y - C.Y)) / denom;
            float b = ((C.Y - A.Y) * (P.X - C.X) + (A.X - C.X) * (P.Y - C.Y)) / denom;
            float c = 1 - a - b;

            if (a >= 0 && b >= 0 && c >= 0) {
                return true;
            }

            return false;
        }


        public static Vector2 highest_dot(Vector2 direction, params Vector2[] verts) {
            return highest_dot(verts, direction, out _);
        }
        public static Vector2 highest_dot(Vector2[] verts, Vector2 direction, out float dot) {
            dot = float.MinValue;
            Vector2 v = Vector2.Zero;

            for (int i = 0; i < verts.Length; i++) {
                float d = Vector2.Dot(direction, verts[i]);

                if (d > dot) {
                    dot = d;
                    v = verts[i];
                }
            }

            return v;
        }

        public static Vector2 perpendicular(Vector2 a) {
            return new Vector2(a.Y, -a.X);
        }

        public static Vector2 perpendicular_inverse(Vector2 a) {
            return new Vector2(-a.Y, a.X);
        }

        public static Vector2 closest_point_on_line(Vector2 a, Vector2 b, Vector2 p) {
            var ab = b - a;
            var t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return a + t * ab;
        }

        public static float lines_closest_points(Vector2 AA, Vector2 AB, Vector2 BA, Vector2 BB, out float s, out float t, out Vector2 P1, out Vector2 P2) {
            Vector2 d1 = AB - AA;
            Vector2 d2 = BB - BA;
            Vector2 r = AA - BA;

            float a = Vector2.Dot(d1, d1);
            float e = Vector2.Dot(d2, d2);
            float f = Vector2.Dot(d2, r);

            if (a <= float.Epsilon && e <= float.Epsilon) {
                s = t = 0.0f;
                P1 = AA;
                P2 = BA;
                return Vector2.Dot(P1, P2);
            }

            if (a <= float.Epsilon) {
                s = 0.0f;
                t = f / e;
                t = MathHelper.Clamp(t, 0.0f, 1.0f);
            } else {
                float c = Vector2.Dot(d1, r);
                if (e <= float.Epsilon) {
                    t = 0.0f;
                    s = MathHelper.Clamp(-c / a, 0f, 1f);
                } else {
                    float b = Vector2.Dot(d1, d2);
                    float denom = a * e - b * b;

                    if (denom != 0f) {
                        s = MathHelper.Clamp((b * f - c * e) / denom, 0f, 1f);
                    } else s = 0f;

                    t = (b * s + f) / e;

                    if (t < 0f) {
                        t = 0f;
                        s = MathHelper.Clamp(-c / a, 0f, 1f);
                    } else if (t > 1f) {
                        t = 1f;
                        s = MathHelper.Clamp((b - c) / a, 0f, 1f);
                    }
                }
            }

            P1 = AA + d1 * s;
            P2 = BA + d2 * t;

            return Vector2.Dot(P1 - P2, P1 - P2);
        }

        public static (Vector2 A, Vector2 B) closest_points_two_triangles((Vector2 A, Vector2 B, Vector2 C) triangle_A, (Vector2 A, Vector2 B, Vector2 C) triangle_B ) {
            float shortest_dist = float.MaxValue;
            Vector2 short_A = Vector2.Zero;
            Vector2 short_B = Vector2.Zero;

            Vector2 l_A, l_B;
            var d = lines_closest_points(triangle_A.A, triangle_A.B, triangle_B.A, triangle_B.B, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }
            d = lines_closest_points(triangle_A.A, triangle_A.B, triangle_B.B, triangle_B.C, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }
            d = lines_closest_points(triangle_A.A, triangle_A.B, triangle_B.C, triangle_B.A, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }

            d = lines_closest_points(triangle_A.B, triangle_A.C, triangle_B.A, triangle_B.B, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }
            d = lines_closest_points(triangle_A.B, triangle_A.C, triangle_B.B, triangle_B.C, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }
            d = lines_closest_points(triangle_A.C, triangle_A.C, triangle_B.C, triangle_B.A, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }

            d = lines_closest_points(triangle_A.C, triangle_A.A, triangle_B.A, triangle_B.B, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }
            d = lines_closest_points(triangle_A.C, triangle_A.A, triangle_B.B, triangle_B.C, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }
            d = lines_closest_points(triangle_A.C, triangle_A.A, triangle_B.C, triangle_B.A, out _, out _, out l_A, out l_B);
            if (d < shortest_dist) { shortest_dist = d; short_A = l_A; short_B = l_B; }

            return (short_A, short_B);
        }

        public static bool same_dir(Vector2 dir, Vector2 origin) {
            return Vector2.Dot(dir, origin) > 0;
        }
    }
}
